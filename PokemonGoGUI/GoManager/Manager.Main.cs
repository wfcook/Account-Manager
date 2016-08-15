using Newtonsoft.Json;
using POGOProtos.Data;
using POGOProtos.Data.Player;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings.Master;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Helpers;
using PokemonGoGUI.Enums;
using PokemonGoGUI.GoManager.Models;
using PokemonGoGUI.Models;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        private Client _client = new Client();
        private DateTime _lastMapRequest = new DateTime();
        private Random _rand = new Random();

        private int _totalZeroExpStops = 0;
        private bool _firstRun = true;
        private int _failedInventoryReponses = 0;
        private const int _failedInventoryUntilBan = 3;
        private int _fleeingPokemonResponses = 0;
        private bool _potentialPokemonBan = false;
        private const int _fleeingPokemonUntilBan = 3;
        private bool _potentialPokeStopBan = false;
        /*private int _failedPokestopResponse = 0;*/
        private bool _autoRestart = false;
        private bool _wasAutoRestarted = false;

        private ManualResetEvent _pauser = new ManualResetEvent(true);

        private bool _isPaused { get { return !_pauser.WaitOne(0); } }

        [JsonConstructor]
        public Manager(bool jsonConstructor = true)
        {
            //Json.net can't deserialize the type
            Stats = new PlayerStats();
            Logs = new List<Log>();

            LoadFarmLocations();
        }

        public Manager()
        {
            UserSettings = new Settings();
            Logs = new List<Log>();
            Stats = new PlayerStats();

            LoadFarmLocations();
        }

        public async Task<MethodResult> Login()
        {
            LogCaller(new LoggerEventArgs("Attempting to login ...", LoggerTypes.Debug));

            try
            {
                MethodResult result = await _client.DoLogin(UserSettings);

                LogCaller(new LoggerEventArgs(result.Message, LoggerTypes.Debug));

                return result;
            }
            catch(PtcOfflineException ex)
            {
                LogCaller(new LoggerEventArgs("Ptc server offline. Please try again later.", LoggerTypes.Exception));

                return new MethodResult
                {
                    Message = "Ptc server offline."
                };
            }
            catch(AccountNotVerifiedException ex)
            {
                LogCaller(new LoggerEventArgs("Account not verified.", LoggerTypes.Exception));

                return new MethodResult
                {
                    Message = "Account not verified."
                };
            }
            catch(WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    if (String.IsNullOrEmpty(Proxy))
                    {
                        LogCaller(new LoggerEventArgs("Login request has timed out.", LoggerTypes.Warning));
                    }
                    else
                    {
                        LogCaller(new LoggerEventArgs("Login request has timed out. Possible bad proxy.", LoggerTypes.ProxyIssue));
                    }

                    return new MethodResult
                    {
                        Message = "Request has timed out."
                    };
                }

                if (!String.IsNullOrEmpty(Proxy))
                {
                    if (ex.Status == WebExceptionStatus.ConnectionClosed)
                    {
                        LogCaller(new LoggerEventArgs("Potential http proxy detected. Only https proxies will work.", LoggerTypes.ProxyIssue));

                        return new MethodResult
                        {
                            Message = "Http proxy detected"
                        };
                    }
                    else if (ex.Status == WebExceptionStatus.ConnectFailure || ex.Status == WebExceptionStatus.ProtocolError || ex.Status == WebExceptionStatus.ReceiveFailure
                        || ex.Status == WebExceptionStatus.ServerProtocolViolation)
                    {
                        LogCaller(new LoggerEventArgs("Proxy is offline", LoggerTypes.ProxyIssue));

                        return new MethodResult
                        {
                            Message = "Proxy is offline"
                        };
                    }
                }

                LogCaller(new LoggerEventArgs("Failed to login due to request error", LoggerTypes.Exception, ex.InnerException));

                return new MethodResult
                {
                    Message = "Failed to login due to request error"
                };
            }
            catch(TaskCanceledException)
            {
                    if(String.IsNullOrEmpty(Proxy))
                    {
                        LogCaller(new LoggerEventArgs("Login request has timed out", LoggerTypes.Warning));
                    }
                    else
                    {
                        LogCaller(new LoggerEventArgs("Login request has timed out. Possible bad proxy", LoggerTypes.ProxyIssue));
                    }

                    return new MethodResult
                    {
                        Message = "Login request has timed out"
                    };
            }
            catch(InvalidCredentialsException ex)
            {
                //Puts stopping log before other log.
                Stop();

                LogCaller(new LoggerEventArgs("Invalid credentials or account lockout. Stopping bot...", LoggerTypes.Warning, ex));

                return new MethodResult
                {
                    Message = "Username or password incorrect"
                };
            }
            catch(IPBannedException ex)
            {
                Stop();

                string message = String.Empty;

                if(!String.IsNullOrEmpty(Proxy))
                {
                    message = "Proxy IP is banned. Stopping bot ...";
                }
                else
                {
                    message = "IP address is banned. Stopping bot ...";
                }
                
                LogCaller(new LoggerEventArgs(message, LoggerTypes.ProxyIssue));

                return new MethodResult
                {
                    Message = message
                };
            }
            catch(GoogleException ex)
            {
                Stop();

                LogCaller(new LoggerEventArgs(ex.Message, LoggerTypes.Warning));

                return new MethodResult
                {
                    Message = "Failed to login"
                };
            }
            catch(Exception ex)
            {
                _client.Logout();

                LogCaller(new LoggerEventArgs("Failed to login", LoggerTypes.Exception, ex));

                return new MethodResult
                {
                    Message = "Failed to login"
                };
            }
        }

        public MethodResult Start()
        {
            ServicePointManager.DefaultConnectionLimit = 100;

            if(IsRunning)
            {
                return new MethodResult
                {
                    Message = "Bot already running"
                };
            }
            else if (State != BotState.Stopped)
            {
                return new MethodResult
                {
                    Message = "Please wait for bot to fully stop"
                };
            }

            if(!_wasAutoRestarted)
            {
                _expGained = 0;
                _wasAutoRestarted = false;
            }

            IsRunning = true;
            _totalZeroExpStops = 0;
            _client.SetSettings(UserSettings);
            _pauser.Set();
            _autoRestart = false;
            _rand = new Random();

            State = BotState.Starting;

            Thread t = new Thread(RunningThread);
            t.IsBackground = true;

            LogCaller(new LoggerEventArgs("Bot started", LoggerTypes.Info));

            _runningStopwatch.Start();
            _potentialPokemonBan = false;

            t.Start();

            return new MethodResult
            {
                Message = "Bot started"
            };
        }

        public void Restart()
        {
            if(!IsRunning)
            {
                Start();

                return;
            }

            LogCaller(new LoggerEventArgs("Restarting bot", LoggerTypes.Info));

            _autoRestart = true;

            Stop();
        }

        public void Pause()
        {
            if(!IsRunning)
            {
                return;
            }

            _pauser.Reset();
            _runningStopwatch.Stop();

            LogCaller(new LoggerEventArgs("Pausing bot ...", LoggerTypes.Info));

            State = BotState.Pausing;
        }

        public void UnPause()
        {
            if (!IsRunning)
            {
                return;
            }

            _pauser.Set();
            _runningStopwatch.Start();

            LogCaller(new LoggerEventArgs("Unpausing bot ...", LoggerTypes.Info));

            State = BotState.Running;
        }

        public void TogglePause()
        {
            if(State == BotState.Paused || State == BotState.Pausing)
            {
                UnPause();
            }
            else
            {
                Pause();
            }
        }

        private bool WaitPaused()
        {
            if (_isPaused)
            {
                LogCaller(new LoggerEventArgs("Bot paused", LoggerTypes.Info));

                State = BotState.Paused;
                _pauser.WaitOne();

                return true;
            }

            return false;
        }

        private bool CheckTime()
        {
            if(UserSettings.RunForHours == 0)
            {
                return false;
            }

            if(_runningStopwatch.Elapsed.TotalHours >= UserSettings.RunForHours)
            {
                Stop();

                LogCaller(new LoggerEventArgs("Max runtime reached. Stopping ...", LoggerTypes.Info));

                return true;
            }

            return false;
        }

        private async void RunningThread()
        {
            int failedWaitTime = 5000;
            int maxFailed = 3;
            int currentFails = 0;

            //Reset account state
            AccountState = Enums.AccountState.Good;

            while(IsRunning)
            {
                if(CheckTime())
                {
                    continue;
                }

                WaitPaused();

                StartingUp = true;

                if(currentFails >= maxFailed)
                {
                    currentFails = 0;
                    _client.Logout();
                }

                if (_failedInventoryReponses >= _failedInventoryUntilBan)
                {
                    AccountState = AccountState.AccountBan;

                    LogCaller(new LoggerEventArgs("Potential account ban/deletion or server issues detected. Note: This is NOT confirmed, but we can't run it anyways. Stopping ...", LoggerTypes.Warning));

                    Stop();

                    continue;
                }

                ++currentFails;

                MethodResult result = new MethodResult();

                try
                {
                    #region Startup

                    if (!_client.LoggedIn)
                    {
                        //Login
                        result = await Login();

                        if (!result.Success)
                        {
                            //A failed login should require longer wait
                            await Task.Delay(failedWaitTime * 3);

                            continue;
                        }
                    }

                    LogCaller(new LoggerEventArgs("Sending echo test ...", LoggerTypes.Debug));

                    result = await SendEcho();

                    if(!result.Success)
                    {
                        //LogCaller(new LoggerEventArgs("Echo failed. Logging out before retry.", LoggerTypes.Debug));

                        _client.Logout();

                        await Task.Delay(failedWaitTime);

                        continue;
                    }

                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                    //Get pokemon settings
                    if(PokeSettings == null)
                    {
                        LogCaller(new LoggerEventArgs("Grabbing pokemon settings ...", LoggerTypes.Debug));

                        result = await GetItemTemplates();
                    }


                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                    //Get profile data
                    LogCaller(new LoggerEventArgs("Grabbing player data ...", LoggerTypes.Debug));
                    result = await RepeatAction(GetProfile, 2);

                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                    //Update inventory
                    LogCaller(new LoggerEventArgs("Updating inventory items ...", LoggerTypes.Debug));

                    result = await RepeatAction<List<InventoryItem>>(UpdateInventory, 1);

                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                    if(!result.Success)
                    {
                        if (result.Message == "Failed to get inventory.")
                        {
                            ++_failedInventoryReponses;
                        }

                        await Task.Delay(failedWaitTime);

                        continue;
                    }

                    _failedInventoryReponses = 0;

                    if(WaitPaused())
                    {
                        continue;
                    }

                    //End startup phase
                    StartingUp = false;
                    State = BotState.Running;

                    //Update location
                    if (_firstRun)
                    {
                        LogCaller(new LoggerEventArgs("Setting default location ...", LoggerTypes.Debug));

                        result = await RepeatAction(() => UpdateLocation(new GeoCoordinate(UserSettings.DefaultLatitude, UserSettings.DefaultLongitude)), 2);

                        if (!result.Success)
                        {
                            await Task.Delay(failedWaitTime);

                            continue;
                        }
                    }

                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                    #endregion

                    #region PokeStopTask

                    //Get pokestops
                    LogCaller(new LoggerEventArgs("Grabbing pokestops...", LoggerTypes.Debug));

                    MethodResult<List<FortData>> pokestops = await RepeatAction<List<FortData>>(GetPokeStops, 2);

                    if(!pokestops.Success)
                    {
                        await Task.Delay(failedWaitTime);

                        continue;
                    }

                    int pokeStopNumber = 1;
                    int totalStops = pokestops.Data.Count;

                    if(totalStops == 0)
                    {
                        _potentialPokeStopBan = false;

                        LogCaller(new LoggerEventArgs(String.Format("{0}. Failure {1}/{2}", pokestops.Message, currentFails, maxFailed), LoggerTypes.Warning));

                        await Task.Delay(failedWaitTime);

                        continue;
                    }

                    GeoCoordinate defaultLocation = new GeoCoordinate(UserSettings.DefaultLatitude, UserSettings.DefaultLongitude);

                    List<FortData> pokestopsToFarm = pokestops.Data;

                    int maxFailedStops = 3;
                    int currentFailedStops = 0;

                    while(pokestopsToFarm.Any())
                    {
                        if (!IsRunning || currentFailedStops >= maxFailedStops)
                        {
                            break;
                        }

                        if (CheckTime())
                        {
                            continue;
                        }

                        WaitPaused();

                        pokestopsToFarm = pokestopsToFarm.OrderBy(x => CalculateDistanceInMeters(_client.CurrentLatitude, _client.CurrentLongitude, x.Latitude, x.Longitude)).ToList();

                        FortData pokestop = pokestopsToFarm[0];
                        pokestopsToFarm.RemoveAt(0);

                        GeoCoordinate currentLocation = new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude);
                        GeoCoordinate fortLocation = new GeoCoordinate(pokestop.Latitude, pokestop.Longitude);

                        double distance = CalculateDistanceInMeters(currentLocation, fortLocation);

                        LogCaller(new LoggerEventArgs(String.Format("Going to stop {0} of {1}. Distance {2:0.00}m", pokeStopNumber, totalStops, distance), LoggerTypes.Info));

                        //Go to stops
                        MethodResult walkResult = await RepeatAction(() => GoToLocation(new GeoCoordinate(pokestop.Latitude, pokestop.Longitude)), 1);

                        if (!walkResult.Success)
                        {
                            LogCaller(new LoggerEventArgs("Too many failed walking attempts. Restarting to fix ...", LoggerTypes.Warning));

                            break;
                        }

                        await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                        //Search
                        MethodResult searchResult = await SearchPokestop(pokestop);

                        //OutOfRange will show up as a success
                        if(searchResult.Success)
                        {
                            currentFailedStops = 0;
                        }
                        else
                        {
                            ++currentFailedStops;
                        }

                        //Stop bot instantly
                        if(!IsRunning)
                        {
                            continue;
                        }

                        int remainingBalls = RemainingPokeballs();

                        if (remainingBalls > 0)
                        {
                            //Catch nearby pokemon
                            MethodResult nearbyPokemonResponse = await RepeatAction(CatchNeabyPokemon, 1);

                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                            //Get nearby lured pokemon
                            MethodResult luredPokemonResponse = await RepeatAction(() => CatchLuredPokemon(pokestop), 1);

                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                        }

                        //Check sniping
                        if (Stats.Level >= UserSettings.SnipeAfterLevel)
                        {
                            if (remainingBalls >= UserSettings.MinBallsToSnipe)
                            {
                                if (UserSettings.SnipePokemon && IsRunning && pokeStopNumber >= UserSettings.SnipeAfterPokestops && pokeStopNumber % UserSettings.SnipeAfterPokestops == 0)
                                {
                                    await SnipeAllPokemon();
                                }
                            }
                            else
                            {
                                LogCaller(new LoggerEventArgs(String.Format("Not enough pokeballs to snipe. Need {0} have {1}", UserSettings.MinBallsToSnipe, remainingBalls), LoggerTypes.Info));
                            }

                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                        }

                        //Clean inventory, evolve, transfer, etc on first and every 10 stops
                        if(IsRunning && ((pokeStopNumber > 4 && pokeStopNumber % 10 == 0) || pokeStopNumber == 1))
                        {
                            await SendEcho();

                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                            bool secondInventoryUpdate = false;

                            await UpdateInventory();

                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                            if (UserSettings.RecycleItems)
                            {
                                secondInventoryUpdate = true;

                                await RecycleFilteredItems();

                                await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                            }

                            if(UserSettings.EvolvePokemon)
                            {
                                MethodResult evolveResult = await EvolveFilteredPokemon();

                                if (evolveResult.Success)
                                {
                                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                                    await UpdateInventory();

                                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                                }
                            }

                            if (UserSettings.TransferPokemon)
                            {
                                MethodResult transferResult =  await TransferFilteredPokemon();

                                if(transferResult.Success)
                                {
                                    secondInventoryUpdate = true;

                                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                                }
                            }

                            if(UserSettings.IncubateEggs)
                            {
                                MethodResult incubateResult = await IncubateEggs();

                                if (incubateResult.Success)
                                {
                                    secondInventoryUpdate = true;

                                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                                }
                            }

                            if (secondInventoryUpdate)
                            {
                                await UpdateInventory();
                            }
                        }

                        ++pokeStopNumber;

                        await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                        if(UserSettings.MaxLevel > 0 && Level >= UserSettings.MaxLevel)
                        {
                            LogCaller(new LoggerEventArgs(String.Format("Max level of {0} reached.", UserSettings.MaxLevel), LoggerTypes.Info));

                            Stop();
                        }

                        if (_potentialPokeStopBan)
                        {
                            //Break out of pokestop loop to test for ip ban
                            break;
                        }
                    }

                    #endregion
                }
                catch(Exception ex)
                {
                    LogCaller(new LoggerEventArgs("Unknown exception occured. Restarting ...", LoggerTypes.Exception, ex));
                }

                currentFails = 0;
                _firstRun = false;
            }

            State = BotState.Stopped;
            _client.Logout();
            LogCaller(new LoggerEventArgs(String.Format("Bot fully stopped at {0}", DateTime.Now), LoggerTypes.Info));

            if(_autoRestart)
            {
                _wasAutoRestarted = true;
                Start();
            }
        }

        public void Stop()
        {
            if(!IsRunning)
            {
                return;
            }

            State = BotState.Stopping;
            LogCaller(new LoggerEventArgs("Bot stopping. Please wait for actions to complete ...", LoggerTypes.Info));

            _pauser.Set();
            _runningStopwatch.Stop();

            if(!_autoRestart)
            {
                _runningStopwatch.Reset();
            }

            IsRunning = false;
        }

        private async Task<MethodResult> RepeatAction(Func<Task<MethodResult>> action, int tries)
        {
            MethodResult result = new MethodResult();

            for(int i = 0; i < tries; i++)
            {
                result = await action();

                if(result.Success)
                {
                    return result;
                }

                await Task.Delay(CalculateDelay(1000, 200));
            }

            return result;
        }

        private async Task<MethodResult<T>> RepeatAction<T>(Func<Task<MethodResult<T>>> action, int tries)
        {
            MethodResult<T> result = new MethodResult<T>();

            for (int i = 0; i < tries; i++)
            {
                result = await action();

                if (result.Success)
                {
                    return result;
                }

                await Task.Delay(CalculateDelay(1000, 200));
            }

            return result;
        }

        private async Task<MethodResult> SendEcho()
        {
            try
            {
                if(_client.Misc == null || !_client.LoggedIn)
                {
                    return new MethodResult
                    {
                        Message = "Client not logged in"
                    };
                }

                EchoResponse response = await _client.Misc.SendEcho();

                return new MethodResult
                {
                    Message = response.Context,
                    Success = true
                };
            }
            catch(Exception)
            {
                LogCaller(new LoggerEventArgs("Echo failed", LoggerTypes.Warning));

                return new MethodResult
                {
                    Message = "Echo failed"
                };
            }
        }

        private void LoadFarmLocations()
        {
            FarmLocations = new List<FarmLocation>();

            FarmLocations.Add(new FarmLocation
                {
                    Name = "Current"
                });

            FarmLocations.Add(new FarmLocation
                {
                    Latitude = -33.870225,
                    Longitude = 151.208343,
                    Name = "Sydney, Australia"
                });

            FarmLocations.Add(new FarmLocation
                {
                    Latitude = 35.665705,
                    Longitude = 139.753348,
                    Name = "Tokyo, Japan"
                });

            FarmLocations.Add(new FarmLocation
                {
                    Latitude = 40.764665,
                    Longitude = -73.973184,
                    Name = "Central Park, NY"
                });

            FarmLocations.Add(new FarmLocation
                {
                    Latitude = 52.373806,
                    Longitude = 4.903985,  
                    Name = "Amsterdam, Netherlands"
                });
        }

        public void ClearStats()
        {
            _fleeingPokemonResponses = 0;
            //_expGained = 0;
            PokemonCaught = 0;
            PokestopsFarmed = 0;
            ItemsFarmed = 0;
            TotalPokeStopExp = 0;
        }
    }
}
