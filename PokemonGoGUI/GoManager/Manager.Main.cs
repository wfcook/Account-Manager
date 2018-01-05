using System.Globalization;
using GeoCoordinatePortable;
using Newtonsoft.Json;
using POGOLib.Official.Exceptions;
using POGOProtos.Data.Player;
using POGOProtos.Enums;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Responses;
using PokemonGoGUI.AccountScheduler;
using PokemonGoGUI.Enums;
using PokemonGoGUI.Exceptions;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager.Models;
using PokemonGoGUI.Models;
using PokemonGoGUI.ProxyManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using POGOLib.Official.Util.Hash.PokeHash;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        private Client _client = new Client();
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
        private DateTime TimeAutoCatch = DateTime.Now;
        private bool CatchDisabled = false;

        public bool _proxyIssue = false;

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

        public async Task<MethodResult> AcLogin()
        {
            LogCaller(new LoggerEventArgs("Attempting to login ...", LoggerTypes.Debug));
            AccountState = AccountState.Conecting;

            MethodResult result = null;
            result = await _client.DoLogin(this);
            LogCaller(new LoggerEventArgs(result.Message, LoggerTypes.Debug));

            if (!result.Success)
            {
                LogCaller(new LoggerEventArgs(result.Message, LoggerTypes.FatalError));
                if (AccountState == AccountState.Conecting || AccountState == AccountState.Good)
                    AccountState = AccountState.Unknown;
                Stop();
            }
            else
            {
                if (AccountState == AccountState.Conecting)
                {
                    AccountState = AccountState.Good;
                }
                // This is part of the login process
                if (UserSettings.ClaimLevelUpRewards){
                    await ClaimLevelUpRewards(Level);
                }
            }

            if (CurrentProxy != null)
            {
                ProxyHandler.ResetFailCounter(CurrentProxy);
            }

            return result;
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

            if (State != BotState.Stopped)
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
            _client.SetSettings(this);
            _pauser.Set();
            _autoRestart = false;
            //_wasAutoRestarted = false;
            _rand = new Random();

            State = BotState.Starting;

            var t = new Thread(RunningThread)
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
            _client.ClientSession.Pause();

            LogCaller(new LoggerEventArgs("Pausing bot ...", LoggerTypes.Info));

            State = BotState.Pausing;
        }

        public async void UnPause()
        {
            if (!IsRunning)
            {
                return;
            }

            _pauser.Set();
            _runningStopwatch.Start();
            await _client.ClientSession.ResumeAsync();

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
            if (Math.Abs(UserSettings.RunForHours) < 0.001)
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
            const int failedWaitTime = 5000;
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

                var result = new MethodResult();

                #region Startup

                try
                {
                    if (!_client.LoggedIn)
                    {
                        //Login
                        result = await AcLogin();

                        if (!result.Success)
                        {
                            //A failed login should require longer wait
                            await Task.Delay(failedWaitTime * 3);

                            continue;
                        }
                    }

                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                    UpdateInventory();

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
                            var remote = new Version();
                            if (_client.ClientSession.GlobalSettings != null)
                                remote = new Version(_client.ClientSession.GlobalSettings?.MinimumClientVersion);
                            if (_client.VersionStr < remote)
                            {
                                LogCaller(new LoggerEventArgs($"Emulates API {_client.VersionStr} ...", LoggerTypes.FatalError, new Exception($"New API needed {remote}. Stopping ...")));
                                Stop();
                                continue;
                            }
                        }
                        catch (Exception ex1)
                        {
                            AccountState = AccountState.PokemonBanAndPokestopBanTemp;
                            LogCaller(new LoggerEventArgs("Exception: " + ex1, LoggerTypes.Debug));
                            LogCaller(new LoggerEventArgs("Game settings failed", LoggerTypes.FatalError, new Exception("Maybe this account is banned ...")));
                            Stop();
                            continue;
                        }
                        await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                    }

                    //Get pokemon settings
                    if (PokeSettings == null)
                    {

                        LogCaller(new LoggerEventArgs("Grabbing pokemon settings ...", LoggerTypes.Debug));
                        result = await GetItemTemplates();

                        if (!result.Success)
                        {
                            AccountState = AccountState.PokemonBanAndPokestopBanTemp;
                            LogCaller(new LoggerEventArgs("Load pokemon settings failed", LoggerTypes.FatalError, new Exception("Maybe this account is banned ...")));
                            Stop();
                            continue;
                        }
                    }

                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                    //Auto complete tutorials
                    if (UserSettings.CompleteTutorial)
                    {
                        if (!PlayerData.TutorialState.Contains(TutorialState.AvatarSelection))
                        {
                            result = await MarkStartUpTutorialsComplete(true);

                            if (!result.Success)
                            {
                                LogCaller(new LoggerEventArgs("Failed. Marking startup tutorials completed..", LoggerTypes.Warning));

                                Stop();

                                await Task.Delay(failedWaitTime);

                                continue;
                            }

                            LogCaller(new LoggerEventArgs("Marking startup tutorials completed.", LoggerTypes.Success));

                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                        }


                        if (!PlayerData.TutorialState.Contains(TutorialState.PokestopTutorial))
                        {
                            result = await MarkTutorialsComplete(new [] {TutorialState.PokestopTutorial, TutorialState.PokemonBerry, TutorialState.UseItem });

                            if (!result.Success)
                            {
                                LogCaller(new LoggerEventArgs("Failed. Marking pokestop, pokemonberry, useitem, pokemoncapture tutorials completed..", LoggerTypes.Warning));

                                Stop();

                                await Task.Delay(failedWaitTime);

                                continue;
                            }

                            LogCaller(new LoggerEventArgs("Marking pokestop, pokemonberry, useitem, pokemoncapture tutorials completed.", LoggerTypes.Success));

                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                        }
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

                        result = await UpdateLocation(new GeoCoordinate(UserSettings.Location.Latitude, UserSettings.Location.Longitude));

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
                    LogCaller(new LoggerEventArgs("getting pokestops...", LoggerTypes.Debug));

                    MethodResult<List<FortData>> pokestops = GetPokeStops();

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
                            if (pokestops.Message.StartsWith("No pokestop data found.", StringComparison.Ordinal))
                            {
                                _proxyIssue = true;
                                await ChangeProxy();
                            }
                        }

                        await Task.Delay(failedWaitTime);

                        continue;
                    }

                    var defaultLocation = new GeoCoordinate(_client.ClientSession.Player.Latitude, _client.ClientSession.Player.Longitude);

                    var pokestopsToFarm = new Queue<FortData>(pokestops.Data);

                    int currentFailedStops = 0;

                    while (pokestopsToFarm.Any())
                    {
                        // In each iteration of the loop we store the current level
                        int prevLevel = Level;

                        if (!IsRunning || currentFailedStops >= UserSettings.MaxFailBeforeReset)
                        {
                            break;
                        }

                        if (CheckTime())
                        {
                            continue;
                        }

                        WaitPaused();

                        FortData pokestop = pokestopsToFarm.Dequeue();
                        LogCaller(new LoggerEventArgs("fort Dequeued: " + pokestop.Id, LoggerTypes.Debug));

                        var currentLocation = new GeoCoordinate(_client.ClientSession.Player.Latitude, _client.ClientSession.Player.Longitude);
                        var fortLocation = new GeoCoordinate(pokestop.Latitude, pokestop.Longitude);

                        double distance = CalculateDistanceInMeters(currentLocation, fortLocation);

                        string fort = (pokestop.Type == FortType.Gym) ? "gym" : "pokestop";

                        LogCaller(new LoggerEventArgs(String.Format("Going to {0} {1} of {2}. Distance {3:0.00}m", fort, pokeStopNumber, totalStops, distance), pokestop.Type == FortType.Checkpoint ? LoggerTypes.Info : LoggerTypes.FortGym));

                        //Go to pokestops
                        MethodResult walkResult = await GoToLocation(new GeoCoordinate(pokestop.Latitude, pokestop.Longitude));

                        if (!walkResult.Success)
                        {
                            LogCaller(new LoggerEventArgs("Too many failed walking attempts. Restarting to fix ...", LoggerTypes.Warning));
                            LogCaller(new LoggerEventArgs("Result: " + walkResult.Message, LoggerTypes.Debug));
                            break;
                        }

                        await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                        UpdateInventory();

                        if (CatchDisabled)
                        {
                            //Check delay if account not have balls
                            var now = DateTime.Now;
                            LogCaller(new LoggerEventArgs("Now: " + now.ToLongDateString() + " " + now.ToLongTimeString(), LoggerTypes.Debug));
                            LogCaller(new LoggerEventArgs("TimeAutoCatch: " + TimeAutoCatch.ToLongDateString() + " " + TimeAutoCatch.ToLongTimeString(), LoggerTypes.Debug));
                            if (now > TimeAutoCatch)
                            {
                                CatchDisabled = false;
                                LogCaller(new LoggerEventArgs("Enable catch after wait time.", LoggerTypes.Info));
                            }
                        }

                        // NOTE: not an "else" we could enabled catch in this time 
                        if (!CatchDisabled)
                        {
                            var remainingBalls = RemainingPokeballs();
                            LogCaller(new LoggerEventArgs("Remaining Balls: " + remainingBalls, LoggerTypes.Debug));

                            if (remainingBalls > 0)
                            {
                                //Catch nearby pokemon
                                MethodResult nearbyPokemonResponse = await CatchNeabyPokemon();

                                await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                            }
                            else
                            {
                                LogCaller(new LoggerEventArgs("You don't have any pokeball catching pokemon will be disabled during " + UserSettings.DisableCatchDelay.ToString(CultureInfo.InvariantCulture) + " minutes.", LoggerTypes.Info));
                                CatchDisabled = true;
                                TimeAutoCatch = DateTime.Now.AddMinutes(UserSettings.DisableCatchDelay);
                            }

                            if (RemainingPokeballs() > 0)
                            {
                                //Catch lured pokemon
                                MethodResult luredPokemonResponse = await CatchLuredPokemon(pokestop);
                                await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                            }
                            else
                            {
                                LogCaller(new LoggerEventArgs("You don't have any pokeball catching pokemon will be disabled during " + UserSettings.DisableCatchDelay.ToString(CultureInfo.InvariantCulture) + " minutes.", LoggerTypes.Info));
                                CatchDisabled = true;
                                TimeAutoCatch = DateTime.Now.AddMinutes(UserSettings.DisableCatchDelay);
                            }
                            UpdateInventory();
                        }

                        //Stop bot instantly
                        if (!IsRunning)
                        {
                            continue;
                        }

                        //Clean inventory,
                        if (UserSettings.RecycleItems)
                        {
                            await RecycleFilteredItems();
                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                        }


                        //Search
                        double filledInventorySpace = FilledInventoryStorage();
                        LogCaller(new LoggerEventArgs(String.Format("Filled Inventory Storage: {0:0.00}%", filledInventorySpace), LoggerTypes.Debug));

                        if ((filledInventorySpace < UserSettings.SearchFortBelowPercent) && (filledInventorySpace <= 100))
                        {
                            if (pokestop.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime())
                            {
                                if (pokestop.Type == FortType.Gym)
                                {
                                    if (!UserSettings.SpinGyms)
                                        continue;
                                    try
                                    {
                                        MethodResult<GymGetInfoResponse> _result = await GymGetInfo(pokestop);
                                        LogCaller(new LoggerEventArgs("Gym Name: " + _result.Data.Name, LoggerTypes.Info));
                                    }
                                    catch (Exception)
                                    {
                                        LogCaller(new LoggerEventArgs("Skypped Gym...", LoggerTypes.Warning));
                                        continue;
                                    }
                                }
                                else
                                {
                                    var fortDetails = await FortDetails(pokestop);
                                    LogCaller(new LoggerEventArgs("Fort Name: " + fortDetails.Data.Name, LoggerTypes.Info));
                                }

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
                                LogCaller(new LoggerEventArgs(String.Format("Skipping fort. In cooldown"), LoggerTypes.Info));
                            }
                        }
                        else
                        {
                            LogCaller(new LoggerEventArgs(String.Format("Skipping fort. Inventory Currently at {0:0.00}% filled", filledInventorySpace), LoggerTypes.Info));
                        }

                        //Stop bot instantly
                        if (!IsRunning)
                        {
                            continue;
                        }


                        // evolve, transfer, etc on first and every 10 stops
                        if (IsRunning && ((pokeStopNumber > 4 && pokeStopNumber % 10 == 0) || pokeStopNumber == 1))
                        {
                            MethodResult echoResult = await CheckReauthentication();

                            //Echo failed, restart
                            if (!echoResult.Success)
                            {
                                break;
                            }

                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));


                            if (UserSettings.EvolvePokemon)
                            {
                                MethodResult evolveResult = await EvolveFilteredPokemon();

                                if (evolveResult.Success)
                                {
                                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                                }
                            }

                            if (UserSettings.TransferPokemon)
                            {
                                MethodResult transferResult = await TransferFilteredPokemon();

                                if (transferResult.Success)
                                {
                                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                                }
                            }

                            if (UserSettings.IncubateEggs)
                            {
                                MethodResult incubateResult = await IncubateEggs();

                                if (incubateResult.Success)
                                {
                                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                                }
                            }

                            if (Level > prevLevel)
                            {
                                await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                                await ClaimLevelUpRewards(Level);
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
                }
                catch (PokeHashException ex)
                {
                    AccountState = AccountState.HashIssues;
                    LogCaller(new LoggerEventArgs("Hash service exception occured. Restarting ...", LoggerTypes.Exception, ex));
                }
                catch (Exception ex)
                {
                    LogCaller(new LoggerEventArgs("Unknown exception occured. Restarting ...", LoggerTypes.Exception, ex));
                    //LogCaller(new LoggerEventArgs("Unknown exception occured. Stopping ...", LoggerTypes.Exception, ex));
                    //Stop();
                }

                #endregion

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
            if (!_client.ClientSession.AccessToken.IsExpired)
            {
                return new MethodResult
                {
                    Success = true
                };
            }

            try
            {
                LogCaller(new LoggerEventArgs("Session expired. Logging back in", LoggerTypes.Debug));

                await _client.DoLogin(_client.ClientManager);

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
                    Latitude = 45.03009,
                    Longitude = -93.31934,
                    Name = "6Pokestop, Cleveland"
                },

                new FarmLocation
                {
                    Latitude = 35.696428,
                    Longitude = 139.814404,
                    Name = "9Lures, Tokyo Japan"
                },

                new FarmLocation
                {
                    Latitude = 40.755184,
                    Longitude = -73.983724,
                    Name = "7Pokestops, Central Park NY"
                },

                new FarmLocation
                {
                    Latitude = 51.22505600,
                    Longitude = 6.80713000,
                    Name = "Dusseldorf, Germany"
                },

                new FarmLocation
                {
                    Latitude = 46.50759600,
                    Longitude = 6.62834800,
                    Name = "Lausanne, Suisse"
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
