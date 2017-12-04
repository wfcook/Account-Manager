using GeoCoordinatePortable;
using Newtonsoft.Json;
using POGOProtos.Data.Player;
using POGOProtos.Map.Fort;
using PokemonGoGUI.AccountScheduler;
using PokemonGoGUI.Enums;
using PokemonGoGUI.Exceptions;
using PokemonGoGUI.GoManager.Models;
using PokemonGoGUI.Models;
using PokemonGoGUI.ProxyManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private bool _proxyIssue = false;


        //Needs to be saved on close
        public GoProxy CurrentProxy { get; set; }

        [JsonIgnore]
        public ProxyHandler ProxyHandler { get; set; }

        private bool _isPaused { get { return !_pauser.WaitOne(0); } }

        [JsonConstructor]
        public Manager()
        {
            Stats = new PlayerStats();
            Logs = new List<Log>();
            Tracker = new Tracker();

            LoadFarmLocations();
        }

        public Manager(ProxyHandler handler)
        {
            UserSettings = new Settings();
            Logs = new List<Log>();
            Stats = new PlayerStats();
            Tracker = new PokemonGoGUI.AccountScheduler.Tracker();

            ProxyHandler = handler;

            LoadFarmLocations();
        }

        public async Task<MethodResult> Login()
        {
            LogCaller(new LoggerEventArgs("Attempting to login ...", LoggerTypes.Debug));

            try
            {
                MethodResult result = null;
                result = await _client.DoLogin(UserSettings);
                LogCaller(new LoggerEventArgs(result.Message, LoggerTypes.Debug));

                if (CurrentProxy != null)
                {
                    ProxyHandler.ResetFailCounter(CurrentProxy);
                }

                return result;
            }
            catch (PtcOfflineException)
            {
                LogCaller(new LoggerEventArgs("Ptc server offline. Please try again later.", LoggerTypes.Warning));

                return new MethodResult
                {
                    Message = "Ptc server offline."
                };
            }
            catch (AccountNotVerifiedException)
            {
                Stop();

                RemoveProxy();

                LogCaller(new LoggerEventArgs("Account not verified. Stopping ...", LoggerTypes.Warning));

                AccountState = Enums.AccountState.NotVerified;

                return new MethodResult
                {
                    Message = "Account not verified."
                };
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    if (String.IsNullOrEmpty(Proxy))
                    {
                        LogCaller(new LoggerEventArgs("Login request has timed out.", LoggerTypes.Warning));
                    }
                    else
                    {
                        _proxyIssue = true;
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
                        _proxyIssue = true;
                        LogCaller(new LoggerEventArgs("Potential http proxy detected. Only https proxies will work.", LoggerTypes.ProxyIssue));

                        return new MethodResult
                        {
                            Message = "Http proxy detected"
                        };
                    }
                    else if (ex.Status == WebExceptionStatus.ConnectFailure || ex.Status == WebExceptionStatus.ProtocolError || ex.Status == WebExceptionStatus.ReceiveFailure
                        || ex.Status == WebExceptionStatus.ServerProtocolViolation)
                    {
                        _proxyIssue = true;
                        LogCaller(new LoggerEventArgs("Proxy is offline", LoggerTypes.ProxyIssue));

                        return new MethodResult
                        {
                            Message = "Proxy is offline"
                        };
                    }
                }

                if (!String.IsNullOrEmpty(Proxy))
                {
                    _proxyIssue = true;
                }

                LogCaller(new LoggerEventArgs("Failed to login due to request error", LoggerTypes.Exception, ex.InnerException));

                return new MethodResult
                {
                    Message = "Failed to login due to request error"
                };
            }
            catch (TaskCanceledException)
            {
                if (String.IsNullOrEmpty(Proxy))
                {
                    LogCaller(new LoggerEventArgs("Login request has timed out", LoggerTypes.Warning));
                }
                else
                {
                    _proxyIssue = true;
                    LogCaller(new LoggerEventArgs("Login request has timed out. Possible bad proxy", LoggerTypes.ProxyIssue));
                }

                return new MethodResult
                {
                    Message = "Login request has timed out"
                };
            }
            catch (InvalidCredentialsException ex)
            {
                //Puts stopping log before other log.
                Stop();

                RemoveProxy();

                LogCaller(new LoggerEventArgs("Invalid credentials or account lockout. Stopping bot...", LoggerTypes.Warning, ex));

                return new MethodResult
                {
                    Message = "Username or password incorrect"
                };
            }
            catch (IPBannedException)
            {
                if (UserSettings.StopOnIPBan)
                {
                    Stop();
                }

                string message = String.Empty;

                if (!String.IsNullOrEmpty(Proxy))
                {
                    if (CurrentProxy != null)
                    {
                        ProxyHandler.MarkProxy(CurrentProxy, true);
                    }

                    message = "Proxy IP is banned.";
                }
                else
                {
                    message = "IP address is banned.";
                }

                _proxyIssue = true;

                LogCaller(new LoggerEventArgs(message, LoggerTypes.ProxyIssue));

                return new MethodResult
                {
                    Message = message
                };
            }
            catch (GoogleException ex)
            {
                Stop();
                RemoveProxy();

                LogCaller(new LoggerEventArgs(ex.Message, LoggerTypes.Warning));

                return new MethodResult
                {
                    Message = "Failed to login"
                };
            }
            catch (Exception ex)
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
            //Fixing a bug on my part
            if (Tracker == null)
            {
                Tracker = new Tracker();
            }

            ServicePointManager.DefaultConnectionLimit = Int32.MaxValue;

            if (IsRunning)
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

            if (!_wasAutoRestarted)
            {
                _expGained = 0;
            }

            IsRunning = true;
            _totalZeroExpStops = 0;
            _client.SetSettings(UserSettings);
            _pauser.Set();
            _autoRestart = false;
            //_wasAutoRestarted = false;
            _rand = new Random();

            State = BotState.Starting;

            Thread t = new Thread(RunningThread)
            {
                IsBackground = true
            };

            LogCaller(new LoggerEventArgs("Bot started", LoggerTypes.Info));

            _runningStopwatch.Start();
            _potentialPokemonBan = false;
            _fleeingPokemonResponses = 0;

            t.Start();

            return new MethodResult
            {
                Message = "Bot started"
            };
        }

        public void Restart()
        {
            if (!IsRunning)
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
            if (!IsRunning)
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
            if (State == BotState.Paused || State == BotState.Pausing)
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
            if (UserSettings.RunForHours == 0)
            {
                return false;
            }

            if (_runningStopwatch.Elapsed.TotalHours >= UserSettings.RunForHours)
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
            int currentFails = 0;

            //Reset account state
            AccountState = Enums.AccountState.Good;

            while (IsRunning)
            {
                if (CheckTime())
                {
                    continue;
                }

                WaitPaused();

                if ((_proxyIssue || CurrentProxy == null) && UserSettings.AutoRotateProxies)
                {
                    bool success = await ChangeProxy();

                    //Fails when it's stopping
                    if (!success)
                    {
                        continue;
                    }

                    //Have to restart to set proxy
                    Restart();

                    _proxyIssue = false;

                    continue;
                }


                StartingUp = true;

                if (currentFails >= UserSettings.MaxFailBeforeReset)
                {
                    currentFails = 0;
                    _client.Logout();
                }

                if (_failedInventoryReponses >= _failedInventoryUntilBan)
                {
                    AccountState = AccountState.PermAccountBan;

                    LogCaller(new LoggerEventArgs("Potential account ban", LoggerTypes.Warning));

                    //Remove proxy
                    RemoveProxy();

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

                    //LogCaller(new LoggerEventArgs("Sending echo test ...", LoggerTypes.Debug));

                    result = await CheckReauthentication();

                    if (!result.Success)
                    {
                        LogCaller(new LoggerEventArgs("Echo failed. Logging out before retry.", LoggerTypes.Debug));

                        _client.Logout();

                        await Task.Delay(failedWaitTime);

                        continue;
                    }

                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                    if (UserSettings.StopOnAPIUpdate)
                    {
                        //Get Game settings
                        LogCaller(new LoggerEventArgs("Grabbing game settings ...", LoggerTypes.Debug));

                        try
                        {
                            MethodResult<bool> minClientResponse = await GetGameSettings(_client.VersionStr);

                            if (result.Success)
                            {
                                //Version isn't 0.xx.x
                                if (!minClientResponse.Data)
                                {
                                    LogCaller(new LoggerEventArgs($"Emulates API {_client.VersionStr} ...", LoggerTypes.FatalError, new Exception($"New API needed {_client.ClientSession.GlobalSettings.MinimumClientVersion}. Stopping ...")));
                                    Stop();
                                    continue;
                                }
                            }

                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                        }
                        catch (Exception)
                        {
                            AccountState = AccountState.PokemonBanAndPokestopBanTemp;
                            LogCaller(new LoggerEventArgs("Game settings failed", LoggerTypes.FatalError, new Exception("Maybe this account is banned ...")));
                            Stop();
                            continue;
                        }
                    }

                    //Get pokemon settings
                    if (PokeSettings == null)
                    {
                        LogCaller(new LoggerEventArgs("Grabbing pokemon settings ...", LoggerTypes.Debug));
                        result = await GetItemTemplates();

                        if(!result.Success)
                        {
                            AccountState = AccountState.PokemonBanAndPokestopBanTemp;
                            LogCaller(new LoggerEventArgs("Load pokemon settings failed", LoggerTypes.FatalError, new Exception("Maybe this account is banned ...")));
                            Stop();
                            continue;
                        }
                    }

                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                    //Get profile data not needed on pogolib
                    //LogCaller(new LoggerEventArgs("Grabbing player data ...", LoggerTypes.Debug));
                    //result = await GetPlayer();

                    //await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                    //Update inventory
                    LogCaller(new LoggerEventArgs("Updating inventory items ...", LoggerTypes.Debug));

                    result = await UpdateInventory();

                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                    if (!result.Success)
                    {
                        if (result.Message == "Failed to get inventory.")
                        {
                            ++_failedInventoryReponses;
                        }

                        await Task.Delay(failedWaitTime);

                        continue;
                    }

                    if (UserSettings.ClaimLevelUpRewards)
                    {
                        LogCaller(new LoggerEventArgs("Getting level up rewards ...", LoggerTypes.Debug));

                        result = await ClaimLevelUpRewards(Level);

                        await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                    }

                    _failedInventoryReponses = 0;

                    if (WaitPaused())
                    {
                        continue;
                    }

                    //End startup phase
                    StartingUp = false;

                    //Prevent changing back to running state
                    if (State != BotState.Stopping)
                    {
                        State = BotState.Running;
                    }
                    else
                    {
                        continue;
                    }

                    //Update location
                    if (_firstRun)
                    {
                        LogCaller(new LoggerEventArgs("Setting default location ...", LoggerTypes.Debug));

                        result = await UpdateLocation(new GeoCoordinate(UserSettings.DefaultLatitude, UserSettings.DefaultLongitude));

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

                    MethodResult<List<FortData>> pokestops = await GetPokeStops();

                    if (!pokestops.Success)
                    {
                        await Task.Delay(failedWaitTime);

                        continue;
                    }

                    int pokeStopNumber = 1;
                    int totalStops = pokestops.Data.Count;

                    if (totalStops == 0)
                    {
                        _proxyIssue = false;
                        _potentialPokeStopBan = false;

                        LogCaller(new LoggerEventArgs(String.Format("{0}. Failure {1}/{2}", pokestops.Message, currentFails, UserSettings.MaxFailBeforeReset), LoggerTypes.Warning));

                        if (UserSettings.AutoRotateProxies && currentFails >= UserSettings.MaxFailBeforeReset)
                        {
                            if (pokestops.Message.StartsWith("No pokestop data found."))
                            {
                                _proxyIssue = true;
                                await ChangeProxy();
                            }
                        }

                        await Task.Delay(failedWaitTime);

                        continue;
                    }

                    GeoCoordinate defaultLocation = new GeoCoordinate(UserSettings.DefaultLatitude, UserSettings.DefaultLongitude);

                    List<FortData> pokestopsToFarm = pokestops.Data.ToList();

                    int currentFailedStops = 0;

                    while (pokestopsToFarm.Any())
                    {
                        if (!IsRunning || currentFailedStops >= UserSettings.MaxFailBeforeReset)
                        {
                            break;
                        }

                        if (CheckTime())
                        {
                            continue;
                        }

                        WaitPaused();

                        pokestopsToFarm = pokestopsToFarm.OrderBy(x => CalculateDistanceInMeters(_client.ClientSession.Player.Latitude, _client.ClientSession.Player.Longitude, x.Latitude, x.Longitude)).ToList();

                        FortData pokestop = pokestopsToFarm[0];
                        pokestopsToFarm.RemoveAt(0);

                        GeoCoordinate currentLocation = new GeoCoordinate(_client.ClientSession.Player.Latitude, _client.ClientSession.Player.Longitude);
                        GeoCoordinate fortLocation = new GeoCoordinate(pokestop.Latitude, pokestop.Longitude);

                        double distance = CalculateDistanceInMeters(currentLocation, fortLocation);

                        LogCaller(new LoggerEventArgs(String.Format("Going to stop {0} of {1}. Distance {2:0.00}m", pokeStopNumber, totalStops, distance), LoggerTypes.Info));

                        //Go to stops
                        MethodResult walkResult = await GoToLocation(new GeoCoordinate(pokestop.Latitude, pokestop.Longitude));

                        if (!walkResult.Success)
                        {
                            LogCaller(new LoggerEventArgs("Too many failed walking attempts. Restarting to fix ...", LoggerTypes.Warning));

                            break;
                        }

                        await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                        //Search
                        double filledInventorySpace = FilledInventorySpace();

                        if (filledInventorySpace < UserSettings.SearchFortBelowPercent)
                        {
                            MethodResult searchResult = await SearchPokestop(pokestop);

                            //OutOfRange will show up as a success
                            if (searchResult.Success)
                            {
                                currentFailedStops = 0;
                            }
                            else
                            {
                                ++currentFailedStops;
                            }
                        }
                        else
                        {
                            LogCaller(new LoggerEventArgs(String.Format("Skipping fort. Currently at {0:0.00}% filled", filledInventorySpace), LoggerTypes.Info));
                        }

                        //Stop bot instantly
                        if (!IsRunning)
                        {
                            continue;
                        }

                        int remainingBalls = RemainingPokeballs();

                        if (remainingBalls > 0)
                        {
                            //Catch nearby pokemon
                            MethodResult nearbyPokemonResponse = await CatchNeabyPokemon();

                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                            //Get nearby lured pokemon
                            MethodResult luredPokemonResponse = await CatchLuredPokemon(pokestop);

                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                        }

                        //Check sniping
                        if (Stats.Level >= UserSettings.SnipeAfterLevel)
                        {
                            if (UserSettings.SnipePokemon && IsRunning && pokeStopNumber >= UserSettings.SnipeAfterPokestops && pokeStopNumber % UserSettings.SnipeAfterPokestops == 0)
                            {
                                if (remainingBalls >= UserSettings.MinBallsToSnipe)
                                {
                                    await SnipeAllPokemon();
                                }
                                else
                                {
                                    LogCaller(new LoggerEventArgs(String.Format("Not enough pokeballs to snipe. Need {0} have {1}", UserSettings.MinBallsToSnipe, remainingBalls), LoggerTypes.Info));
                                }

                                await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                            }
                        }

                        //Clean inventory, evolve, transfer, etc on first and every 10 stops
                        if (IsRunning && ((pokeStopNumber > 4 && pokeStopNumber % 10 == 0) || pokeStopNumber == 1))
                        {
                            MethodResult echoResult = await CheckReauthentication();

                            //Echo failed, restart
                            if (!echoResult.Success)
                            {
                                break;
                            }

                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                            bool secondInventoryUpdate = false;

                            int prevLevel = Level;

                            await UpdateInventory();

                            if (Level > prevLevel)
                            {
                                await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                                await ClaimLevelUpRewards(Level);
                            }

                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                            if (UserSettings.RecycleItems)
                            {
                                secondInventoryUpdate = true;

                                await RecycleFilteredItems();

                                await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                            }

                            if (UserSettings.EvolvePokemon)
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
                                MethodResult transferResult = await TransferFilteredPokemon();

                                if (transferResult.Success)
                                {
                                    secondInventoryUpdate = true;

                                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                                }
                            }

                            if (UserSettings.IncubateEggs)
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

                        if (UserSettings.MaxLevel > 0 && Level >= UserSettings.MaxLevel)
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
                catch (Exception ex)
                {
                    LogCaller(new LoggerEventArgs("Unknown exception occured. Restarting ...", LoggerTypes.Exception, ex));
                }

                currentFails = 0;
                _firstRun = false;
            }

            State = BotState.Stopped;
            _client.Logout();
            LogCaller(new LoggerEventArgs(String.Format("Bot fully stopped at {0}", DateTime.Now), LoggerTypes.Info));

            if (_autoRestart)
            {
                _wasAutoRestarted = true;
                Start();
            }
            else if (UserSettings.AutoRemoveOnStop)
            {
                RemoveProxy();
            }
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                _client.Logout();
                return;
            }

            State = BotState.Stopping;
            LogCaller(new LoggerEventArgs("Bot stopping. Please wait for actions to complete ...", LoggerTypes.Info));

            _pauser.Set();
            _runningStopwatch.Stop();
            _failedInventoryReponses = 0;

            if (!_autoRestart)
            {
                _runningStopwatch.Reset();
            }

            IsRunning = false;
        }

        /*
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
        */

        private async Task<MethodResult> CheckReauthentication()
        {
            if (!_client.AccessToken.IsExpired)
            {
                return new MethodResult
                {
                    Success = true
                };
            }

            try
            {
                LogCaller(new LoggerEventArgs("Session expired. Logging back in", LoggerTypes.Debug));

                await _client.DoLogin(UserSettings);

                return new MethodResult
                {
                    Success = true
                };
            }
            /*catch (BadImageFormatException)
            {
                LogCaller(new LoggerEventArgs("Incorrect encrypt dll used. Please delete 'encrypt.dll' and restart the program", LoggerTypes.FatalError));

                return new MethodResult
                {
                    Message = "Incorrect DLL used"
                };
            }*/
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to reauthenticate failed", LoggerTypes.Warning, ex));

                return new MethodResult();
            }
        }

        private void LoadFarmLocations()
        {
            FarmLocations = new List<FarmLocation>
            {
                new FarmLocation
                {
                    Name = "Current"
                },

                new FarmLocation
                {
                    Latitude = -33.870225,
                    Longitude = 151.208343,
                    Name = "Sydney, Australia"
                },

                new FarmLocation
                {
                    Latitude = 35.665705,
                    Longitude = 139.753348,
                    Name = "Tokyo, Japan"
                },

                new FarmLocation
                {
                    Latitude = 40.764665,
                    Longitude = -73.973184,
                    Name = "Central Park, NY"
                },

                new FarmLocation
                {
                    Latitude = 52.373806,
                    Longitude = 4.903985,
                    Name = "Amsterdam, Netherlands"
                }
            };
        }

        public void ClearStats()
        {
            _fleeingPokemonResponses = 0;
            TotalPokeStopExp = 0;
            Tracker.Values.Clear();
            Tracker.CalculatedTrackingHours();
        }
    }
}
