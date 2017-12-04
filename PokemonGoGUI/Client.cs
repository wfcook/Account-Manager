#region using directives

using Newtonsoft.Json;
using POGOLib.Official;
using POGOLib.Official.Extensions;
using POGOLib.Official.Logging;
using POGOLib.Official.LoginProviders;
using POGOLib.Official.Net;
using POGOLib.Official.Net.Authentication;
using POGOLib.Official.Net.Authentication.Data;
using POGOLib.Official.Net.Captcha;
using POGOLib.Official.Util.Device;
using POGOLib.Official.Util.Hash;
using PokemonGoGUI.Enums;
using PokemonGoGUI.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;
using static POGOProtos.Networking.Envelopes.Signature.Types;

#endregion

namespace PokemonGoGUI
{
    public class Client
    {
        public ProxyEx Proxy;
        public ISettings Settings { get; private set; }

        public AuthType AuthType
        { get { return Settings.AuthType; } private set { Settings.AuthType = value; } }

        public AccessToken AccessToken { get; private set; }
        public Session ClientSession { get; private set; }

        public bool LoggedIn { get; private set; }

        public LocaleInfo LocaleInfo { get; private set; }

        public DeviceWrapper ClientDeviceWrapper { get; private set; }

        public uint VersionInt = 8300;
        public string VersionStr = "0.83.2";

        public void Logout()
        {
            ClientSession.Shutdown();
            LoggedIn = false;
            AccessToken = null;
        }

        public Client()
        {
        }

        public async Task<MethodResult<bool>> DoLogin(ISettings settings)
        {
            SetSettings(settings);
            Configuration.Hasher = new PokeHashHasher(Settings.AuthAPIKey);
            Configuration.IgnoreHashVersion = true;

            ILoginProvider loginProvider;

            switch (Settings.AuthType)
            {
                case AuthType.Google:
                    loginProvider = new GoogleLoginProvider(Settings.GoogleUsername, Settings.GooglePassword);
                    break;
                case AuthType.Ptc:
                    loginProvider = new PtcLoginProvider(Settings.PtcUsername, Settings.PtcPassword);
                    break;
                default:
                    throw new ArgumentException("Login provider must be either \"google\" or \"ptc\".");
            }

            ClientSession = await GetSession(loginProvider, Settings.DefaultLatitude, Settings.DefaultLongitude, true);

            SaveAccessToken(ClientSession.AccessToken);

            ClientSession.AccessTokenUpdated += SessionOnAccessTokenUpdated;
            ClientSession.InventoryUpdate += InventoryOnUpdate;
            ClientSession.MapUpdate += MapOnUpdate;
            ClientSession.CaptchaReceived += SessionOnCaptchaReceived;

            // Send initial requests and start HeartbeatDispatcher.
            // This makes sure that the initial heartbeat request finishes and the "session.Map.Cells" contains stuff.
            string msgStr = null;
            if (!await ClientSession.StartupAsync())
            {
                msgStr = "Session couldn't start up.";
                LoggedIn = false;
            }
            else
            {
                LoggedIn = true;
                msgStr = "Successfully logged into server.";
            }
            return new MethodResult<bool>()
            {
                Success = LoggedIn,
                Message = msgStr
            };
        }

        private void SessionOnCaptchaReceived(object sender, CaptchaEventArgs e)
        {
            var session = (Session)sender;

            Logger.Warn("Captcha received: " + e.CaptchaUrl);

            // Solve
            //            var verifyChallengeResponse = await session.RpcClient.SendRemoteProcedureCallAsync(new Request
            //            {
            //                RequestType = RequestType.VerifyChallenge,
            //                RequestMessage = new VerifyChallengeMessage
            //                {
            //                    Token = "token"
            //                }.ToByteString()
            //            }, false);
            //
            //            var verifyChallenge = VerifyChallengeResponse.Parser.ParseFrom(verifyChallengeResponse);
            //            
            //            Console.WriteLine(JsonConvert.SerializeObject(verifyChallenge, Formatting.Indented));
        }

        private void SessionOnAccessTokenUpdated(object sender, EventArgs e)
        {
            var session = (Session)sender;

            SaveAccessToken(session.AccessToken);

            Logger.Info("Saved access token to file.");
        }

        private void InventoryOnUpdate(object sender, EventArgs e)
        {
            Logger.Info("Inventory was updated.");
        }

        private void MapOnUpdate(object sender, EventArgs e)
        {
            Logger.Info("Map was updated.");
        }
     
        public void SetSettings(ISettings settings)
        {
            Settings = settings;

            int osId = OsVersions[Settings.FirmwareType.Length].Length;
            var firmwareUserAgentPart = OsUserAgentParts[osId];
            var firmwareType = OsVersions[osId];

            ClientDeviceWrapper = new DeviceWrapper
            {
                UserAgent = $"pokemongo/1 {firmwareUserAgentPart}",
                DeviceInfo = new DeviceInfo
                {
                    DeviceId = Settings.DeviceId,
                    DeviceBrand = Settings.DeviceBrand,
                    DeviceModelBoot = Settings.DeviceModelBoot,
                    HardwareModel = Settings.HardwareModel,
                    HardwareManufacturer = Settings.HardwareManufacturer,
                    FirmwareBrand = Settings.FirmwareBrand,
                    FirmwareType = Settings.FirmwareType,
                    AndroidBoardName = Settings.AndroidBoardName,
                    AndroidBootloader = Settings.AndroidBootloader,
                    DeviceModel = Settings.DeviceModel,
                    DeviceModelIdentifier = Settings.DeviceModelIdentifier,
                    FirmwareFingerprint = Settings.FirmwareFingerprint,
                    FirmwareTags = Settings.FirmwareTags
                }
            };

            Proxy = new ProxyEx
            {
                Address = Settings.ProxyIP,
                Port = Settings.ProxyPort,
                Username = Settings.ProxyUsername,
                Password = Settings.ProxyPassword
            };

            LocaleInfo = new LocaleInfo();
            LocaleInfo.SetValues(Settings.Country, Settings.Language, Settings.TimeZone, Settings.POSIX);
        }

        private void SaveAccessToken(AccessToken accessToken)
        {
            var fileName = Path.Combine(Directory.GetCurrentDirectory(), "Cache", $"{accessToken.Uid}.json");

            File.WriteAllText(fileName, JsonConvert.SerializeObject(accessToken, Formatting.Indented));
            AccessToken = accessToken;
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
                        return Login.GetSession(loginProvider, accessToken, initLat, initLong, ClientDeviceWrapper);
                }
            }

            var session = await Login.GetSession(loginProvider, initLat, initLong, ClientDeviceWrapper);

            if (mayCache)
                SaveAccessToken(session.AccessToken);

            return session;
        }

        private static readonly string[] OsUserAgentParts = {
            "CFNetwork/758.0.2 Darwin/15.0.0",  // 9.0
            "CFNetwork/758.0.2 Darwin/15.0.0",  // 9.0.1
            "CFNetwork/758.0.2 Darwin/15.0.0",  // 9.0.2
            "CFNetwork/758.1.6 Darwin/15.0.0",  // 9.1
            "CFNetwork/758.2.8 Darwin/15.0.0",  // 9.2
            "CFNetwork/758.2.8 Darwin/15.0.0",  // 9.2.1
            "CFNetwork/758.3.15 Darwin/15.4.0", // 9.3
            "CFNetwork/758.4.3 Darwin/15.5.0", // 9.3.2
            "CFNetwork/807.2.14 Darwin/16.3.0", // 10.3.3
            "CFNetwork/889.3 Darwin/17.2.0", // 11.1.0
            "CFNetwork/893.10 Darwin/17.3.0", // 11.2.0
        };

        private static readonly string[][] Devices =
        {
            new[] {"iPad5,1", "iPad", "J96AP"},
            new[] {"iPad5,2", "iPad", "J97AP"},
            new[] {"iPad5,3", "iPad", "J81AP"},
            new[] {"iPad5,4", "iPad", "J82AP"},
            new[] {"iPad6,7", "iPad", "J98aAP"},
            new[] {"iPad6,8", "iPad", "J99aAP"},
            new[] {"iPhone5,1", "iPhone", "N41AP"},
            new[] {"iPhone5,2", "iPhone", "N42AP"},
            new[] {"iPhone5,3", "iPhone", "N48AP"},
            new[] {"iPhone5,4", "iPhone", "N49AP"},
            new[] {"iPhone6,1", "iPhone", "N51AP"},
            new[] {"iPhone6,2", "iPhone", "N53AP"},
            new[] {"iPhone7,1", "iPhone", "N56AP"},
            new[] {"iPhone7,2", "iPhone", "N61AP"},
            new[] {"iPhone8,1", "iPhone", "N71AP"},
            new[] {"iPhone8,2", "iPhone", "MKTM2"}, //iphone 6s plus
            new[] {"iPhone9,3", "iPhone", "MN9T2"}
        };

        private static readonly string[] OsVersions = {
            "9.0",
            "9.0.1",
            "9.0.2",
            "9.1",
            "9.2",
            "9.2.1",
            "9.3",
            "9.3.2",
            "10.3.3",
            "11.1.0",
            "11.2.0"
        };
    }
}