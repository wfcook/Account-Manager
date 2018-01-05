using Google.Protobuf;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        private async Task<MethodResult> SearchPokestop(FortData pokestop)
        {
            try
            {
                FortSearchResponse fortResponse = null;
                const int maxFortAttempts = 5;

                for (int i = 0; i < maxFortAttempts; i++)
                {
                    var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                    {
                        RequestType = RequestType.FortSearch,
                        RequestMessage = new FortSearchMessage
                        {
                            FortId = pokestop.Id,
                            FortLatitude = pokestop.Latitude,
                            FortLongitude = pokestop.Longitude,
                            PlayerLatitude = _client.ClientSession.Player.Latitude,
                            PlayerLongitude = _client.ClientSession.Player.Longitude
                        }.ToByteString()
                    });

                    try
                    {
                        fortResponse = FortSearchResponse.Parser.ParseFrom(response);
                    }
                    catch (Exception)
                    {
                        return new MethodResult();
                    }

                    if (fortResponse.Result == FortSearchResponse.Types.Result.OutOfRange)
                    {
                        if (_potentialPokeStopBan)
                        {
                            if (AccountState != Enums.AccountState.PokestopBanTemp && AccountState != Enums.AccountState.PokemonBanAndPokestopBanTemp)
                            {
                                LogCaller(new LoggerEventArgs("Pokestop ban detected. Marking state", LoggerTypes.Warning));
                            }

                            //Already pokemon banned
                            if (AccountState == Enums.AccountState.PokemonBanTemp || AccountState == Enums.AccountState.PokemonBanAndPokestopBanTemp)
                            {
                                AccountState = Enums.AccountState.PokemonBanAndPokestopBanTemp;
                            }
                            else
                            {
                                AccountState = Enums.AccountState.PokestopBanTemp;
                            }

                            //Check for auto stop bot
                            if ((UserSettings.StopAtMinAccountState == Enums.AccountState.PokestopBanTemp ||
                                UserSettings.StopAtMinAccountState == Enums.AccountState.PokemonBanOrPokestopBanTemp) ||
                                (UserSettings.StopAtMinAccountState == Enums.AccountState.PokemonBanAndPokestopBanTemp && AccountState == Enums.AccountState.PokemonBanAndPokestopBanTemp))
                            {
                                LogCaller(new LoggerEventArgs("Auto stopping bot ...", LoggerTypes.Info));

                                Stop();
                            }
                        }
                        else //This error should never happen normally, so assume temp ban
                        {
                            _potentialPokeStopBan = true;
                            _proxyIssue = true;
                            //Display error only on first notice
                            LogCaller(new LoggerEventArgs("Pokestop out of range. Potential temp pokestop ban or IP ban", LoggerTypes.Warning));
                        }

                        //Let it continue down
                    }
                    else if (fortResponse.Result != FortSearchResponse.Types.Result.Success)
                    {
                        LogCaller(new LoggerEventArgs(String.Format("Failed to search fort. Response: {0}", fortResponse.Result), LoggerTypes.Warning));
                        return new MethodResult
                        {
                            Message = "Failed to search fort"
                        };
                    }

                    string message = String.Format("Searched Fort. Exp: {0}. Items: {1}.", // Badge: {2}. BonusLoot: {3}. Gems: {4}. Loot: {5}, Eggs: {6:0.0}. RaidTickets: {7}. TeamBonusLoot: {8}",
                            fortResponse.ExperienceAwarded,
                            StringUtil.GetSummedFriendlyNameOfItemAwardList(fortResponse.ItemsAwarded.ToList())
                            /*,
                            fortResponse.AwardedGymBadge.ToString(),
                            fortResponse.BonusLoot.LootItem.ToString(),
                            fortResponse.GemsAwarded.ToString(),
                            fortResponse.Loot.LootItem.ToString(),
                            fortResponse.PokemonDataEgg.EggKmWalkedStart,
                            fortResponse.RaidTickets.ToString(),
                            fortResponse.TeamBonusLoot.LootItem.ToString()*/);

                    var itemDictionary = new Dictionary<ItemId, ItemData>();

                   
                    if (fortResponse.Result != FortSearchResponse.Types.Result.OutOfRange)
                    {
                        //Successfully grabbed stop
                        
                        UpdateInventory(); // <- should not be needed
                        if (AccountState == Enums.AccountState.PokemonBanAndPokestopBanTemp || AccountState == Enums.AccountState.PokestopBanTemp)
                        {
                            AccountState = AccountState == Enums.AccountState.PokemonBanAndPokestopBanTemp ? Enums.AccountState.PokemonBanTemp : Enums.AccountState.Good;

                            LogCaller(new LoggerEventArgs("Pokestop ban was removed", LoggerTypes.Info));
                        }

                        ExpIncrease(fortResponse.ExperienceAwarded);
                        TotalPokeStopExp += fortResponse.ExperienceAwarded;

                        Tracker.AddValues(0, 1);

                        if (fortResponse.ExperienceAwarded == 0)
                        {
                            //Softban on the fleeing pokemon. Reset.
                            _fleeingPokemonResponses = 0;
                            _potentialPokemonBan = false;

                            ++_totalZeroExpStops;
                            message += String.Format(" No exp gained. Attempt {0} of {1}", i + 1, maxFortAttempts);
                        }

                        LogCaller(new LoggerEventArgs(message, LoggerTypes.Success));
                    }

                    if (fortResponse.ExperienceAwarded != 0 || fortResponse.Result == FortSearchResponse.Types.Result.OutOfRange)
                    {
                        if (!_potentialPokemonBan && _fleeingPokemonResponses >= _fleeingPokemonUntilBan)
                        {
                            LogCaller(new LoggerEventArgs("Potential pokemon ban detected. Setting flee count to 0 avoid false positives", LoggerTypes.Warning));

                            _potentialPokemonBan = true;
                            _fleeingPokemonResponses = 0;
                        }
                        else if (_fleeingPokemonResponses >= _fleeingPokemonUntilBan)
                        {
                            //Already pokestop banned
                            if (AccountState == Enums.AccountState.PokestopBanTemp || AccountState == Enums.AccountState.PokemonBanAndPokestopBanTemp)
                            {
                                _potentialPokemonBan = false;
                                AccountState = Enums.AccountState.PokemonBanAndPokestopBanTemp;
                            }
                            else
                            {
                                _potentialPokemonBan = false;
                                AccountState = Enums.AccountState.PokemonBanTemp;
                            }

                            if (AccountState != Enums.AccountState.PokemonBanTemp)
                            {
                                //Only occurs when out of range is found
                                if (fortResponse.ExperienceAwarded == 0)
                                {
                                    LogCaller(new LoggerEventArgs("Pokemon fleeing and failing to grab stops. Potential pokemon & pokestop ban.", LoggerTypes.Warning));
                                }
                                else
                                {
                                    LogCaller(new LoggerEventArgs("Pokemon fleeing, yet grabbing stops. Potential pokemon ban.", LoggerTypes.Warning));
                                }
                            }

                            if (UserSettings.StopAtMinAccountState == Enums.AccountState.PokemonBanTemp ||
                                UserSettings.StopAtMinAccountState == Enums.AccountState.PokemonBanOrPokestopBanTemp ||
                                (UserSettings.StopAtMinAccountState == Enums.AccountState.PokemonBanAndPokestopBanTemp && AccountState == Enums.AccountState.PokemonBanAndPokestopBanTemp))
                            {
                                LogCaller(new LoggerEventArgs("Auto stopping bot ...", LoggerTypes.Info));

                                Stop();
                            }

                            return new MethodResult
                            {
                                Message = "Bans detected",
                                Success = true
                            };
                        }

                    }

                    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                    if (fortResponse.Result == FortSearchResponse.Types.Result.Success)
                    {
                        return new MethodResult
                        {
                            Success = true,
                            Message = "Success"
                        };
                    }

                }

                return new MethodResult
                {
                    Success = true,
                    Message = "Success"
                };
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to search fort", LoggerTypes.Exception, ex));

                return new MethodResult
                {
                    Message = "Failed to search fort"
                };
            }
        }

        private async Task<MethodResult<FortDetailsResponse>> FortDetails(FortData pokestop)
        {
            try
            {
                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.FortDetails,
                    RequestMessage = new FortDetailsMessage
                    {
                        FortId = pokestop.Id,
                        Latitude = pokestop.Latitude,
                        Longitude = pokestop.Longitude,
                    }.ToByteString()
                });

                var fortDetailsResponse = FortDetailsResponse.Parser.ParseFrom(response);

                return new MethodResult<FortDetailsResponse>
                {
                    Data = fortDetailsResponse,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed getting fort info", LoggerTypes.Exception, ex));

                return new MethodResult<FortDetailsResponse>
                {
                    Data = new FortDetailsResponse(),
                    Message = "Failed getting fort info"
                };
            }
        }

        private async Task<MethodResult<GymGetInfoResponse>> GymGetInfo(FortData pokestop)
        {
           try
            {
                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.GymGetInfo,
                    RequestMessage = new GymGetInfoMessage
                    {
                        GymId = pokestop.Id,
                        GymLatDegrees = pokestop.Latitude,
                        GymLngDegrees = pokestop.Longitude,
                        PlayerLatDegrees = _client.ClientSession.Player.Latitude,
                        PlayerLngDegrees = _client.ClientSession.Player.Longitude
                    }.ToByteString()
                });

                var gymGetInfoResponse = GymGetInfoResponse.Parser.ParseFrom(response);

                return new MethodResult<GymGetInfoResponse>
                {
                    Data = gymGetInfoResponse,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed getting gym info", LoggerTypes.Exception, ex));

                return new MethodResult<GymGetInfoResponse>
                {
                    Data = new GymGetInfoResponse { Result = GymGetInfoResponse.Types.Result.Unset },
                    Message = "Failed getting gym info"
                };
            }
        }
    }
}
