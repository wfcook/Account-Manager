#region using directives

using Newtonsoft.Json;
using POGOLib.Official;
using POGOLib.Official.Logging;
using POGOLib.Official.LoginProviders;
using POGOLib.Official.Net;
using POGOLib.Official.Net.Authentication;
using POGOLib.Official.Net.Authentication.Data;
using POGOLib.Official.Net.Captcha;
using POGOLib.Official.Util.Hash;
using PokemonGoGUI.Enums;
using PokemonGoGUI.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;

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
                        return Login.GetSession(loginProvider, accessToken, initLat, initLong);
                }
            }

            var session = await Login.GetSession(loginProvider, initLat, initLong);

            if (mayCache)
                SaveAccessToken(session.AccessToken);

            return session;
        }
    }
}