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

                int maxFortAttempts = 5;

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
                    catch (Exception ex)
                    {
                        if (response.IsEmpty)
                            LogCaller(new LoggerEventArgs("FortSearchResponse parsing failed because response was empty", LoggerTypes.Exception, ex));

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
                    else if (fortResponse.Result != FortSearchResponse.Types.Result.Success && fortResponse.Result != FortSearchResponse.Types.Result.InventoryFull)
                    {
                        LogCaller(new LoggerEventArgs(String.Format("Failed to search fort. Response: {0}", fortResponse.Result), LoggerTypes.Warning));

                        return new MethodResult
                        {
                            Message = "Failed to search fort"
                        };
                    }

                    string message = String.Format("Searched Fort. Exp: {0}. Items: {1}.",
                        fortResponse.ExperienceAwarded,
                        StringUtil.GetSummedFriendlyNameOfItemAwardList(fortResponse.ItemsAwarded.ToList()));

                    Dictionary<ItemId, ItemData> itemDictionary = new Dictionary<ItemId, ItemData>();

                    foreach(ItemData item in Items)
                    {
                        itemDictionary.Add(item.ItemId, item);
                    }

                    foreach(ItemAward item in fortResponse.ItemsAwarded)
                    {
                        if(itemDictionary.ContainsKey(item.ItemId))
                        {
                            itemDictionary[item.ItemId].Count += item.ItemCount;
                        }
                        else
                        {
                            Items.Add(new ItemData
                                {
                                    ItemId = item.ItemId,
                                    Unseen = true,
                                    Count = item.ItemCount
                                });
                        }
                    }

                    if (fortResponse.Result != FortSearchResponse.Types.Result.OutOfRange)
                    {
                        //Successfully grabbed stop
                        if(AccountState == Enums.AccountState.PokemonBanAndPokestopBanTemp || AccountState == Enums.AccountState.PokestopBanTemp)
                        {
                            if(AccountState == Enums.AccountState.PokemonBanAndPokestopBanTemp)
                            {
                                AccountState = Enums.AccountState.PokemonBanTemp;
                            }
                            else
                            {
                                AccountState = Enums.AccountState.Good;
                            }

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

                    if(fortResponse.ExperienceAwarded != 0 || fortResponse.Result == FortSearchResponse.Types.Result.OutOfRange)
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

                            if(UserSettings.StopAtMinAccountState == Enums.AccountState.PokemonBanTemp || 
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

                        break;
                    }

                    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                }
                
                if(fortResponse != null && fortResponse.ExperienceAwarded == 0)
                {
                    ++_totalZeroExpStops;

                    if(_totalZeroExpStops >= 15 || _fleeingPokemonResponses >= _fleeingPokemonUntilBan)
                    {
                        _totalZeroExpStops = 0;

                        LogCaller(new LoggerEventArgs("Potential softban detected. Attempting to bypass ...", LoggerTypes.Warning));

                        int totalAttempts = 0;
                        int maxAttempts = 40;

                        FortSearchResponse bypassResponse = null;

                        do
                        {
                            ++totalAttempts;

                            if(totalAttempts >= 5 && totalAttempts % 5 == 0)
                            {
                                LogCaller(new LoggerEventArgs(String.Format("Softban bypass attempt {0} of {1}", totalAttempts, maxAttempts), LoggerTypes.Info));
                            }

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
                                bypassResponse = FortSearchResponse.Parser.ParseFrom(response);
                            }
                            catch (Exception ex)
                            {
                                if (response.IsEmpty)
                                    LogCaller(new LoggerEventArgs("FortSearchResponse parsing failed because response was empty", LoggerTypes.Exception, ex));

                                return new MethodResult();
                            }

                            await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                        } while (bypassResponse.ExperienceAwarded == 0 && totalAttempts <= maxAttempts);

                        if(bypassResponse.ExperienceAwarded != 0)
                        {
                            //Fleeing pokemon was a softban, reset count
                            _fleeingPokemonResponses = 0;
                            _potentialPokemonBan = false;

                            string message = String.Format("Searched Fort. Exp: {0}. Items: {1}.",
                                                    bypassResponse.ExperienceAwarded,
                                                    StringUtil.GetSummedFriendlyNameOfItemAwardList(bypassResponse.ItemsAwarded.ToList()));

                            ExpIncrease(fortResponse.ExperienceAwarded);

                            Tracker.AddValues(0, 1);

                            //_expGained += fortResponse.ExperienceAwarded;

                            LogCaller(new LoggerEventArgs(message, LoggerTypes.Success));
                            LogCaller(new LoggerEventArgs("Softban removed", LoggerTypes.Success));
                        }
                        else
                        {
                            LogCaller(new LoggerEventArgs("Softban still active. Continuing ...", LoggerTypes.Info));
                        }
                    }
                }
                else
                {
                    _totalZeroExpStops = 0;
                }

                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                return new MethodResult
                {
                    Success = true,
                    Message = "Success"
                };
            }
            catch(Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to search fort", LoggerTypes.Exception, ex));

                return new MethodResult
                {
                    Message = "Failed to search fort"
                };
            }
        }
    }
}
