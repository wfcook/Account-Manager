using POGOProtos.Map.Fort;
using POGOProtos.Networking.Responses;
using PokemonGo.RocketAPI;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager.Models;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
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
                    fortResponse = await _client.Fort.SearchFort(pokestop.Id, pokestop.Latitude, pokestop.Longitude);

                    if (fortResponse.Result != FortSearchResponse.Types.Result.Success && fortResponse.Result != FortSearchResponse.Types.Result.InventoryFull)
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


                    ExpIncrease(fortResponse.ExperienceAwarded);
                    //_expGained += fortResponse.ExperienceAwarded;

                    if(fortResponse.ExperienceAwarded == 0)
                    {
                        ++_totalZeroExpStops;
                        message += String.Format(" No exp gained. Attempt {0} of {1}", i + 1, maxFortAttempts);
                    }

                    LogCaller(new LoggerEventArgs(message, LoggerTypes.Success));

                    if(fortResponse.ExperienceAwarded != 0)
                    {
                        break;
                    }

                    await Task.Delay(500);
                }
                
                if(fortResponse != null && fortResponse.ExperienceAwarded == 0)
                {
                    ++_totalZeroExpStops;

                    if(_totalZeroExpStops >= 15)
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

                            bypassResponse = await _client.Fort.SearchFort(pokestop.Id, pokestop.Latitude, pokestop.Longitude);

                            await Task.Delay(300);
                        } while (bypassResponse.ExperienceAwarded == 0 && totalAttempts <= maxAttempts);

                        if(bypassResponse.ExperienceAwarded != 0)
                        {
                            string message = String.Format("Searched Fort. Exp: {0}. Items: {1}.",
                                                    bypassResponse.ExperienceAwarded,
                                                    StringUtil.GetSummedFriendlyNameOfItemAwardList(bypassResponse.ItemsAwarded.ToList()));

                            ExpIncrease(fortResponse.ExperienceAwarded);

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

                await Task.Delay(500);

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
