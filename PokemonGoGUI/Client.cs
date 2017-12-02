#region using directives

using System;
using System.Net;
using POGOProtos.Enums;
using POGOProtos.Networking.Envelopes;
using POGOProtos.Settings;
using System.Threading.Tasks;
using System.Net.Http;
using POGOLib.Official.LoginProviders;
using POGOLib.Official.Net;
using POGOLib.Official.Net.Authentication;
using POGOLib.Official.Net.Authentication.Data;
using POGOLib.Official.Util.Hash;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using POGOLib.Official.Extensions;
using POGOLib.Official.Net.Captcha;
using PokemonGoGUI.Enums;
using POGOLib.Official;
using System.IO;
using Newtonsoft.Json;
using POGOLib.Official.Logging;
using System.Linq;
using Google.Protobuf;

#endregion

namespace PokemonGoGUI
{
    public class Client
    {
        public ProxyEx Proxy;
        public ISettings Settings { get; private set; }

        public AuthType AuthType
        { get { return Settings.AuthType; } set { Settings.AuthType = value; } }

        public AccessToken AccessToken { get; set; }
        public Session Session { get; set; }

        public bool LoggedIn { get; set; }

        public void Logout()
        {
            Session.Shutdown();
            LoggedIn = false;
            AccessToken = null;
        }

        public Client()
        {
        }

        public async Task<MethodResult> DoLogin(ISettings settings)
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

            Session = await GetSession(loginProvider, Settings.DefaultLatitude, Settings.DefaultLongitude, true);

            SaveAccessToken(Session.AccessToken);

            Session.AccessTokenUpdated += SessionOnAccessTokenUpdated;
            Session.InventoryUpdate += InventoryOnUpdate;
            Session.MapUpdate += MapOnUpdate;
            Session.CaptchaReceived += SessionOnCaptchaReceived;

            // Send initial requests and start HeartbeatDispatcher.
            // This makes sure that the initial heartbeat request finishes and the "session.Map.Cells" contains stuff.
            if (!await Session.StartupAsync())
            {
                throw new Exception("Session couldn't start up.");
            }

            return new MethodResult
            {
                Message = "Successfully logged into server.",
                Success = true
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

        
        public async Task<MethodResult> ReAuthenticate()
        {
            await DoLogin(Settings);

            return new MethodResult
            {
                Message = "Successfully reauthenticated.",
                Success = true
            };
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
        }

        private void SaveAccessToken(AccessToken accessToken)
        {
            var fileName = Path.Combine(Directory.GetCurrentDirectory(), "Cache", $"{accessToken.Uid}.json");

            File.WriteAllText(fileName, JsonConvert.SerializeObject(accessToken, Formatting.Indented));
            AccessToken = accessToken;
        }

        /// <summary>
        /// Login to PokémonGo and return an authenticated <see cref="Session" />.
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

            LoggedIn = true;

            return session;
        }
    }
}