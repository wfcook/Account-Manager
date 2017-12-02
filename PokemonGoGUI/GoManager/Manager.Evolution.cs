using Google.Protobuf;
using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings.Master;
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
        private async Task<MethodResult> EvolveFilteredPokemon()
        {
            MethodResult<List<PokemonData>> response = await GetPokemonToEvolve();

            if(response.Data.Count == 0)
            {
                return new MethodResult();
            }

            LogCaller(new LoggerEventArgs(String.Format("{0} pokemon to evolve", response.Data.Count), LoggerTypes.Info));
            
            if (response.Data.Count < UserSettings.MinPokemonBeforeEvolve && !LuckyEggActive && FilledPokemonInventorySpace() <= UserSettings.ForceEvolveAbovePercent)
            {
                LogCaller(new LoggerEventArgs(String.Format("Not enough pokemon to evolve. {0} of {1} evolvable pokemon", response.Data.Count, UserSettings.MinPokemonBeforeEvolve), LoggerTypes.Info));

                return new MethodResult
                {
                    Message = "Success",
                    Success = true
                };
            }

            if (!LuckyEggActive)
            {
                if (UserSettings.UseLuckyEgg)
                {
                    MethodResult result = await UseLuckyEgg();

                    if (!result.Success)
                    {
                        LogCaller(new LoggerEventArgs("Failed to use lucky egg. Possibly already active. Continuing evolving", LoggerTypes.Info));
                    }
                }
            }

            await EvolvePokemon(response.Data);

            return new MethodResult
            {
                Success = true,
                Message = "Success"
            };
        }

        public async Task<MethodResult> EvolvePokemon(IEnumerable<PokemonData> pokemonToEvolve)
        {
            //Shouldn't happen
            if(pokemonToEvolve == null)
            {
                LogCaller(new LoggerEventArgs("Null value sent to evolve pokemon", LoggerTypes.Debug));

                return new MethodResult();
            }

            foreach (PokemonData pokemon in pokemonToEvolve)
            {
                if(pokemon == null)
                {
                    LogCaller(new LoggerEventArgs("Null pokemon data in IEnumerable", LoggerTypes.Debug));

                    continue;
                }

                try
                {
                    var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                    {
                        RequestType = RequestType.EvolvePokemon,
                        RequestMessage = new EvolvePokemonMessage
                        {
                            PokemonId = pokemon.Id
                        }.ToByteString()
                    });

                    EvolvePokemonResponse evolvePokemonResponse = null;

                    try
                    {
                        evolvePokemonResponse = EvolvePokemonResponse.Parser.ParseFrom(response);
                        ExpIncrease(evolvePokemonResponse.ExperienceAwarded);
                        //_expGained += evolveResponse.ExperienceAwarded;

                        LogCaller(new LoggerEventArgs(
                            String.Format("Successully evolved {0}. Experience: {1}. Cp: {2} -> {3}. IV: {4:0.00}%",
                                        pokemon.PokemonId,
                                        evolvePokemonResponse.ExperienceAwarded,
                                        pokemon.Cp,
                                        evolvePokemonResponse.EvolvedPokemonData.Cp,
                                        CalculateIVPerfection(evolvePokemonResponse.EvolvedPokemonData).Data),
                                        LoggerTypes.Evolve));                    

                    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                    return new MethodResult
                        {
                            Success = true
                        };
                    }
                    catch (Exception ex)
                    {
                        if (response.IsEmpty)
                            LogCaller(new LoggerEventArgs("EvolvePokemonResponse parsing failed because response was empty", LoggerTypes.Exception, ex));

                        return new MethodResult();
                    }
                }
                catch(Exception ex)
                {
                    LogCaller(new LoggerEventArgs("Evolve request failed", LoggerTypes.Exception, ex));
                    return new MethodResult();
                }
            }

            return new MethodResult
            {
                Success = true
            };
        }

        private async Task<MethodResult<int>> GetEvolutionCandy(PokemonId pokemonId)
        {
            if(PokeSettings == null)
            {
                MethodResult result = await GetItemTemplates();

                if(!result.Success)
                {
                    return (MethodResult<int>)result;
                }
            }

            MethodResult<PokemonSettings> settingsResult = GetPokemonSetting(pokemonId);

            if(!settingsResult.Success)
            {
                return new MethodResult<int>
                {
                    Message = settingsResult.Message
                };
            }

            return new MethodResult<int>
            {
                Data = settingsResult.Data.CandyToEvolve,
                Message = "Success",
                Success = true
            };
        }

        private async Task<MethodResult<List<PokemonData>>> GetPokemonToEvolve()
        {
            if(!UserSettings.EvolvePokemon)
            {
                LogCaller(new LoggerEventArgs("Evolving disabled", LoggerTypes.Info));

                return new MethodResult<List<PokemonData>>
                {
                    Data = new List<PokemonData>(),
                    Message = "Evolving disabled",
                    Success = true
                };
            }

            await UpdatePokemon(false);
            await UpdatePokemonCandy(false);

            MethodResult result = await GetItemTemplates();

            if(!result.Success)
            {
                LogCaller(new LoggerEventArgs("Failed to grab pokemon settings. Skipping evolution", LoggerTypes.Warning));

                return new MethodResult<List<PokemonData>>
                {
                    Data = new List<PokemonData>(),
                    Success = true
                };
            }

            List<PokemonData> pokemonToEvolve = new List<PokemonData>();

            IEnumerable<IGrouping<PokemonId, PokemonData>> groupedPokemon = Pokemon.OrderByDescending(x => x.PokemonId).GroupBy(x => x.PokemonId);

            foreach(IGrouping<PokemonId, PokemonData> group in groupedPokemon)
            {
                EvolveSetting evolveSetting = UserSettings.EvolveSettings.FirstOrDefault(x => x.Id == group.Key);

                if(evolveSetting == null)
                {
                    LogCaller(new LoggerEventArgs(String.Format("Failed to find evolve settings for pokemon {0}", group.Key), LoggerTypes.Info));

                    continue;
                }

                if(!evolveSetting.Evolve)
                {
                    //Don't evolve
                    continue;
                }

                if (!PokeSettings.TryGetValue(group.Key, out PokemonSettings setting))
                {
                    LogCaller(new LoggerEventArgs(String.Format("Failed to find settings for pokemon {0}", group.Key), LoggerTypes.Info));

                    continue;
                }

                if (setting.EvolutionIds.Count == 0)
                {
                    //Pokemon can't evolve
                    continue;
                }

                Candy pokemonCandy = PokemonCandy.FirstOrDefault(x => x.FamilyId == setting.FamilyId);
                List<PokemonData> pokemonGroupToEvolve = group.Where(x => x.Cp >= evolveSetting.MinCP).OrderByDescending(x => CalculateIVPerfection(x).Data).ToList();

                if(pokemonCandy == null)
                {
                    LogCaller(new LoggerEventArgs(String.Format("No candy found for pokemon {0}", group.Key), LoggerTypes.Info));

                    continue;
                }

                int candyToEvolve = setting.CandyToEvolve;
                int totalPokemon = pokemonGroupToEvolve.Count;
                int totalCandy = pokemonCandy.Candy_;

                int maxPokemon = totalCandy / candyToEvolve;

                foreach(PokemonData pData in pokemonGroupToEvolve.Take(maxPokemon))
                {
                    pokemonToEvolve.Add(pData);
                }
            }

            return new MethodResult<List<PokemonData>>
            {
                Data = pokemonToEvolve,
                Message = "Success",
                Success = true
            };
        }

        private async Task<MethodResult> UseLuckyEgg()
        {
            if(LuckyEggActive)
            {
                return new MethodResult
                {
                    Success = true
                };
            }

            ItemData data = Items.FirstOrDefault(x => x.ItemId == POGOProtos.Inventory.Item.ItemId.ItemLuckyEgg);

            if(data == null || data.Count == 0)
            {
                LogCaller(new LoggerEventArgs("No lucky eggs left", LoggerTypes.Info));

                return new MethodResult
                {
                    Message = "No lucky eggs"
                };
            }

            try
            {
                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.UseItemXpBoost,
                    RequestMessage = new UseItemXpBoostMessage
                    {
                       ItemId = ItemId.ItemLuckyEgg
                    }.ToByteString()
                });

                UseItemXpBoostResponse useItemXpBoostResponse = null;

                try
                {
                    useItemXpBoostResponse = UseItemXpBoostResponse.Parser.ParseFrom(response);
                }
                catch (Exception ex)
                {
                    if (response.IsEmpty)
                        LogCaller(new LoggerEventArgs("UseItemXpBoostResponse parsing failed because response was empty", LoggerTypes.Exception, ex));

                    return new MethodResult();
                }

                if (useItemXpBoostResponse.Result == UseItemXpBoostResponse.Types.Result.Success)
                {
                    LastLuckyEgg = DateTime.Now;

                    LogCaller(new LoggerEventArgs(String.Format("Lucky egg used. Remaining: {0}", data.Count - 1), LoggerTypes.Info));

                    return new MethodResult
                    {
                        Success = true
                    };
                }
                else if (useItemXpBoostResponse.Result == UseItemXpBoostResponse.Types.Result.ErrorNoItemsRemaining)
                {
                    LogCaller(new LoggerEventArgs("No lucky eggs left", LoggerTypes.Info));

                    return new MethodResult
                    {
                        Message = "No lucky eggs",
                        Success = true
                    };
                }
                else if (useItemXpBoostResponse.Result == UseItemXpBoostResponse.Types.Result.ErrorXpBoostAlreadyActive)
                {
                    LogCaller(new LoggerEventArgs("Lucky egg already active", LoggerTypes.Info));

                    return new MethodResult
                    {
                        Message = "Lucky egg already active",
                        Success = true
                    };
                }
                else
                {
                    LogCaller(new LoggerEventArgs(String.Format("Unknown response from lucky egg request. Response: {0}", useItemXpBoostResponse.Result), LoggerTypes.Info));

                    return new MethodResult
                    {
                        Message = "Unknown response from lucky egg request"
                    };
                }
            }
            catch(Exception ex)
            {
                LogCaller(new LoggerEventArgs("Lucky egg request failed", LoggerTypes.Exception, ex));

                return new MethodResult
                {
                    Message = "Lucky egg request failed"
                };
            }
        }

        public double FilledPokemonInventorySpace()
        {
            if (Pokemon == null || PlayerData == null)
            {
                return 0;
            }

            return (double)(Pokemon.Count + Eggs.Count) / PlayerData.MaxPokemonStorage * 100;
        }
    }
}
