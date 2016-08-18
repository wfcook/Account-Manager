using Newtonsoft.Json;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PokemonGo.RocketAPI.Login
{
    class PtcLogin : ILoginType
    {
        readonly string password;
        readonly string username;
        readonly ISettings settings;
        readonly CookieContainer cookies = new CookieContainer();
        readonly int defaultTimeout = 10000;
        readonly ProxyEx defaultProxy;
        string ip = IPGenerate.GenerateIP();

        public PtcLogin(string username, string password, ISettings settings)
        {
            this.username = username;
            this.password = password;
            this.settings = settings;

            defaultProxy = new ProxyEx
            {
                Address = settings.ProxyIP,
                Port = settings.ProxyPort,
                Username = settings.ProxyUsername,
                Password = settings.ProxyPassword
            };
        }

        public async Task<string> GetAccessToken()
        {
            //Get session cookie
            var sessionData = await GetSessionCookie();//.ConfigureAwait(false);

            //Login
            var ticketId = await GetLoginTicket(username, password, sessionData);//.ConfigureAwait(false);

            //Get tokenvar
            return await GetToken(ticketId);//.ConfigureAwait(false);
        }

        private string ExtractTicketFromResponse(string contentResponse, WebHeaderCollection headers)
        {
            if (!String.IsNullOrEmpty(contentResponse))
            {
                dynamic responseObject = JsonConvert.DeserializeObject<dynamic>(contentResponse.Trim());

                if(responseObject["errors"] != null)
                {
                    foreach(dynamic error in responseObject["errors"])
                    {
                        if(error.Value.Contains("Your username or password is incorrect"))
                        {
                            throw new InvalidCredentialsException(error.Value);
                        }
                        else if (error.Value.Contains("As a security measure, your account has been disabled"))
                        {
                            throw new InvalidCredentialsException(error.Value);
                        }
                        else if (error.Value.Contains("Username is a required field"))
                        {
                            throw new InvalidCredentialsException(error.Value);
                        }
                        else if (error.Value.Contains("Password is a required field"))
                        {
                            throw new InvalidCredentialsException(error.Value);
                        }
                        else if (error.Value.Contains("Account is not yet active"))
                        {
                            throw new AccountNotVerifiedException();
                        }
                    }
                }
            }

            Uri location = null;

            for (int i = 0; i < headers.Count; ++i)
            {
                string header = headers.GetKey(i);

                if(header == "Location")
                {
                    location = new Uri(headers.GetValues(i)[0]);
                }
            }

            if (location == null)
                throw new LoginFailedException();

            var ticketId = HttpUtility.ParseQueryString(location.Query)["ticket"];

            if (ticketId == null)
                throw new PtcOfflineException();

            return ticketId;
        }

        private static NameValueCollection GenerateLoginRequest(SessionData sessionData, string user, string pass)
        {
            return new NameValueCollection
            {
                { "lt", sessionData.Lt },
                { "execution", sessionData.Execution },
                { "_eventId", "submit" },
                { "username", user },
                { "password", pass }
            };
        }

        private static NameValueCollection GenerateTokenVarRequest(string ticketId)
        {
            return new NameValueCollection
            {
                {"client_id", "mobile-app_pokemon-go"},
                {"redirect_uri", "https://www.nianticlabs.com/pokemongo/error"},
                {"client_secret", "w8ScCUXJQc6kXKw8FiOhd8Fixzht18Dq3PEVkUCP5ZPxtgyWsbTvWHFLm2wNY0JR"},
                {"grant_type", "refresh_token"},
                {"code", ticketId}
            };
        }

        private async Task<string> GetLoginTicket(string username, string password, SessionData sessionData)
        {
            var loginRequest = GenerateLoginRequest(sessionData, username, password);

            using(WebClientEx wc = new WebClientEx())
            {
                wc.CookieContainer = cookies;
                wc.Timeout = defaultTimeout;
                wc.Proxy = defaultProxy.AsWebProxy();

                string response = Encoding.UTF8.GetString(await wc.UploadValuesTaskAsync(Resources.PtcLoginUrl, loginRequest));

                var ticketId = ExtractTicketFromResponse(response, wc.ResponseHeaders);

                return ticketId;
            }
        }

        private async Task<SessionData> GetSessionCookie()
        {
            using(WebClientEx wc = new WebClientEx())
            {
                wc.CookieContainer = cookies;
                wc.Timeout = defaultTimeout;
                wc.Proxy = defaultProxy.AsWebProxy();

                string response = await wc.DownloadStringTaskAsync(Resources.PtcLoginUrl);
                SessionData sessionData = JsonConvert.DeserializeObject<SessionData>(response);

                return sessionData;
            }
        }

        private async Task<string> GetToken(string ticketId)
        {
            var tokenRequest = GenerateTokenVarRequest(ticketId);

            using(WebClientEx wc = new WebClientEx())
            {
                wc.CookieContainer = cookies;
                wc.Timeout = defaultTimeout;
                wc.Proxy = defaultProxy.AsWebProxy();

                string tokenData = Encoding.UTF8.GetString(await wc.UploadValuesTaskAsync(Resources.PtcLoginOauth, tokenRequest));

                return HttpUtility.ParseQueryString(tokenData)["access_token"];
            }
        }

        private class SessionData
        {
            public string Lt { get; set; }
            public string Execution { get; set; }
        }
    }
}