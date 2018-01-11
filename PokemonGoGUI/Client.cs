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
using POGOLib.Official.Util.Hash.PokeHash;
using POGOProtos.Data;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using PokemonGoGUI.Enums;
using PokemonGoGUI.Exceptions;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager;
using PokemonGoGUI.GoManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using PokemonGoGUI.Captcha;
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
        private GetPlayerMessage.Types.PlayerLocale PlayerLocale;
        private DeviceWrapper ClientDeviceWrapper;
        public Manager ClientManager;

        public Client()
        {
            VersionStr = new Version("0.87.5");
            AppVersion = 8700;
        }

        public void Logout()
        {
            if (ClientManager.AccountState == AccountState.Conecting)
                ClientManager.AccountState = AccountState.Good;

            if (!LoggedIn)
                return;
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
                    Configuration.Hasher = new PokeHashHasher(ClientManager.UserSettings.HashKeys.ToArray());

                // TODO: make this configurable. To avoid bans (may be with a checkbox in hash keys tab).
                //Configuration.IgnoreHashVersion = true;
                VersionStr = Configuration.Hasher.PokemonVersion;
                AppVersion = Configuration.Hasher.AppVersion;
                // TODO: Revise sleeping
                // Used on Windows phone background app
                //((PokeHashHasher)Configuration.Hasher).PokehashSleeping += OnPokehashSleeping;
            }
            // *****

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

            ClientSession = await GetSession(loginProvider, ClientManager.UserSettings.Location.Latitude, ClientManager.UserSettings.Location.Longitude, true);

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
                ClientSession.Logger.RegisterLogOutput(LoggerFucntion);

                if (await ClientSession.StartupAsync(ClientManager.UserSettings.DownloadResources))
                {
                    LoggedIn = true;
                    msgStr = "Successfully logged into server.";

                    ClientManager.LogCaller(new LoggerEventArgs("Succefully added all events to the client.", LoggerTypes.Debug));

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

                    SaveAccessToken(ClientSession.AccessToken);
                }
                else
                {
                    if (ClientSession.Player.Banned)
                    {
                        ClientManager.AccountState = AccountState.PermanentBan;
                        ClientManager.LogCaller(new LoggerEventArgs("The account is banned.", LoggerTypes.FatalError));

                        //Remove proxy
                        ClientManager.RemoveProxy();

                        ClientManager.Stop();

                        msgStr = "The account is banned.";
                    }
                    if (ClientSession.State == SessionState.TemporalBanned)
                    {
                        ClientManager.AccountState = AccountState.TemporalBan;
                        ClientManager.LogCaller(new LoggerEventArgs("The account is banned temporally.", LoggerTypes.FatalError));

                        //Remove proxy
                        ClientManager.RemoveProxy();

                        ClientManager.Stop();

                        msgStr = "The account is banned temporally.";
                    }
                }
            }
            catch (PtcOfflineException ) // poex
            {
                ClientManager.Stop();

                ClientManager.LogCaller(new LoggerEventArgs("Ptc server offline. Please try again later.", LoggerTypes.Warning));

                msgStr = "Ptc server offline.";
            }
            catch (AccountNotVerifiedException ) // anvex
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
            catch (TaskCanceledException ) // tce
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
            catch (IPBannedException ) // ipex
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
                ClientManager.AccountState = AccountState.TemporalBan;
                ClientManager.Stop();
                ClientManager.RemoveProxy();
                msgStr = "This account is banned temporally.";
            }
            
            catch (PokeHashException phex)
            {
                ClientManager.AccountState = AccountState.HashIssues;

                msgStr = "Hash issues";
                ClientManager.LogCaller(new LoggerEventArgs(phex.Message, LoggerTypes.FatalError, phex));
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
                Message = msgStr
            };
        }

        private void OnAssetDisgestReceived(object sender, List<POGOProtos.Data.AssetDigestEntry> data)
        {
            var filename = "data/" + ClientManager.UserSettings.DeviceInfo.DeviceId + "_AD.json";
            if (!Directory.Exists("data"))
                Directory.CreateDirectory("data");
            try
            {
                File.WriteAllText(filename, Serializer.ToJson(data));
            }
            catch (Exception ex1)
            {
                ClientManager.LogCaller(new LoggerEventArgs("AssetDigests could not be saved sucessfully", LoggerTypes.Warning, ex1));
            }
        }

        private void OnItemTemplatesReceived(object sender, List<DownloadItemTemplatesResponse.Types.ItemTemplate> data)
        {
            if (!Directory.Exists("data"))
                Directory.CreateDirectory("data");
            var filename = "data/" + ClientManager.UserSettings.DeviceInfo.DeviceId + "_IT.json";
            try
            {
                File.WriteAllText(filename, Serializer.ToJson(data));
            }
            catch (Exception ex1)
            {
                ClientManager.LogCaller(new LoggerEventArgs("ItemTemplates could not be saved sucessfully", LoggerTypes.Warning, ex1));
            }
        }

        private void OnDownloadUrlsReceived(object sender, List<POGOProtos.Data.DownloadUrlEntry> data)
        {
            if (!Directory.Exists("data"))
                Directory.CreateDirectory("data");
            var filename = "data/" + ClientManager.UserSettings.DeviceInfo.DeviceId + "_UR.json";
            try
            {
                File.WriteAllText(filename, Serializer.ToJson(data));
            }
            catch (Exception ex1)
            {
                ClientManager.LogCaller(new LoggerEventArgs("Urls could not be saved sucessfully", LoggerTypes.Warning, ex1));
            }
        }

        private void OnLocalConfigVersionReceived(object sender, DownloadRemoteConfigVersionResponse data)
        {
            if (!Directory.Exists("data"))
                Directory.CreateDirectory("data");
            var filename = "data/" + ClientManager.UserSettings.DeviceInfo.DeviceId + "_LCV.json";
            try
            {
                File.WriteAllText(filename, Serializer.ToJson(data));
            }
            catch (Exception ex1)
            {
                ClientManager.LogCaller(new LoggerEventArgs("LocalConfigVersion could not be saved sucessfully", LoggerTypes.Warning, ex1));
            }
        }

        private event EventHandler<int> OnPokehashSleeping;

        private void PokehashSleeping(object sender, int sleepTime)
        {
            OnPokehashSleeping?.Invoke(sender, sleepTime);
        }

        private void SessionMapUpdate(object sender, EventArgs e)
        {
            // Update BuddyPokemon Stats
            //var msg = $"BuddyWalked Candy: {ClientSession.Player.BuddyCandy}";
            //ClientManager.LogCaller(new LoggerEventArgs(msg, LoggerTypes.Success));
        }

        public async void SessionOnCaptchaReceived(object sender, CaptchaEventArgs e)
        {
            ClientManager.LogCaller(new LoggerEventArgs("Captcha ceceived.", LoggerTypes.Warning));
            var resolved = await CaptchaManager.SolveCaptcha(ClientManager, e.CaptchaUrl.ToString());
            if (!resolved)
            {
                ClientManager.AccountState = AccountState.CaptchaReceived;
                ClientManager.Stop();
            }
        }

        private void SessionInventoryUpdate(object sender, EventArgs e)
        {
            //TODO: review needed here
            //ClientManager.UpdateInventory(); // <- this line should be the unique line updating the inventory
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

        public void SetSettings(Manager manager)
        {
            ClientManager = manager;

            int osId = OsVersions[ClientManager.UserSettings.DeviceInfo.FirmwareType.Length].Length;
            var firmwareUserAgentPart = OsUserAgentParts[osId];
            var firmwareType = OsVersions[osId];

            Proxy = new ProxyEx
            {
                Address = ClientManager.UserSettings.ProxyIP,
                Port = ClientManager.UserSettings.ProxyPort,
                Username = ClientManager.UserSettings.ProxyUsername,
                Password = ClientManager.UserSettings.ProxyPassword
            };

            ClientDeviceWrapper = new DeviceWrapper
            {
                UserAgent = $"pokemongo/1 {firmwareUserAgentPart}",
                DeviceInfo = new DeviceInfo
                {
                    DeviceId = ClientManager.UserSettings.DeviceInfo.DeviceId,
                    DeviceBrand = ClientManager.UserSettings.DeviceInfo.DeviceBrand,
                    DeviceModel = ClientManager.UserSettings.DeviceInfo.DeviceModel,
                    DeviceModelBoot = ClientManager.UserSettings.DeviceInfo.DeviceModelBoot,
                    HardwareManufacturer = ClientManager.UserSettings.DeviceInfo.HardwareManufacturer,
                    HardwareModel = ClientManager.UserSettings.DeviceInfo.HardwareModel,
                    FirmwareBrand = ClientManager.UserSettings.DeviceInfo.FirmwareBrand,
                    FirmwareType = ClientManager.UserSettings.DeviceInfo.FirmwareType
                },
                Proxy = Proxy.AsWebProxy()
            };

            PlayerLocale = new GetPlayerMessage.Types.PlayerLocale
            {
                Country = ClientManager.UserSettings.PlayerLocale.Country,
                Language = ClientManager.UserSettings.PlayerLocale.Language,
                Timezone = ClientManager.UserSettings.PlayerLocale.Timezone
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
            var filename = "data/" + ClientManager.UserSettings.DeviceInfo.DeviceId + "_IT.json";
            if (File.Exists(filename))
                session.Templates.ItemTemplates = Serializer.FromJson<List<DownloadItemTemplatesResponse.Types.ItemTemplate>>(File.ReadAllText(filename));
            filename = "data/" + ClientManager.UserSettings.DeviceInfo.DeviceId + "_UR.json";
            if (File.Exists(filename))
                session.Templates.DownloadUrls = Serializer.FromJson<List<DownloadUrlEntry>>(File.ReadAllText(filename));
            filename = "data/" + ClientManager.UserSettings.DeviceInfo.DeviceId + "_AD.json";
            if (File.Exists(filename))
                session.Templates.AssetDigests = Serializer.FromJson<List<AssetDigestEntry>>(File.ReadAllText(filename));
            filename = "data/" + ClientManager.UserSettings.DeviceInfo.DeviceId + "_LCV.json";
            if (File.Exists(filename))
                session.Templates.LocalConfigVersion = Serializer.FromJson<DownloadRemoteConfigVersionResponse>(File.ReadAllText(filename));
        }

        private static readonly string[] OsUserAgentParts = {
            "CFNetwork/889.3 Darwin/17.2.0",    // 11.1.0
            "CFNetwork/893.10 Darwin/17.3.0",   // 11.2.0
            "CFNetwork/893.14.2 Darwin/17.4.0"  // 11.2.5
        };

        private static readonly string[][] Devices =
        {
            new[] {"iPhone8,1", "iPhone", "N71AP"}, //iphone 6
            new[] {"iPhone8,2", "iPhone", "MKTM2"}, //iphone 6s plus
            new[] {"iPhone9,3", "iPhone", "MN9T2"}  //iphone 7
        };

        private static readonly string[] OsVersions = {
            "11.1.0",
            "11.2.0",
            "11.2.5"
        };
    }
}
