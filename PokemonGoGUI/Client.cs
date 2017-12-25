#region using directives

using GeoCoordinatePortable;
using Newtonsoft.Json;
using POGOLib.Official;
using POGOLib.Official.LoginProviders;
using POGOLib.Official.Net;
using POGOLib.Official.Net.Authentication;
using POGOLib.Official.Net.Authentication.Data;
using POGOLib.Official.Net.Captcha;
using POGOLib.Official.Util.Device;
using POGOLib.Official.Util.Hash;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using PokemonGoGUI.Enums;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager;
using PokemonGoGUI.GoManager.Models;
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
        public Version VersionStr;
        public Session ClientSession;
        public bool LoggedIn = false;
        private GetPlayerMessage.Types.PlayerLocale PlayerLocale;
        private DeviceWrapper ClientDeviceWrapper;
        private ISettings Settings;
        public Manager ClientManager;

        public Client()
        {
            VersionStr = new Version("0.87.5");
        }

        public void Logout()
        {
            if (!LoggedIn)
                return;
            LoggedIn = false;
            ClientSession.AccessTokenUpdated -= SessionOnAccessTokenUpdated;
            ClientSession.CaptchaReceived -= SessionOnCaptchaReceived;
            ClientSession.InventoryUpdate -= SessionInventoryUpdate;
            ClientSession.MapUpdate -= MapUpdate;
            ClientSession.CheckAwardedBadgesReceived -= OnCheckAwardedBadgesReceived;
            ClientSession.HatchedEggsReceived -= OnHatchedEggsReceived;
            ClientSession.Shutdown();
        }

        public async Task<MethodResult<bool>> DoLogin(Manager manager)
        {
            SetSettings(manager);
            // TODO: see how do this only once better.
            if (!(Configuration.Hasher is PokeHashHasher))
            {
                // By default Configuration.Hasher is LegacyHasher type  (see Configuration.cs in the pogolib source code)
                // -> So this comparation only will run once.
                if (Settings.UseOnlyOneKey)
                {
                    Configuration.Hasher = new PokeHashHasher(Settings.AuthAPIKey);
                    Configuration.HasherUrl = Settings.HashHost;
                    Configuration.HashEndpoint = Settings.HashEndpoint;
                }
                else
                    Configuration.Hasher = new PokeHashHasher(Settings.HashKeys.ToArray());

                // TODO: make this configurable. To avoid bans (may be with a checkbox in hash keys tab).
                //Configuration.IgnoreHashVersion = true;
                VersionStr = Configuration.Hasher.PokemonVersion;
                //Revise sleeping line 118
                //((PokeHashHasher)Configuration.Hasher).PokehashSleeping += OnPokehashSleeping;
            }
            // *****

            ILoginProvider loginProvider;

            switch (Settings.AuthType)
            {
                case AuthType.Google:
                    loginProvider = new GoogleLoginProvider(Settings.Username, Settings.Password);
                    break;
                case AuthType.Ptc:
                    loginProvider = new PtcLoginProvider(Settings.Username, Settings.Password, Proxy.AsWebProxy());
                    break;
                default:
                    throw new ArgumentException("Login provider must be either \"google\" or \"ptc\".");
            }

            ClientSession = await GetSession(loginProvider, Settings.DefaultLatitude, Settings.DefaultLongitude, true);

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

                ClientSession.AccessTokenUpdated += SessionOnAccessTokenUpdated;
                ClientSession.CaptchaReceived += SessionOnCaptchaReceived;
                ClientSession.InventoryUpdate += SessionInventoryUpdate;
                ClientSession.MapUpdate += MapUpdate;
                ClientSession.CheckAwardedBadgesReceived += OnCheckAwardedBadgesReceived;
                ClientSession.HatchedEggsReceived += OnHatchedEggsReceived;

                SaveAccessToken(ClientSession.AccessToken);
            }

            return new MethodResult<bool>()
            {
                Success = LoggedIn,
                Message = msgStr
            };
        }

        private event EventHandler<int> OnPokehashSleeping;

        private void PokehashSleeping(object sender, int sleepTime)
        {
            OnPokehashSleeping?.Invoke(sender, sleepTime);
        }

        private void MapUpdate(object sender, EventArgs e)
        {
            //var session = (Session)sender;
            //GeoCoordinate loc = new GeoCoordinate(session.Player.Latitude, session.Player.Longitude);
            //UpdateLocation(loc).Wait();

            ClientManager.GetPokeStops().Wait();
            ClientManager.GetCatchablePokemon().Wait();

            // Update BuddyPokemon Stats
            var buddyID =  ClientManager?.PlayerData?.BuddyPokemon.Id;
            if (buddyID != 0)
            {
                MethodResult<GetBuddyWalkedResponse> buddyWalkedResponse = ClientManager.GetBuddyWalked().Result;
                if (buddyWalkedResponse.Success)
                {
                    ClientManager.LogCaller(new LoggerEventArgs($"BuddyWalked CandyID: {buddyWalkedResponse.Data.FamilyCandyId}, CandyCount: {buddyWalkedResponse.Data.CandyEarnedCount}", LoggerTypes.Success));
                };
            }

        }

        public void SessionOnCaptchaReceived(object sender, CaptchaEventArgs e)
        {
            ClientManager.AccountState = AccountState.CaptchaReceived;
            //2captcha needed to solve or chrome drive for solve url manual
            //e.CaptchaUrl;
        }

        private void SessionInventoryUpdate(object sender, EventArgs e)
        {
            ClientManager.UpdateInventory().Wait();
        }

        private void OnHatchedEggsReceived(object sender, GetHatchedEggsResponse hatchedEggResponse)
        {
            //
        }

        private void OnCheckAwardedBadgesReceived(object sender, CheckAwardedBadgesResponse e)
        {
            //
        }

        private void SessionOnAccessTokenUpdated(object sender, EventArgs e)
        {
            SaveAccessToken(ClientSession.AccessToken);
        }

        public void SetSettings(Manager manager)
        {
            Settings = manager.UserSettings;
            ClientManager = manager;

            int osId = OsVersions[Settings.FirmwareType.Length].Length;
            var firmwareUserAgentPart = OsUserAgentParts[osId];
            var firmwareType = OsVersions[osId];

            Proxy = new ProxyEx
            {
                Address = Settings.ProxyIP,
                Port = Settings.ProxyPort,
                Username = Settings.ProxyUsername,
                Password = Settings.ProxyPassword
            };

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
                },
                Proxy = Proxy.AsWebProxy()
            };

            PlayerLocale = new GetPlayerMessage.Types.PlayerLocale
            {
                Country = Settings.Country,
                Language = Settings.Language,
                Timezone = Settings.TimeZone
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
                        return Login.GetSession(loginProvider, accessToken, initLat, initLong, ClientDeviceWrapper, PlayerLocale);
                }
            }

            var session = await Login.GetSession(loginProvider, initLat, initLong, ClientDeviceWrapper, PlayerLocale);

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
