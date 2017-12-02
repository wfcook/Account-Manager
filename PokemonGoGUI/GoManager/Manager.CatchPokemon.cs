using Google.Protobuf;
using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using PokemonGoGUI.GoManager.Models;
using PokemonGoGUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        private async Task<MethodResult> CatchNeabyPokemon()
        {
            if(!UserSettings.CatchPokemon)
            {
                return new MethodResult
                {
                    Message = "Catching pokemon disabled"
                };
            }

            MethodResult<List<MapPokemon>> catchableResponse = await GetCatchablePokemon();

            if(!catchableResponse.Success)
            {
                return new MethodResult();
            }

            foreach(MapPokemon pokemon in catchableResponse.Data)
            {
                if(!PokemonWithinCatchSettings(pokemon))
                {
                    continue;
                }


                MethodResult<EncounterResponse> result = await EncounterPokemon(pokemon);

                if (!result.Success)
                {
                    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                    continue;
                }


                MethodResult catchResult = await CatchPokemon(result.Data, pokemon);

                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
            }

            return new MethodResult
            {
                Success = true
            };
        }

        private async Task<MethodResult> CatchLuredPokemon(FortData fortData)
        {
            if (fortData.LureInfo == null)
            {
                return new MethodResult
                {
                    Message = "No lure on pokestop",
                    Success = true
                };
            }

            if(!UserSettings.CatchPokemon)
            {
                return new MethodResult
                {
                    Message = "Catching pokemon disabled"
                };
            }

            if(fortData.LureInfo.ActivePokemonId == PokemonId.Missingno)
            {
                return new MethodResult
                {
                    Message = "No lured pokemon",
                    Success = true
                };
            }

            if (!PokemonWithinCatchSettings(fortData.LureInfo.ActivePokemonId))
            {
                return new MethodResult
                {
                    Success = true
                };
            }

            MethodResult catchResult = await CatchPokemon(fortData);

            await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

            return new MethodResult
            {
                Success = true
            };
        }

        //Catch lured pokemon
        private async Task<MethodResult> CatchPokemon(FortData fortData)
        {
            try
            {
                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.DiskEncounter,
                    RequestMessage = new DiskEncounterMessage
                    {
                        EncounterId = fortData.LureInfo.EncounterId,
                        FortId = fortData.Id,
                        PlayerLatitude = _client.ClientSession.Player.Latitude,
                        PlayerLongitude = _client.ClientSession.Player.Longitude
                    }.ToByteString()
                });

                DiskEncounterResponse eResponse = null;

                try
                {
                    eResponse = DiskEncounterResponse.Parser.ParseFrom(response);
                }
                catch (Exception ex)
                {
                    if (response.IsEmpty)
                        LogCaller(new LoggerEventArgs("DiskEncounterResponse failed because is empty", LoggerTypes.Exception, ex));

                    return new MethodResult();
                }

                if (eResponse.Result == DiskEncounterResponse.Types.Result.PokemonInventoryFull)
                {
                    LogCaller(new LoggerEventArgs("Encounter failed. Pokemon inventory full", LoggerTypes.Warning));

                    return new MethodResult
                    {
                        Message = "Encounter failed. Pokemon inventory full"
                    };
                }
                else if (eResponse.Result != DiskEncounterResponse.Types.Result.Success)
                {
                    if(eResponse.Result == DiskEncounterResponse.Types.Result.NotAvailable)
                    {
                        //Ignore
                        return new MethodResult
                        {
                            Message = "Encounter not available"
                        };
                    }

                    LogCaller(new LoggerEventArgs(String.Format("Lured encounter failed with response {0}", eResponse.Result), LoggerTypes.Warning));

                    return new MethodResult
                    {
                        Message = "Encounter failed"
                    };
                }

                CatchPokemonResponse catchPokemonResponse = null;
                int attemptCount = 1;

                do
                {
                    //Uses lowest capture probability
                    float probability = eResponse.CaptureProbability.CaptureProbability_[0];
                    ItemId pokeBall = await GetBestBall(eResponse.PokemonData);

                    if (pokeBall == ItemId.ItemUnknown)
                    {
                        LogCaller(new LoggerEventArgs("No pokeballs remaining (lure)", LoggerTypes.Warning));

                        return new MethodResult
                        {
                            Message = "No pokeballs remaining"
                        };
                    }

                    bool isLowProbability = probability < 0.35;
                    bool isHighCp = eResponse.PokemonData.Cp > 700;
                    bool isHighPerfection = CalculateIVPerfection(eResponse.PokemonData).Data > 90;

                    if ((isLowProbability && isHighCp) || isHighPerfection)
                    {
                        await UseBerry(fortData.LureInfo.EncounterId, fortData.Id);
                    }


                    double reticuleSize = 1.95;
                    bool hitInsideReticule = true;

                    //Humanization
                    if (UserSettings.EnableHumanization)
                    {
                        reticuleSize = (double)_rand.Next(10, 195) / 100;
                        hitInsideReticule = HitInsideReticle();
                    }

                    //End humanization

                    var catchresponse = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                    {
                        RequestType = RequestType.CatchPokemon,
                        RequestMessage = new CatchPokemonMessage
                        {
                            EncounterId = fortData.LureInfo.EncounterId,
                            HitPokemon = hitInsideReticule,
                            NormalizedHitPosition = 1,
                            NormalizedReticleSize = reticuleSize,
                            Pokeball = pokeBall,
                            SpawnPointId = fortData.Id,
                            SpinModifier = 1
                        }.ToByteString()
                    });

                    try
                    {
                        catchPokemonResponse = CatchPokemonResponse.Parser.ParseFrom(catchresponse);
                    }
                    catch (Exception ex)
                    {
                        if (catchresponse.IsEmpty)
                            LogCaller(new LoggerEventArgs("CatchPokemonResponse failed because is empty", LoggerTypes.Exception, ex));

                        return new MethodResult();
                    }

                    string pokemon = String.Format("Name: {0}, CP: {1}, IV: {2:0.00}%", fortData.LureInfo.ActivePokemonId, eResponse.PokemonData.Cp, CalculateIVPerfection(eResponse.PokemonData).Data);

                    if (catchPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess)
                    {
                        int expGained = catchPokemonResponse.CaptureAward.Xp.Sum();


                        Tracker.AddValues(1, 0);

                        ExpIncrease(expGained);

                        //_expGained += expGained;

                        LogCaller(new LoggerEventArgs(String.Format("Lured Pokemon Caught. {0}. Exp {1}. Attempt #{2}", pokemon, expGained, attemptCount), LoggerTypes.Success));

                        return new MethodResult
                        {
                            Message = "Pokemon caught",
                            Success = true
                        };
                    }
                    else if (catchPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchFlee)
                    {
                        LogCaller(new LoggerEventArgs(String.Format("Pokemon fled. {0}. Attempt #{1}", pokemon, attemptCount), LoggerTypes.PokemonFlee));
                    }
                    else if(catchPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape)
                    {
                        LogCaller(new LoggerEventArgs(String.Format("Escaped ball. {0}. Attempt #{1}.", pokemon, attemptCount), LoggerTypes.PokemonEscape));
                    }
                    else
                    {
                        LogCaller(new LoggerEventArgs(String.Format("Unknown Error. {0}. Attempt #{1}. Status: {2}", pokemon, attemptCount, catchPokemonResponse.Status), LoggerTypes.Warning));
                    }

                    ++attemptCount;

                    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                } while (catchPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed || catchPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape);
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to catch lured pokemon due to error", LoggerTypes.Exception, ex));

                return new MethodResult
                {
                    Message = "Failed to catch lured pokemon"
                };
            }

            return new MethodResult
            {
                Message = "Failed to catch lured pokemon",
                Success = true
            };
        }

        private async Task<MethodResult<EncounterResponse>> EncounterPokemon(MapPokemon mapPokemon)
        {
            try
            {
                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.Encounter,
                    RequestMessage = new EncounterMessage
                    {
                        EncounterId = mapPokemon.EncounterId,
                        PlayerLatitude = _client.ClientSession.Player.Latitude,
                        PlayerLongitude = _client.ClientSession.Player.Longitude,
                        SpawnPointId = mapPokemon.SpawnPointId
                    }.ToByteString()
                });

                EncounterResponse eResponse = null;

                try
                {
                    eResponse = EncounterResponse.Parser.ParseFrom(response);
                }
                catch (Exception ex)
                {
                    if (response.IsEmpty)
                        LogCaller(new LoggerEventArgs("EncounterResponse failed because is empty", LoggerTypes.Exception, ex));

                    return new MethodResult<EncounterResponse>();
                }

                if (eResponse.Status == EncounterResponse.Types.Status.PokemonInventoryFull)
                {
                    LogCaller(new LoggerEventArgs("Encounter failed. Pokemon inventory full", LoggerTypes.Warning));

                    return new MethodResult<EncounterResponse>
                    {
                        Message = "Encounter failed. Pokemon inventory full"
                    };
                }
                else if (eResponse.Status != EncounterResponse.Types.Status.EncounterSuccess)
                {
                    LogCaller(new LoggerEventArgs(String.Format("Encounter failed with response {0}", eResponse.Status), LoggerTypes.Warning));

                    return new MethodResult<EncounterResponse>
                    {
                        Message = "Encounter failed"
                    };
                }

                return new MethodResult<EncounterResponse>
                {
                    Data = eResponse,
                    Success = true,
                    Message = "Success"
                };
            }
            catch(Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to encounter pokemon due to error", LoggerTypes.Exception, ex));

                return new MethodResult<EncounterResponse>
                {
                    Message = "Failed to encounter pokemon"
                };
            }
        }

        //Catch encountered pokemon
        private async Task<MethodResult> CatchPokemon(EncounterResponse eResponse, MapPokemon mapPokemon)
        {
            try
            {
                CatchPokemonResponse catchPokemonResponse = null;
                int attemptCount = 1;

                do
                {
                    //Uses lowest capture probability
                    float probability = eResponse.CaptureProbability.CaptureProbability_[0];
                    ItemId pokeBall = await GetBestBall(eResponse.WildPokemon.PokemonData);

                    if (pokeBall == ItemId.ItemUnknown)
                    {
                        LogCaller(new LoggerEventArgs("No pokeballs remaining (encounter)", LoggerTypes.Warning));

                        return new MethodResult
                        {
                            Message = "No pokeballs remaining"
                        };
                    }

                    bool isLowProbability = probability < 0.40;
                    bool isHighCp = eResponse.WildPokemon.PokemonData.Cp > 800;
                    bool isHighPerfection = CalculateIVPerfection(eResponse.WildPokemon.PokemonData).Data > 95;

                    if((isLowProbability && isHighCp) || isHighPerfection)
                    {
                        await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                        await UseBerry(mapPokemon);
                    }

                    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));


                    double reticuleSize = 1.95;
                    bool hitInsideReticule = true;

                    //Humanization
                    if (UserSettings.EnableHumanization)
                    {
                        reticuleSize = (double)_rand.Next(10, 195) / 100;
                        hitInsideReticule = HitInsideReticle();
                    }

                    var catchresponse = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                    {
                        RequestType = RequestType.CatchPokemon,
                        RequestMessage = new CatchPokemonMessage
                        {
                            EncounterId = mapPokemon.EncounterId,
                            HitPokemon = hitInsideReticule,
                            NormalizedHitPosition = 1,
                            NormalizedReticleSize = reticuleSize,
                            Pokeball = pokeBall,
                            SpawnPointId = mapPokemon.SpawnPointId,
                            SpinModifier = 1
                        }.ToByteString()
                    });

                    try
                    {
                        catchPokemonResponse = CatchPokemonResponse.Parser.ParseFrom(catchresponse);
                    }
                    catch (Exception ex)
                    {
                        if (catchresponse.IsEmpty)
                            LogCaller(new LoggerEventArgs("CatchPokemonResponse failed because is empty", LoggerTypes.Exception, ex));

                        return new MethodResult();
                    }

                    string pokemon = String.Format("Name: {0}, CP: {1}, IV: {2:0.00}%", mapPokemon.PokemonId, eResponse.WildPokemon.PokemonData.Cp, CalculateIVPerfection(eResponse.WildPokemon.PokemonData).Data);
                    string pokeBallName = pokeBall.ToString().Replace("Item", "");

                    if(catchPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess)
                    {
                        //Reset data
                        _fleeingPokemonResponses = 0;
                        Tracker.AddValues(1, 0);
                        _potentialPokemonBan = false;

                        int expGained = catchPokemonResponse.CaptureAward.Xp.Sum();

                        ExpIncrease(expGained);

                        //_expGained += expGained;

                        LogCaller(new LoggerEventArgs(String.Format("Caught. {0}. Exp {1}. Attempt #{2}. Ball: {3}", pokemon, expGained, attemptCount, pokeBallName), LoggerTypes.Success));

                        return new MethodResult
                        {
                            Message = "Pokemon caught",
                            Success = true
                        };
                    }
                    else if (catchPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchFlee)
                    {
                        ++_fleeingPokemonResponses;

                        LogCaller(new LoggerEventArgs(String.Format("Pokemon fled. {0}. Attempt #{1}. Ball: {2}", pokemon, attemptCount, pokeBallName), LoggerTypes.PokemonFlee));
                    }
                    else if(catchPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape)
                    {
                        //If we get this response, means we're good
                        _fleeingPokemonResponses = 0;
                        _potentialPokemonBan = false;

                        if(AccountState == Enums.AccountState.PokemonBanAndPokestopBanTemp || AccountState == Enums.AccountState.PokemonBanTemp)
                        {
                            if(AccountState == Enums.AccountState.PokemonBanAndPokestopBanTemp)
                            {
                                AccountState = Enums.AccountState.PokestopBanTemp;
                            }
                            else
                            {
                                AccountState = Enums.AccountState.Good;
                            }

                            LogCaller(new LoggerEventArgs("Pokemon ban was lifted", LoggerTypes.Info));
                        }

                        LogCaller(new LoggerEventArgs(String.Format("Escaped ball. {0}. Attempt #{1}. Ball: {2}", pokemon, attemptCount, pokeBallName), LoggerTypes.PokemonEscape));
                    }
                    else
                    {
                        LogCaller(new LoggerEventArgs(String.Format("Unknown Error. {0}. Attempt #{1}. Status: {2}", pokemon, attemptCount, catchPokemonResponse.Status), LoggerTypes.Warning));
                    }

                    ++attemptCount;

                    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                } while (catchPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed || catchPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape);
            }
            catch(Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to catch pokemon due to error", LoggerTypes.Exception, ex));

                return new MethodResult
                {
                    Message = "Failed to catch pokemon"
                };
            }

            return new MethodResult
            {
                Message = "Failed to catch pokemon",
                Success = true
            };
        }

        private bool PokemonWithinCatchSettings(PokemonId pokemondId, bool isSnipe = false)
        {
            CatchSetting catchSettings = UserSettings.CatchSettings.FirstOrDefault(x => x.Id == pokemondId);

            if(catchSettings == null)
            {
                LogCaller(new LoggerEventArgs(String.Format("Failed to find catch setting for {0}. Attempting to catch", pokemondId), LoggerTypes.Warning));

                return true;
            }

            if (isSnipe)
            {
                return catchSettings.Snipe;
            }

            if (!catchSettings.Catch)
            {
                LogCaller(new LoggerEventArgs(String.Format("Skipping catching {0}", pokemondId), LoggerTypes.Info));
            }

            return catchSettings.Catch;
        }

        private bool PokemonWithinCatchSettings(MapPokemon pokemon)
        {
            CatchSetting catchSettings = UserSettings.CatchSettings.FirstOrDefault(x => x.Id == pokemon.PokemonId);

            if(catchSettings == null)
            {
                LogCaller(new LoggerEventArgs(String.Format("Failed to find catch setting for {0}. Attempting to catch", pokemon.PokemonId), LoggerTypes.Warning));

                return true;
            }

            if(!catchSettings.Catch)
            {
                LogCaller(new LoggerEventArgs(String.Format("Skipping catching {0}", pokemon.PokemonId), LoggerTypes.Info));
            }

            return catchSettings.Catch;
        }

        private async Task<ItemId> GetBestBall(PokemonData pokemonData)
        {
            if(Items == null)
            {
                LogCaller(new LoggerEventArgs("Item list is empty ... Regrabbing", LoggerTypes.Debug));

                MethodResult result = await UpdateInventory();

                if(!result.Success)
                {
                    return ItemId.ItemUnknown;
                }
            }

            int pokemonCp = pokemonData.Cp;
            //double ivPercent = CalculateIVPerfection(encounter.WildPokemon.PokemonData).Data;

            ItemData pokeBalls = Items.FirstOrDefault(x => x.ItemId == ItemId.ItemPokeBall);
            ItemData greatBalls = Items.FirstOrDefault(x => x.ItemId == ItemId.ItemGreatBall);
            ItemData ultraBalls = Items.FirstOrDefault(x => x.ItemId == ItemId.ItemUltraBall);
            ItemData masterBalls = Items.FirstOrDefault(x => x.ItemId == ItemId.ItemMasterBall);

            if (masterBalls != null && masterBalls.Count > 0 && pokemonCp >= 1200)
            {
                masterBalls.Count--;

                return ItemId.ItemMasterBall;
            }

            if (ultraBalls != null && ultraBalls.Count > 0 && pokemonCp >= 750)
            {
                ultraBalls.Count--;

                return ItemId.ItemUltraBall;
            }

            if (greatBalls != null && greatBalls.Count > 0 && pokemonCp >= 1000)
            {
                greatBalls.Count--;

                return ItemId.ItemGreatBall;
            }

            if (pokeBalls != null && pokeBalls.Count > 0)
            {
                pokeBalls.Count--;

                return ItemId.ItemPokeBall;
            }

            if (greatBalls != null && greatBalls.Count > 0)
            {
                greatBalls.Count--;

                return ItemId.ItemGreatBall;
            }

            if (ultraBalls != null && ultraBalls.Count > 0)
            {
                ultraBalls.Count--;

                return ItemId.ItemUltraBall;
            }

            if (masterBalls != null && masterBalls.Count > 0)
            {
                masterBalls.Count--;

                return ItemId.ItemMasterBall;
            }

            return ItemId.ItemUnknown;
        }

        private async Task UseBerry(ulong encounterId, string spawnId)
        {
            ItemData berryData = Items.Where(x => x.ItemId == ItemId.ItemRazzBerry).FirstOrDefault();

            if (berryData == null || berryData.Count <= 0)
            {
                return;
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.UseItemCapture,
                RequestMessage = new UseItemCaptureMessage
                {
                    EncounterId = encounterId,
                    ItemId = ItemId.ItemRazzBerry,
                    SpawnPointId = spawnId
                }.ToByteString()
            });

            UseItemCaptureResponse useItemCaptureResponse = null;

            try
            {
                useItemCaptureResponse = UseItemCaptureResponse.Parser.ParseFrom(response);
                int remaining = berryData.Count - 1;
                berryData.Count = remaining;
                LogCaller(new LoggerEventArgs(String.Format("Successfully used berry. Remaining: {0}", remaining), LoggerTypes.Info));
            }
            catch (Exception)
            {
                LogCaller(new LoggerEventArgs(String.Format("Failed to use berry. Remaining: {0}", berryData.Count), LoggerTypes.Warning));
            }

            await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
        }

        private async Task UseBerry(MapPokemon pokemon)
        {
            await UseBerry(pokemon.EncounterId, pokemon.SpawnPointId);
        }

        private bool HitInsideReticle()
        {
            lock(_rand)
            {
                if (_rand.Next(1, 101) <= UserSettings.InsideReticuleChance)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
