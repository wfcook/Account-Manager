#region using directives

using Newtonsoft.Json;
using POGOLib.Official;
using POGOLib.Official.Exceptions;
using POGOLib.Official.Logging;
using POGOLib.Official.LoginProviders;
using POGOLib.Official.Net;
using POGOLib.Official.Net.Authentication;
using POGOLib.Official.Net.Authentication.Data;
using POGOLib.Official.Net.Captcha;
using POGOLib.Official.Util.Device;
using POGOLib.Official.Util.Hash;
using POGOProtos.Data;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using PokemonGoGUI.Enums;
using PokemonGoGUI.Exceptions;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using static POGOProtos.Networking.Envelopes.Signature.Types;

#endregion

namespace PokemonGoGUI
{
    public class Client
    {
        public ProxyEx Proxy;
        public Version VersionStr;
        public uint AppVersion;
        public Session ClientSession;
        public bool LoggedIn = false;
        public GetPlayerMessage.Types.PlayerLocale PlayerLocale;
        private DeviceWrapper ClientDeviceWrapper;
        public Manager ClientManager;
        private string RessourcesFolder;
        private event EventHandler<int> OnPokehashSleeping;

        public Client()
        {
            VersionStr = new Version("0.89.1");
            AppVersion = 8900;
            RessourcesFolder = $"data/{VersionStr.ToString()}/";
        }

        public void Logout()
        {
            if (!LoggedIn)
                return;

            if (ClientManager.AccountState == AccountState.Conecting)
                ClientManager.AccountState = AccountState.Good;

            LoggedIn = false;
            ClientSession.AssetDigestUpdated -= OnAssetDisgestReceived;
            ClientSession.ItemTemplatesUpdated -= OnItemTemplatesReceived;
            ClientSession.UrlsUpdated -= OnDownloadUrlsReceived;
            ClientSession.RemoteConfigVersionUpdated -= OnLocalConfigVersionReceived;
            ClientSession.AccessTokenUpdated -= SessionAccessTokenUpdated;
            ClientSession.CaptchaReceived -= SessionOnCaptchaReceived;
            ClientSession.InventoryUpdate -= SessionInventoryUpdate;
            ClientSession.MapUpdate -= SessionMapUpdate;
            ClientSession.CheckAwardedBadgesReceived -= OnCheckAwardedBadgesReceived;
            ClientSession.HatchedEggsReceived -= OnHatchedEggsReceived;
            ClientSession.TemporalBanReceived -= TempBanReceived;
            ClientSession.BuddyWalked -= OnBuddyWalked;
            ClientSession.InboxDataReceived -= OnInboxDataReceived;

            if (ClientSession.State != SessionState.Stopped)
                ClientSession.Shutdown();
        }

        private void LoggerFucntion(LogLevel logLevel, string message)
        {
            var logtype = LoggerTypes.Debug;
            switch (logLevel)
            {
                case LogLevel.Warn:
                    logtype = LoggerTypes.Warning;
                    break;
                case LogLevel.Error:
                    logtype = LoggerTypes.FatalError;
                    break;
                case LogLevel.Info:
                    logtype = LoggerTypes.Info;
                    break;
                case LogLevel.Notice:
                    logtype = LoggerTypes.Success;
                    break;
            }

            ClientManager.LogCaller(new LoggerEventArgs(message, logtype));
        }
        public async Task<MethodResult<bool>> DoLogin(Manager manager)
        {
            SetSettings(manager);
            // TODO: see how do this only once better.
            if (!(Configuration.Hasher is PokeHashHasher))
            {
                // By default Configuration.Hasher is LegacyHasher type  (see Configuration.cs in the pogolib source code)
                // -> So this comparation only will run once.
                if (ClientManager.UserSettings.UseOnlyOneKey)
                {
                    Configuration.Hasher = new PokeHashHasher(ClientManager.UserSettings.AuthAPIKey);
                    Configuration.HasherUrl = ClientManager.UserSettings.HashHost;
                    Configuration.HashEndpoint = ClientManager.UserSettings.HashEndpoint;
                }
                else
                {
                    //Need valid keys but this send all
                    Configuration.Hasher = new PokeHashHasher(ClientManager.UserSettings.HashKeys.ToArray());
                }

                // TODO: make this configurable. To avoid bans (may be with a checkbox in hash keys tab).
                //Configuration.IgnoreHashVersion = true;
                VersionStr = Configuration.Hasher.PokemonVersion;
                AppVersion = Configuration.Hasher.AppVersion;                
                // TODO: Revise sleeping
                // Used on Windows phone background app
                ((PokeHashHasher)Configuration.Hasher).PokehashSleeping += OnPokehashSleeping;
            }
            // */

            ILoginProvider loginProvider;

            switch (ClientManager.UserSettings.AuthType)
            {
                case AuthType.Google:
                    loginProvider = new GoogleLoginProvider(ClientManager.UserSettings.Username, ClientManager.UserSettings.Password);
                    break;
                case AuthType.Ptc:
                    loginProvider = new PtcLoginProvider(ClientManager.UserSettings.Username, ClientManager.UserSettings.Password, Proxy.AsWebProxy());
                    break;
                default:
                    throw new ArgumentException("Login provider must be either \"google\" or \"ptc\".");
            }

            ClientSession = await GetSession(loginProvider, ClientManager.UserSettings.Latitude, ClientManager.UserSettings.Longitude, true);

            // Send initial requests and start HeartbeatDispatcher.
            // This makes sure that the initial heartbeat request finishes and the "session.Map.Cells" contains stuff.
            var msgStr = "Session couldn't start up.";
            LoggedIn = false;
            try
            {
                ClientSession.AssetDigestUpdated += OnAssetDisgestReceived;
                ClientSession.ItemTemplatesUpdated += OnItemTemplatesReceived;
                ClientSession.UrlsUpdated += OnDownloadUrlsReceived;
                ClientSession.RemoteConfigVersionUpdated += OnLocalConfigVersionReceived;
                ClientSession.AccessTokenUpdated += SessionAccessTokenUpdated;
                ClientSession.CaptchaReceived += SessionOnCaptchaReceived;
                ClientSession.InventoryUpdate += SessionInventoryUpdate;
                ClientSession.MapUpdate += SessionMapUpdate;
                ClientSession.CheckAwardedBadgesReceived += OnCheckAwardedBadgesReceived;
                ClientSession.HatchedEggsReceived += OnHatchedEggsReceived;
                ClientSession.TemporalBanReceived += TempBanReceived;
                ClientSession.BuddyWalked += OnBuddyWalked;
                ClientSession.InboxDataReceived += OnInboxDataReceived;

                ClientSession.Logger.RegisterLogOutput(LoggerFucntion);

                ClientSession.ManageRessources = ClientManager.UserSettings.DownloadResources;
                ClientManager.LogCaller(new LoggerEventArgs("Succefully added all events to the client.", LoggerTypes.Debug));

                if (await ClientSession.StartupAsync())
                {
                    LoggedIn = true;
                    msgStr = "Successfully logged into server.";

                    if (ClientSession.Player.Warn)
                    {
                        ClientManager.AccountState = AccountState.Flagged;
                        ClientManager.LogCaller(new LoggerEventArgs("The account is flagged.", LoggerTypes.Warning));

                        if (ClientManager.UserSettings.StopAtMinAccountState == AccountState.Flagged)
                        {
                            //Remove proxy
                            ClientManager.RemoveProxy();
                            ClientManager.Stop();

                            msgStr = "The account is flagged.";
                        }
                    }

                    if (ClientSession.Player.Banned)
                    {
                        ClientManager.AccountState = AccountState.PermanentBan;
                        ClientManager.LogCaller(new LoggerEventArgs("The account is banned.", LoggerTypes.FatalError));

                        //Remove proxy
                        ClientManager.RemoveProxy();

                        ClientManager.Stop();

                        msgStr = "The account is banned.";
                    }

                    SaveAccessToken(ClientSession.AccessToken);
                }
            }
            catch (HashVersionMismatchException ex)
            {
                ClientManager.LogCaller(new LoggerEventArgs(ex.Message, LoggerTypes.Warning));
                ClientManager.Stop();
            }
            catch (APIBadRequestException ex)
            {
                ClientManager.LogCaller(new LoggerEventArgs($"API {ex.Message}, this account is maybe banned ...", LoggerTypes.Warning));
            }
            catch (InvalidPlatformException)// ex
            {
                ClientManager.LogCaller(new LoggerEventArgs("Invalid Platform, continue  ...", LoggerTypes.Warning));
            }
            catch (SessionInvalidatedException)// ex
            {
                ClientManager.LogCaller(new LoggerEventArgs("Session Invalidated, continue ...", LoggerTypes.Warning));
            }
            catch (SessionUnknowException)
            {
                ClientManager.AccountState = AccountState.Unknown;
                msgStr = "Skipping request. Restarting ...";
            }
            catch (ArgumentOutOfRangeException)
            {
                ClientManager.AccountState = AccountState.Unknown;
                msgStr = "Skipping request";
            }
            catch (SessionStateException ex)
            {
                if (ClientSession.State == SessionState.TemporalBanned)
                {
                    //Remove proxy
                    ClientManager.RemoveProxy();

                    ClientManager.Stop();

                    msgStr = ex.Message;
                }
                else
                    ClientManager.LogCaller(new LoggerEventArgs(ex.Message, LoggerTypes.Warning));
            }
            catch (PtcLoginException) // poex
            {
                ClientManager.Stop();

                ClientManager.LogCaller(new LoggerEventArgs("Ptc server offline. Please try again later.", LoggerTypes.Warning));

                msgStr = "Ptc server offline.";
            }
            catch (AccountNotVerifiedException) // anvex
            {
                ClientManager.Stop();
                ClientManager.RemoveProxy();

                ClientManager.LogCaller(new LoggerEventArgs("Account not verified. Stopping ...", LoggerTypes.Warning));

                ClientManager.AccountState = Enums.AccountState.NotVerified;

                msgStr = "Account not verified.";
            }
            catch (WebException wex)
            {
                ClientManager.Stop();

                if (wex.Status == WebExceptionStatus.Timeout)
                {
                    if (String.IsNullOrEmpty(ClientManager.Proxy))
                    {
                        ClientManager.LogCaller(new LoggerEventArgs("Login request has timed out.", LoggerTypes.Warning));
                    }
                    else
                    {
                        ClientManager._proxyIssue = true;
                        ClientManager.LogCaller(new LoggerEventArgs("Login request has timed out. Possible bad proxy.", LoggerTypes.ProxyIssue));
                    }

                    msgStr = "Request has timed out.";
                }

                if (!String.IsNullOrEmpty(ClientManager.Proxy))
                {
                    if (wex.Status == WebExceptionStatus.ConnectionClosed)
                    {
                        ClientManager._proxyIssue = true;
                        ClientManager.LogCaller(new LoggerEventArgs("Potential http proxy detected. Only https proxies will work.", LoggerTypes.ProxyIssue));

                        msgStr = "Http proxy detected";
                    }
                    else if (wex.Status == WebExceptionStatus.ConnectFailure || wex.Status == WebExceptionStatus.ProtocolError || wex.Status == WebExceptionStatus.ReceiveFailure
                        || wex.Status == WebExceptionStatus.ServerProtocolViolation)
                    {
                        ClientManager._proxyIssue = true;
                        ClientManager.LogCaller(new LoggerEventArgs("Proxy is offline", LoggerTypes.ProxyIssue));

                        msgStr = "Proxy is offline";
                    }
                }

                ClientManager._proxyIssue |= !String.IsNullOrEmpty(ClientManager.Proxy);

                ClientManager.LogCaller(new LoggerEventArgs("Failed to login due to request error", LoggerTypes.Exception, wex.InnerException));

                msgStr = "Failed to login due to request error";
            }
            catch (TaskCanceledException) // tce
            {
                ClientManager.Stop();

                if (String.IsNullOrEmpty(ClientManager.Proxy))
                {
                    ClientManager.LogCaller(new LoggerEventArgs("Login request has timed out", LoggerTypes.Warning));
                }
                else
                {
                    ClientManager._proxyIssue = true;
                    ClientManager.LogCaller(new LoggerEventArgs("Login request has timed out. Possible bad proxy", LoggerTypes.ProxyIssue));
                }

                msgStr = "Login request has timed out";
            }
            catch (InvalidCredentialsException icex)
            {
                //Puts stopping log before other log.
                ClientManager.Stop();
                ClientManager.RemoveProxy();

                ClientManager.LogCaller(new LoggerEventArgs("Invalid credentials or account lockout. Stopping bot...", LoggerTypes.Warning, icex));

                msgStr = "Username or password incorrect";
            }
            catch (IPBannedException) // ipex
            {
                if (ClientManager.UserSettings.StopOnIPBan)
                {
                    ClientManager.Stop();
                }

                string message = String.Empty;

                if (!String.IsNullOrEmpty(ClientManager.Proxy))
                {
                    if (ClientManager.CurrentProxy != null)
                    {
                        ClientManager.ProxyHandler.MarkProxy(ClientManager.CurrentProxy, true);
                    }

                    message = "Proxy IP is banned.";
                }
                else
                {
                    message = "IP address is banned.";
                }

                ClientManager._proxyIssue = true;

                ClientManager.LogCaller(new LoggerEventArgs(message, LoggerTypes.ProxyIssue));

                msgStr = message;
            }
            catch (GoogleLoginException glex)
            {
                ClientManager.Stop();
                ClientManager.RemoveProxy();

                ClientManager.LogCaller(new LoggerEventArgs(glex.Message, LoggerTypes.Warning));

                msgStr = "Failed to login";
            }
            catch (ArgumentNullException) // anex
            {
                //ClientManager.AccountState = AccountState.TemporalBan;
                ClientManager.Stop();
                ClientManager.RemoveProxy();
                msgStr = "Argument Null Exception.";
            }
            catch (PokeHashException phex)
            {
                ClientManager.AccountState = AccountState.HashIssues;
                msgStr = "Hash issues";
                ClientManager.LogCaller(new LoggerEventArgs("Hash issues", LoggerTypes.FatalError, phex));
            }
            catch (Exception ex)
            {
                ClientManager.Stop();
                //RemoveProxy();

                ClientManager.LogCaller(new LoggerEventArgs("Failed to login", LoggerTypes.Exception, ex));

                msgStr = "Failed to login";
            }

            return new MethodResult<bool>()
            {
                Success = LoggedIn,
                Message = msgStr,
            };
        }

        private void OnBuddyWalked(object sender, GetBuddyWalkedResponse data)
        {
            ClientManager.LogCaller(new LoggerEventArgs($"Buddy Walked CandyID: {data.FamilyCandyId}, Candy Count: {data.CandyEarnedCount}", LoggerTypes.Buddy));
        }

        private void OnInboxDataReceived(object sender, GetInboxResponse data)
        {
            // Inbox data 
        }

        private void OnAssetDisgestReceived(object sender, List<AssetDigestEntry> data)
        {
            if (!Directory.Exists(RessourcesFolder))
                Directory.CreateDirectory(RessourcesFolder);

            var filename = RessourcesFolder + ClientManager.UserSettings.DeviceId + "_AD.json";

            try
            {
                File.WriteAllText(filename, Serializer.ToJson(data));
            }
            catch (Exception)
            {
                ClientManager.LogCaller(new LoggerEventArgs("AssetDigests could not be saved sucessfully", LoggerTypes.Warning));
            }
        }

        private void OnItemTemplatesReceived(object sender, List<DownloadItemTemplatesResponse.Types.ItemTemplate> data)
        {
            if (!Directory.Exists(RessourcesFolder))
                Directory.CreateDirectory(RessourcesFolder);

            var filename = RessourcesFolder + ClientManager.UserSettings.DeviceId + "_IT.json";

            try
            {
                File.WriteAllText(filename, Serializer.ToJson(data));
            }
            catch (Exception)
            {
                ClientManager.LogCaller(new LoggerEventArgs("ItemTemplates could not be saved sucessfully", LoggerTypes.Warning));
            }
        }

        private void OnDownloadUrlsReceived(object sender, List<DownloadUrlEntry> data)
        {
            if (!Directory.Exists(RessourcesFolder))
                Directory.CreateDirectory(RessourcesFolder);

            var filename = RessourcesFolder + ClientManager.UserSettings.DeviceId + "_UR.json";

            try
            {
                File.WriteAllText(filename, Serializer.ToJson(data));
            }
            catch (Exception)
            {
                ClientManager.LogCaller(new LoggerEventArgs("Urls could not be saved sucessfully", LoggerTypes.Warning));
            }
        }

        private void OnLocalConfigVersionReceived(object sender, DownloadRemoteConfigVersionResponse data)
        {
            if (!Directory.Exists(RessourcesFolder))
                Directory.CreateDirectory(RessourcesFolder);

            var filename = RessourcesFolder + ClientManager.UserSettings.DeviceId + "_LCV.json";

            try
            {
                File.WriteAllText(filename, Serializer.ToJson(data));
            }
            catch (Exception)
            {
                ClientManager.LogCaller(new LoggerEventArgs("LocalConfigVersion could not be saved sucessfully", LoggerTypes.Warning));
            }
        }

        private void PokehashSleeping(object sender, int sleepTime)
        {
            OnPokehashSleeping?.Invoke(sender, sleepTime);
        }

        private void SessionMapUpdate(object sender, EventArgs e)
        {
            // Update Map
        }

        public void SessionOnCaptchaReceived(object sender, CaptchaEventArgs e)
        {
            AccountState accountState = ClientManager.AccountState;

            ClientManager.AccountState = AccountState.CaptchaReceived;

            ClientManager.LogCaller(new LoggerEventArgs("Captcha received.", LoggerTypes.Warning));

            ClientManager.LogCaller(new LoggerEventArgs("Bot paused VerifyChallenge...", LoggerTypes.Captcha));

            bool solved = false;
            //int retries = 1;

            //while (retries-- > 0 && !solved)
            //{
            solved = ClientManager.CaptchaSolver.SolveCaptcha(this, e.CaptchaUrl).Result;
            //}
            if (solved)
            {
                ClientManager.LogCaller(new LoggerEventArgs("Unpausing bot Challenge finished...", LoggerTypes.Captcha));
                ClientManager.AccountState = accountState;
                return;
            }

            ClientManager.Stop();
        }

        private void SessionInventoryUpdate(object sender, EventArgs e)
        {
            //TODO: review needed here            
            //ClientManager.UpdateInventory(0); // <- this line should be the unique line updating the inventory
        }

        private void OnHatchedEggsReceived(object sender, GetHatchedEggsResponse hatchedEggResponse)
        {
            //
        }

        private void OnCheckAwardedBadgesReceived(object sender, CheckAwardedBadgesResponse e)
        {
            //
        }

        private void SessionAccessTokenUpdated(object sender, EventArgs e)
        {
            SaveAccessToken(ClientSession.AccessToken);
        }

        private void TempBanReceived(object sender, EventArgs e)
        {
            ClientManager.AccountState = AccountState.TemporalBan;
            ClientManager.Stop();
        }

        public void SetSettings(Manager manager)
        {
            ClientManager = manager;

            Proxy = new ProxyEx
            {
                Address = ClientManager.UserSettings.ProxyIP,
                Port = ClientManager.UserSettings.ProxyPort,
                Username = ClientManager.UserSettings.ProxyUsername,
                Password = ClientManager.UserSettings.ProxyPassword
            };

            Dictionary<string, string> Header = new Dictionary<string, string>()
            {
                {"11.1.0", "CFNetwork/889.3 Darwin/17.2.0"},
                {"11.2.0", "CFNetwork/893.10 Darwin/17.3.0"},
                {"11.2.5", "CFNetwork/893.14.2 Darwin/17.4.0"}
            };

            ClientDeviceWrapper = new DeviceWrapper
            {
                UserAgent = $"pokemongo/1 {Header[ClientManager.UserSettings.FirmwareType]}",
                DeviceInfo = new DeviceInfo
                {
                    DeviceId = ClientManager.UserSettings.DeviceId,
                    DeviceBrand = ClientManager.UserSettings.DeviceBrand,
                    DeviceModel = ClientManager.UserSettings.DeviceModel,
                    DeviceModelBoot = ClientManager.UserSettings.DeviceModelBoot,
                    HardwareManufacturer = ClientManager.UserSettings.HardwareManufacturer,
                    HardwareModel = ClientManager.UserSettings.HardwareModel,
                    FirmwareBrand = ClientManager.UserSettings.FirmwareBrand,
                    FirmwareType = ClientManager.UserSettings.FirmwareType
                },
                Proxy = Proxy.AsWebProxy()
            };

            PlayerLocale = new GetPlayerMessage.Types.PlayerLocale
            {
                Country = ClientManager.UserSettings.Country,
                Language = ClientManager.UserSettings.Language,
                Timezone = ClientManager.UserSettings.TimeZone
            };
        }

        private void SaveAccessToken(AccessToken accessToken)
        {
            var fileName = Path.Combine(Directory.GetCurrentDirectory(), "Cache", $"{accessToken.Uid}.json");
            File.WriteAllText(fileName, JsonConvert.SerializeObject(accessToken, Formatting.Indented));
        }

        /// <summary>
        /// Login to PokémonGo and return an authenticated <see cref="ClientSession" />.
        /// </summary>
        /// <param name="loginProvider">Provider must be PTC or Google.</param>
        /// <param name="initLat">The initial latitude.</param>
        /// <param name="initLong">The initial longitude.</param>
        /// <param name="mayCache">Can we cache the <see cref="AccessToken" /> to a local file?</param>
        private async Task<Session> GetSession(ILoginProvider loginProvider, double initLat, double initLong, bool mayCache = false)
        {
            var cacheDir = Path.Combine(Directory.GetCurrentDirectory(), "Cache");
            var fileName = Path.Combine(cacheDir, $"{loginProvider.UserId}-{loginProvider.ProviderId}.json");
            if (mayCache)
            {
                if (!Directory.Exists(cacheDir))
                    Directory.CreateDirectory(cacheDir);

                if (File.Exists(fileName))
                {
                    var accessToken = JsonConvert.DeserializeObject<AccessToken>(File.ReadAllText(fileName));

                    if (!accessToken.IsExpired)
                    {
                        var sess = Login.GetSession(loginProvider, accessToken, initLat, initLong, ClientDeviceWrapper, PlayerLocale);
                        LoadResources(sess);
                        return sess;
                    }
                }
            }

            var session = await Login.GetSession(loginProvider, initLat, initLong, ClientDeviceWrapper, PlayerLocale);
            LoadResources(session);

            if (mayCache)
                SaveAccessToken(session.AccessToken);

            return session;
        }

        private void LoadResources(Session session)
        {
            //My files resources here  
            var filename = RessourcesFolder + ClientManager.UserSettings.DeviceId + "_IT.json";
            if (File.Exists(filename))
                session.Templates.ItemTemplates = Serializer.FromJson<List<DownloadItemTemplatesResponse.Types.ItemTemplate>>(File.ReadAllText(filename));
            filename = RessourcesFolder +  ClientManager.UserSettings.DeviceId + "_UR.json";
            if (File.Exists(filename))
                session.Templates.DownloadUrls = Serializer.FromJson<List<DownloadUrlEntry>>(File.ReadAllText(filename));
            filename = RessourcesFolder + ClientManager.UserSettings.DeviceId + "_AD.json";
            if (File.Exists(filename))
                session.Templates.AssetDigests = Serializer.FromJson<List<AssetDigestEntry>>(File.ReadAllText(filename));
            filename = RessourcesFolder + ClientManager.UserSettings.DeviceId + "_LCV.json";
            if (File.Exists(filename))
                session.Templates.LocalConfigVersion = Serializer.FromJson<DownloadRemoteConfigVersionResponse>(File.ReadAllText(filename));
        }
    }
}
