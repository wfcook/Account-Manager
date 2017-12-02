#region using directives

using System;
using System.Net;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.HttpClient;
using PokemonGo.RocketAPI.Rpc;
using POGOProtos.Enums;
using POGOProtos.Networking.Envelopes;
using PokemonGo.RocketAPI.Helpers;
using PokemonGo.RocketAPI.Hash;
using PokemonGo.RocketAPI.Encrypt;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Authentication.Data;
using PokemonGo.RocketAPI.LoginProviders;
using POGOProtos.Settings;
using System.Threading.Tasks;
using System.Net.Http;

#endregion

namespace PokemonGo.RocketAPI
{
    public class Client : ICaptchaResponseHandler
    {
        public static ProxyEx Proxy;

        internal PokemonHttpClient PokemonHttpClient;
        public Download Download;
        public Encounter Encounter;
        public Fort Fort;
        public Inventory Inventory;
        public Login Login;
        public Map Map;
        public Misc Misc;
        public Player Player;
        string CaptchaToken;
        public KillSwitchTask KillswitchTask;
        public IHasher Hasher;
        public ICrypt Cryptor;
        internal RequestBuilder RequestBuilder;

        public ISettings Settings { get; private set; }

        public double CurrentLatitude { get; internal set; }
        public double CurrentLongitude { get; internal set; }
        public double CurrentAltitude { get; internal set; }
        public double CurrentAccuracy { get; internal set; }
        public float CurrentSpeed { get; internal set; }

        public AuthType AuthType
        { get { return Settings.AuthType; } set { Settings.AuthType = value; } }

        internal string ApiUrl { get; set; }
        internal AuthTicket AuthTicket { get; set; }

        internal string SettingsHash { get; set; }
        public GlobalSettings GlobalSettings { get; set; }
        public long InventoryLastUpdateTimestamp { get; set; }
        internal Platform Platform { get; set; }
        internal uint AppVersion { get; set; }
        internal string UnknownPlat8Field { get; set; }
        internal long Unknown25 { get; set; }
        public string ApiEndPoint { get; set; }
        public long StartTime { get; set; }
        public Version CurrentApiEmulationVersion { get; set; }
        public Version MinimumClientVersion { get; set; }        // This is version from DownloadSettings, but after login is updated from https://pgorelease.nianticlabs.com/plfe/version

        //public POGOLib.Net.Session AuthSession { get; set; }
        public ILoginProvider LoginProvider { get; set; }
        public AccessToken AccessToken { get; set; }
        //For Test
        //public int PageOffset { get; set; }
        //public string AuthToken { get; set; }

        public Client()
        {
        }

        public async Task<MethodResult> DoLogin(ISettings settings)
        {
            SetSettings(settings);

            await Login.DoLogin();

            return new MethodResult
            {
                Message = "Successfully logged into server.",
                Success = true
            };
        }

        public async Task<MethodResult> ReAuthenticate()
        {
            await Login.Reauthenticate(true);

            return new MethodResult
            {
                Message = "Successfully reauthenticated.",
                Success = true
            };
        }

        public void Logout()
        {
            AuthTicket = null;
            ApiUrl = null;
            //AuthToken = null;
            AccessToken = null;
        }

        public bool LoggedIn
        {
            get
            {
                if (Login == null)
                {
                    return false;
                }

                return Login.LoggedIn;
            }
        }

        public bool AuthExpired
        {
            get
            {
                if (AuthTicket == null)
                {
                    return true;
                }

                DateTime expired = DateTimeExtensions.GetDateTimeFromMilliseconds(AuthTicket.ExpireTimestampMs);

                //1 minute buffer
                return DateTime.UtcNow > expired.AddMinutes(-1);
            }
        }

        public void SetSettings(ISettings settings)
        {
            if (settings.UsePogoDevHashServer || settings.UseCustomAPI)
            {
                if (string.IsNullOrEmpty(settings.AuthAPIKey)) throw new AuthConfigException("You have selected Pogodev API but not provide proper API Key");

                Cryptor = new Cipher();

                ApiEndPoint = Constants.ApiEndPoint;
		
                Hasher = new PokefarmerHasher(settings, settings.AuthAPIKey, ApiEndPoint);

                // These 4 constants below need to change if we update the hashing server API version that is used.
                Unknown25 = Constants.Unknown25;
 				
                // WARNING! IF YOU CHANGE THE APPVERSION BELOW ALSO UPDATE THE API_VERSION AT THE TOP OF THE FILE!
                AppVersion = Constants.AppVersion; 
		
                CurrentApiEmulationVersion = new Version(Constants.API_VERSION);
                UnknownPlat8Field = Constants.UnknownPlat8Field;
            }
            /*
            else
            if (settings.UseLegacyAPI)
            {
                Hasher = new LegacyHashser();
                Cryptor = new LegacyCrypt();

                Unknown25 = -816976800928766045;// - 816976800928766045;// - 1553869577012279119;
                AppVersion = 4500;
                CurrentApiEmulationVersion = new Version("0.45.0");
            }
            */
            else
            {
                throw new AuthConfigException("No API method was selected in auth.json");
            }

            Proxy = new ProxyEx
            {
                Address = Settings.ProxyIP,
                Port = Settings.ProxyPort,
                Username = Settings.ProxyUsername,
                Password = Settings.ProxyPassword
            };

            HttpClientHandler handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = false,
                UseProxy = Proxy != null,
                Proxy = Proxy.AsWebProxy()
            };

            PokemonHttpClient = new PokemonHttpClient(handler);

            Settings = settings;
            Login = new Login(this);
            Player = new Player(this);
            Download = new Download(this);
            Inventory = new Inventory(this);
            Map = new Map(this);
            Fort = new Fort(this);
            Encounter = new Encounter(this);
            Misc = new Misc(this);
            KillswitchTask = new KillSwitchTask(this);

            Player.SetCoordinates(Settings.DefaultLatitude, Settings.DefaultLongitude, Settings.DefaultAltitude);
            Platform = settings.DevicePlatform.Equals("ios", StringComparison.Ordinal) ? Platform.Ios : Platform.Android;

            // We can no longer emulate Android so for now just overwrite settings with randomly generated iOS device info.
            if (Platform == Platform.Android)
            {
                Signature.Types.DeviceInfo iosInfo = DeviceInfoHelper.GetRandomIosDevice();
                settings.DeviceId = iosInfo.DeviceId;
                settings.DeviceBrand = iosInfo.DeviceBrand;
                settings.DeviceModel = iosInfo.DeviceModel;
                settings.DeviceModelBoot = iosInfo.DeviceModelBoot;
                settings.HardwareManufacturer = iosInfo.HardwareManufacturer;
                settings.HardwareModel = iosInfo.HardwareModel;
                settings.FirmwareBrand = iosInfo.FirmwareBrand;
                settings.FirmwareType = iosInfo.FirmwareType;

                // Clear out the android fields.
                settings.AndroidBoardName = "";
                settings.AndroidBootloader = "";
                settings.DeviceModelIdentifier = "";
                settings.FirmwareTags = "";
                settings.FirmwareFingerprint = "";

                // Now set the client platform to ios
                Platform = Platform.Ios;
            }
        }

        public void Reset()
        {
            AccessToken = null;
            //AuthToken = null;
            AuthTicket = null;
            StartTime = Utils.GetTime(true);
            RequestBuilder = new RequestBuilder(this, this.Settings);
            InventoryLastUpdateTimestamp = 0;
            SettingsHash = "";
        }

        public void SetCaptchaToken(string token)
        {
            CaptchaToken = token;
        }

        public bool CheckCurrentVersionOutdated()
        {
            if (MinimumClientVersion == null)
                return false;

            return CurrentApiEmulationVersion < MinimumClientVersion;
        }

        public static Version GetMinimumRequiredVersionFromUrl()
        {
            try
            {
                var Client = new WebClient();
                var version = Client.DownloadString(Constants.VersionUrl).Replace("\u0006", "").Replace("\n", "");
                return new Version(version);
            }
            catch (Exception ex)
            {
                var errorMessage = $"The Niantic version check URL ({Constants.VersionUrl}) is returning the following error(s): {ex.Message}";
                throw new HasherException(errorMessage);
            }
        }
    }
}