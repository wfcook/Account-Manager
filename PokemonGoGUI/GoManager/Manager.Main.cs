using System.Globalization;
using Newtonsoft.Json;
using POGOProtos.Data.Player;
using POGOProtos.Enums;
using POGOProtos.Map.Fort;
using PokemonGoGUI.AccountScheduler;
using PokemonGoGUI.Enums;
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
using System.Windows.Forms;
using POGOLib.Official.Exceptions;
using PokemonGoGUI.Captcha;
using POGOLib.Official.Extensions;
using POGOLib.Official.Net;

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
        private int _failedPokestopResponse = 0;
        private bool _autoRestart = false;
        private bool _wasAutoRestarted = false;
        private ManualResetEvent _pauser = new ManualResetEvent(true);
        private DateTime TimeAutoCatch = DateTime.Now;
        private bool CatchDisabled = false;

        public bool _proxyIssue = false;
        //Manager captcha solver
        public CaptchaManager CaptchaSolver = new CaptchaManager();

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
            Tracker = new Tracker();

            ProxyHandler = handler;

            LoadFarmLocations();
        }

        public async Task<MethodResult> AcLogin()
        {
            LogCaller(new LoggerEventArgs("Attempting to login ...", LoggerTypes.Debug));
            AccountState = AccountState.Conecting;

            MethodResult result = await _client.DoLogin(this);

            if (result == null)
                Stop();

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

                if (CurrentProxy != null)
                {
                    ProxyHandler.ResetFailCounter(CurrentProxy);
                }
            }

            return result;
        }

        public MethodResult Start()
        {
            if (IsRunning || AccountState == AccountState.Conecting)
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

            State = BotState.Starting;

            //Fixing a bug on my part
            if (Tracker == null)
            {
                Tracker = new Tracker();
            }

            ServicePointManager.DefaultConnectionLimit = Int32.MaxValue;

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
            if (!IsRunning || State == BotState.Pausing || State == BotState.Paused)
            {
                return;
            }

            _pauser.Reset();
            _runningStopwatch.Stop();
            //_client.ClientSession.Pause();

            LogCaller(new LoggerEventArgs("Pausing bot ...", LoggerTypes.Info));

            State = BotState.Pausing;
        }

        public void UnPause()
        {
            if (!IsRunning || State != BotState.Paused)
            {
                return;
            }

            _pauser.Set();
            _runningStopwatch.Start();

            //if (_client.ClientSession.State == SessionState.Paused)
            //    await _client.ClientSession.ResumeAsync();

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

            if (_client?.ClientSession?.State == SessionState.Paused)
            {
                LogCaller(new LoggerEventArgs("Bot paused", LoggerTypes.Info));

                State = BotState.Paused;

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
            AccountState = AccountState.Good;

            while (IsRunning)
            {
                if (CheckTime())
                {
                    Stop();
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
                }

                StartingUp = true;

                if (currentFails >= UserSettings.MaxFailBeforeReset)
                {
                    currentFails = 0;
                    _client.Logout();
                }

                if (_failedInventoryReponses >= _failedInventoryUntilBan)
                {
                    AccountState = AccountState.PermanentBan;
                    LogCaller(new LoggerEventArgs("Potential account ban", LoggerTypes.Warning));
                    Stop();
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

                    result = await CheckReauthentication();

                    if (!result.Success)
                    {
                        LogCaller(new LoggerEventArgs("Echo failed. Logging out before retry.", LoggerTypes.Debug));

                        Stop();
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
                            }
                        }
                        catch (Exception ex1)
                        {
                            //if (AccountState != AccountState.CaptchaReceived || AccountState != AccountState.HashIssues)
                            //    AccountState = AccountState.TemporalBan;
                            LogCaller(new LoggerEventArgs("Exception: " + ex1, LoggerTypes.Debug));
                            LogCaller(new LoggerEventArgs("Game settings failed", LoggerTypes.FatalError, new Exception("Maybe this account is banned ...")));
                            Stop();
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
                            //if (AccountState != AccountState.CaptchaReceived || AccountState != AccountState.HashIssues)
                            //    AccountState = AccountState.TemporalBan;
                            LogCaller(new LoggerEventArgs("Load pokemon settings failed", LoggerTypes.FatalError, new Exception("Maybe this account is banned ...")));
                            Stop();
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
                            }

                            LogCaller(new LoggerEventArgs("Marking startup tutorials completed.", LoggerTypes.Success));

                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                        }
                    }

                    _failedInventoryReponses = 0;

                    WaitPaused();

                    //End startup phase
                    StartingUp = false;

                    //Prevent changing back to running state
                    if (State != BotState.Stopping)
                    {
                        State = BotState.Running;
                    }

                    //Update location
                    if (_firstRun)
                    {
                        LogCaller(new LoggerEventArgs("Setting default location ...", LoggerTypes.Debug));

                        result = await UpdateLocation(new GeoCoordinate(UserSettings.Latitude, UserSettings.Longitude));

                        UpdateInventory(InventoryRefresh.All);

                        if (!result.Success)
                        {
                            Stop();
                        }
                    }

                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                    #endregion

                    #region PokeStopTask

                    //Get pokestops
                    LogCaller(new LoggerEventArgs("getting pokestops...", LoggerTypes.Debug));

                    MethodResult<List<FortData>> pokestops = GetAllForts();

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

                    int currentFailedStops = 0;

                    var pokestopsToFarm = new Queue<FortData>(pokestops.Data);

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
                            Stop();
                        }

                        FortData pokestop = pokestopsToFarm.Dequeue();
                        LogCaller(new LoggerEventArgs("Fort Dequeued: " + pokestop.Id, LoggerTypes.Debug));

                        if (UserSettings.GoOnlyToGyms && pokestop.Type != FortType.Gym)
                            continue;

                        var currentLocation = new GeoCoordinate(_client.ClientSession.Player.Latitude, _client.ClientSession.Player.Longitude);
                        var fortLocation = new GeoCoordinate(pokestop.Latitude, pokestop.Longitude);

                        double distance = CalculateDistanceInMeters(currentLocation, fortLocation);

                        string fort = "pokestop";
                        LoggerTypes loggerTypes = LoggerTypes.Info;

                        if (pokestop.Type == FortType.Gym && Level >= 5 && !UserSettings.DefaultTeam.Equals("Neutral") && !String.IsNullOrEmpty(UserSettings.DefaultTeam))
                        {
                            if (!UserSettings.SpinGyms)
                                continue;

                            fort = "Gym";
                            loggerTypes = LoggerTypes.Gym;
                        }

                        LogCaller(new LoggerEventArgs(String.Format("Going to {0} {1} of {2}. Distance {3:0.00}m", fort, pokeStopNumber, totalStops, distance), loggerTypes));

                        //Go to pokestops
                        MethodResult walkResult = await GoToLocation(new GeoCoordinate(pokestop.Latitude, pokestop.Longitude));

                        if (!walkResult.Success)
                        {
                            LogCaller(new LoggerEventArgs("Too many failed walking attempts. Restarting to fix ...", LoggerTypes.Warning));
                            LogCaller(new LoggerEventArgs("Result: " + walkResult.Message, LoggerTypes.Debug));
                            Stop();
                        }

                        await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

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
                                if (PlayerData.MaxPokemonStorage > Pokemon.Count)
                                {
                                    //Catch nearby pokemon
                                    MethodResult nearbyPokemonResponse = await CatchNeabyPokemon();
                                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                                    //Catch incense pokemon
                                    MethodResult incensePokemonResponse = await CatchInsencePokemon();
                                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                                    //Catch lured pokemon
                                    MethodResult luredPokemonResponse = await CatchLuredPokemon(pokestop);
                                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                                    //Check sniping NearyPokemon
                                    MethodResult Snipe = await SnipeAllNearyPokemon();
                                    if (Snipe.Success)
                                    {
                                        await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                                        pokestopsToFarm.Clear();
                                        pokestopsToFarm = new Queue<FortData>(GetAllForts().Data);
                                        continue;
                                    }
                                }
                                else
                                {
                                    LogCaller(new LoggerEventArgs("You inventory pokemon storage is full please transfer some pokemons.", LoggerTypes.Warning));
                                }
                            }
                            else
                            {
                                LogCaller(new LoggerEventArgs("You don't have any pokeball catching pokemon will be disabled during " + UserSettings.DisableCatchDelay.ToString(CultureInfo.InvariantCulture) + " minutes.", LoggerTypes.Info));
                                CatchDisabled = true;
                                TimeAutoCatch = DateTime.Now.AddMinutes(UserSettings.DisableCatchDelay);
                            }
                        }

                        //Stop bot instantly
                        if (!IsRunning)
                        {
                            break;
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
                                if (pokestop.Type == FortType.Gym && Level >= 5 && (!string.IsNullOrEmpty(UserSettings.DefaultTeam) || UserSettings.DefaultTeam != "Neutral"))
                                {
                                    if (!PlayerData.TutorialState.Contains(TutorialState.GymTutorial))
                                    {
                                        if (PlayerData.Team == TeamColor.Neutral)
                                        {
                                            TeamColor team = TeamColor.Neutral;

                                            foreach (TeamColor _team in Enum.GetValues(typeof(TeamColor)))
                                            {
                                                if (UserSettings.DefaultTeam == _team.ToString())
                                                {
                                                    team = _team;
                                                }
                                            }

                                            if (team != TeamColor.Neutral)
                                            {
                                                var setplayerteam = await SetPlayerTeam(team);

                                                if (setplayerteam.Success)
                                                {
                                                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                                                    result = await MarkTutorialsComplete(new[] { TutorialState.GymTutorial });

                                                    if (!result.Success)
                                                    {
                                                        LogCaller(new LoggerEventArgs("Failed. Marking Gym tutorials completed..", LoggerTypes.Warning));
                                                        continue;
                                                    }

                                                    LogCaller(new LoggerEventArgs("Marking Gym tutorials completed.", LoggerTypes.Success));

                                                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                                                }

                                                //Check for missed tutorials
                                                foreach (TutorialState tutos in Enum.GetValues(typeof(TutorialState)))
                                                {
                                                    if (!PlayerData.TutorialState.Contains(tutos))
                                                    {
                                                        DialogResult box = MessageBox.Show($"Tutorial {tutos.ToString()} is not completed on this account {PlayerData.Username}! Complete this?", "Confirmation", MessageBoxButtons.YesNo);

                                                        if (box == DialogResult.Yes)
                                                        {
                                                            result = await MarkTutorialsComplete(new[] { tutos });
                                                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                                                        }
                                                    }
                                                }

                                                //First spin gym after complet tuto
                                                var gyminfo = await GymGetInfo(pokestop);
                                                if (gyminfo.Success)
                                                    LogCaller(new LoggerEventArgs("Gym Name: " + gyminfo.Data.Name, LoggerTypes.Info));
                                                else
                                                    continue;

                                                MethodResult spingym = await SearchPokestop(pokestop);

                                                //OutOfRange will show up as a success
                                                if (spingym.Success)
                                                {
                                                    currentFailedStops = 0;
                                                    //Try to deploy, full gym is 6 now
                                                    if (gyminfo.Data.GymStatusAndDefenders.GymDefender.Count < 6)
                                                    {
                                                        //Checks team color if same of player or Neutral
                                                        if (pokestop.OwnedByTeam == PlayerData.Team || pokestop.OwnedByTeam == TeamColor.Neutral)
                                                        {
                                                            //Check if config as deploy actived
                                                            if (UserSettings.DeployPokemon)
                                                            {
                                                                //Try to deploy
                                                                await GymDeploy(pokestop);
                                                            }
                                                        }
                                                    }
                                                    //Here try to attack gym not released yet
                                                    //
                                                }
                                                else
                                                {
                                                    if (currentFailedStops > 10)
                                                    {
                                                        Stop();
                                                    }
                                                    ++currentFailedStops;
                                                }

                                            }
                                        }
                                    }
                                    else if (PlayerData.TutorialState.Contains(TutorialState.GymTutorial))
                                    {
                                        var gyminfo = await GymGetInfo(pokestop);
                                        if (gyminfo.Success)
                                            LogCaller(new LoggerEventArgs("Gym Name: " + gyminfo.Data.Name, LoggerTypes.Info));
                                        else
                                            continue;

                                        MethodResult spingym = await SearchPokestop(pokestop);

                                        //OutOfRange will show up as a success
                                        if (spingym.Success)
                                        {
                                            currentFailedStops = 0;
                                            //Try to deploy, full gym is 6 now
                                            if (gyminfo.Data.GymStatusAndDefenders.GymDefender.Count < 6)
                                            {
                                                //Checks team color if same of player or Neutral
                                                if (pokestop.OwnedByTeam == PlayerData.Team || pokestop.OwnedByTeam == TeamColor.Neutral)
                                                {
                                                    //Check if config as deploy actived
                                                    if (UserSettings.DeployPokemon)
                                                    {
                                                        //Try to deploy
                                                        await GymDeploy(pokestop);
                                                    }
                                                }
                                            }
                                            //Here try to attack gym not released yet
                                            //
                                        }
                                        else
                                        {
                                            if (currentFailedStops > 10)
                                            {
                                                Stop();
                                            }
                                            ++currentFailedStops;
                                        }
                                    }
                                }
                                else
                                {
                                    if (!PlayerData.TutorialState.Contains(TutorialState.PokestopTutorial))
                                    {
                                        result = await MarkTutorialsComplete(new[] { TutorialState.PokestopTutorial, TutorialState.PokemonBerry, TutorialState.UseItem });

                                        if (!result.Success)
                                        {
                                            LogCaller(new LoggerEventArgs("Failed. Marking pokestop, pokemonberry, useitem, pokemoncapture tutorials completed..", LoggerTypes.Warning));

                                            Stop();
                                        }

                                        LogCaller(new LoggerEventArgs("Marking pokestop, pokemonberry, useitem, pokemoncapture tutorials completed.", LoggerTypes.Success));

                                        await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                                    }

                                    var fortDetails = await FortDetails(pokestop);
                                    if (fortDetails.Success)
                                        LogCaller(new LoggerEventArgs("Fort Name: " + fortDetails.Data.Name, LoggerTypes.Info));
                                    else
                                        continue;

                                    MethodResult searchResult = await SearchPokestop(pokestop);

                                    //OutOfRange will show up as a success
                                    if (searchResult.Success)
                                    {
                                        currentFailedStops = 0;
                                    }
                                    else
                                    {
                                        if (currentFailedStops > 10)
                                        {
                                            Stop();
                                        }
                                        ++currentFailedStops;
                                    }
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
                            break;
                        }

                        //Check account client session state
                        if (AccountState != AccountState.CaptchaReceived && (_client.ClientSession.State == SessionState.Paused || _client.ClientSession.State == SessionState.Stopped))
                            await _client.ClientSession.ResumeAsync();

                        // evolve, transfer, etc on first and every 10 stops
                        if (IsRunning && ((pokeStopNumber > 4 && pokeStopNumber % 10 == 0) || pokeStopNumber == 1))
                        {
                            // clean account state
                            if (AccountState != AccountState.Flagged || AccountState != AccountState.SoftBan)
                                AccountState = AccountState.Good;

                            MethodResult echoResult = await CheckReauthentication();

                            //Echo failed, restart
                            if (!echoResult.Success)
                            {
                                Stop();
                            }

                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                            if (UserSettings.TransferPokemon)
                            {
                                MethodResult transferResult = await TransferFilteredPokemon();

                                if (transferResult.Success)
                                {
                                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                                }
                            }

                            if (UserSettings.EvolvePokemon)
                            {
                                MethodResult evolveResult = await EvolveFilteredPokemon();

                                if (evolveResult.Success)
                                {
                                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                                }
                            }

                            if (UserSettings.UpgradePokemon)
                            {
                                MethodResult upgradeResult = await UpgradeFilteredPokemon();

                                if (upgradeResult.Success)
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

                            UpdateInventory(InventoryRefresh.All); //all inventory
                        }

                        WaitPaused();

                        UpdateInventory(InventoryRefresh.Stats);

                        if (Level > prevLevel)
                        {
                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                            await ClaimLevelUpRewards(Level);
                        }

                        ++pokeStopNumber;

                        await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                        if (UserSettings.MaxLevel > 0 && Level >= UserSettings.MaxLevel)
                        {
                            LogCaller(new LoggerEventArgs(String.Format("Max level of {0} reached.", UserSettings.MaxLevel), LoggerTypes.Info));

                            Stop();
                        }

                        if (_totalZeroExpStops > 25)
                        {
                            LogCaller(new LoggerEventArgs("Potential PokeStop SoftBan.", LoggerTypes.Warning));
                            AccountState = AccountState.SoftBan;
                            // reset values
                            _totalZeroExpStops = 0;
                            Stop();
                        }

                        if (_potentialPokeStopBan)
                        {
                            if (_failedPokestopResponse >= 10)
                            {
                                AccountState = AccountState.SoftBan;
                                _failedPokestopResponse = 0;
                                LogCaller(new LoggerEventArgs("Potential PokeStop SoftBan or daily limit reached. Stoping ...", LoggerTypes.Warning));
                                Stop();
                            }
                            else
                                //Break out of pokestop loop to test for ip ban
                                break;
                        }

                        if (Tracker.PokemonCaught >= UserSettings.CatchPokemonDayLimit && Tracker.PokestopsFarmed >= UserSettings.SpinPokestopsDayLimit)
                        {
                            LogCaller(new LoggerEventArgs("Daily limits reached. Stoping ...", LoggerTypes.Warning));
                            Stop();
                        }
                    }
                }
                catch (StackOverflowException ex)
                {
                    AccountState = AccountState.Unknown;
                    LogCaller(new LoggerEventArgs(ex.Message, LoggerTypes.FatalError));
                    Stop();
                }
                catch (HashVersionMismatchException ex)
                {
                    AccountState = AccountState.Unknown;
                    LogCaller(new LoggerEventArgs(ex.Message, LoggerTypes.FatalError));
                    Stop();
                }
                catch (GoogleLoginException ex)
                {
                    AccountState = AccountState.Unknown;
                    LogCaller(new LoggerEventArgs(ex.Message, LoggerTypes.FatalError));
                    Stop();
                }
                catch (PtcLoginException ex)
                {
                    AccountState = AccountState.Unknown;
                    LogCaller(new LoggerEventArgs(ex.Message, LoggerTypes.FatalError));
                    Stop();
                }
                catch (TaskCanceledException ex)
                {
                    AccountState = AccountState.Unknown;
                    LogCaller(new LoggerEventArgs("TaskCanceledException. Restarting ...", LoggerTypes.Warning, ex));
                }
                catch (OperationCanceledException ex)
                {
                    AccountState = AccountState.Unknown;
                    LogCaller(new LoggerEventArgs("OperationCanceledException. Restarting ...", LoggerTypes.Warning, ex));
                }
                catch (APIBadRequestException ex)
                {
                    LogCaller(new LoggerEventArgs("API Bad Request. Restarting ...", LoggerTypes.Warning, ex));
                }
                catch (InvalidPlatformException ex)
                {
                    LogCaller(new LoggerEventArgs("Invalid Platform or token session refresh. Restarting  ...", LoggerTypes.Warning, ex));
                }
                catch (SessionInvalidatedException ex)
                {
                    LogCaller(new LoggerEventArgs("Session Invalidated or token session refresh. Restarting ...", LoggerTypes.Warning, ex));
                }
                catch (PokeHashException ex)
                {
                    AccountState = AccountState.HashIssues;
                    LogCaller(new LoggerEventArgs($"Hash service exception occured. Restarting ...", LoggerTypes.Warning, ex));
                }
                catch (SessionUnknowException ex)
                {
                    AccountState = AccountState.Unknown;
                    LogCaller(new LoggerEventArgs("Skipping request. Restarting ...", LoggerTypes.Exception, ex));
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    AccountState = AccountState.Unknown;
                    LogCaller(new LoggerEventArgs("Skipping request. Restarting ...", LoggerTypes.Exception, ex));
                }
                catch (SessionStateException ex)
                {
                    AccountState = AccountState.Unknown;
                    LogCaller(new LoggerEventArgs("SessionStateException. Restarting ...", LoggerTypes.Exception, ex));
                    _client.CleanLocalAccesToken();
                }
                catch (Exception ex)
                {
                    LogCaller(new LoggerEventArgs("Unknown exception occured. Restarting ...", LoggerTypes.Exception, ex));
                }

                #endregion

                currentFails = 0;
                _firstRun = false;
            }

            State = BotState.Stopped;
            Stop();
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

            //Remove proxy
            if (UserSettings.AutoRemoveOnStop)
                RemoveProxy();

            _pauser.Set();
            _runningStopwatch.Stop();
            _failedInventoryReponses = 0;

            if (!_autoRestart)
            {
                _runningStopwatch.Reset();
            }

            IsRunning = false;
            _firstRun = true;
        }

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

                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    Stop();
                    return new MethodResult();
                }

                return new MethodResult
                {
                    Success = true
                };
            }
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
