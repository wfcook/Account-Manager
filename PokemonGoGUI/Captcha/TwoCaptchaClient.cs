using PokemonGoGUI.Enums;
using PokemonGoGUI.GoManager;
using PokemonGoGUI.GoManager.Models;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PokemonGoGUI.Captcha
{
    public class TwoCaptchaClient
    {
        private string APIKey { get;  set; }

        public TwoCaptchaClient(string apiKey)
        {
            APIKey = apiKey;
        }

        /// <summary>
        /// Sends a solve request and waits for a response
        /// </summary>
        /// <param name="googleKey">The "sitekey" value from site your captcha is located on</param>
        /// <param name="pageUrl">The page the captcha is located on</param>
        /// <param name="proxy">The proxy used, format: "username:password@ip:port</param>
        /// <param name="proxyType">The type of proxy used</param>
        /// <param name="result">If solving was successful this contains the answer</param>
        /// <returns>Returns true if solving was successful, otherwise false</returns>
        public async Task<string> SolveRecaptchaV2(Client client, string googleKey, string pageUrl, string proxy, ProxyType proxyType)
        {
            string requestUrl = "http://2captcha.com/in.php?key=" + APIKey + "&method=userrecaptcha&googlekey=" + googleKey + "&pageurl=" + pageUrl + "&proxy=" + proxy + "&proxytype=";

            switch (proxyType)
            {
                case ProxyType.HTTP:
                    requestUrl += "HTTP";
                    break;
                case ProxyType.HTTPS:
                    requestUrl += "HTTPS";
                    break;
                case ProxyType.SOCKS4:
                    requestUrl += "SOCKS4";
                    break;
                case ProxyType.SOCKS5:
                    requestUrl += "SOCKS5";
                    break;
            }

            try
            {
                WebRequest req = WebRequest.Create(requestUrl);

                using (WebResponse resp = req.GetResponse())
                using (StreamReader read = new StreamReader(resp.GetResponseStream()))
                {
                    string response = read.ReadToEnd();

                    if (response.Length < 3)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        if (response.Substring(0, 3) == "OK|")
                        {
                            string captchaID = response.Remove(0, 3);
                            client.ClientManager.LogCaller(new LoggerEventArgs($"Captcha has been sent to 2Captcha, Your captcha ID :  {captchaID}", LoggerTypes.Info));

                            for (int i = 0; i < 29; i++)
                            {
                                if (client.ClientManager.AccountState != AccountState.CaptchaReceived)
                                    break;

                                WebRequest getAnswer = WebRequest.Create("http://2captcha.com/res.php?key=" + APIKey + "&action=get&id=" + captchaID);

                                using (WebResponse answerResp = getAnswer.GetResponse())
                                using (StreamReader answerStream = new StreamReader(answerResp.GetResponseStream()))
                                {
                                    string answerResponse = answerStream.ReadToEnd();

                                    if (answerResponse.Length < 3)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        if (answerResponse.Substring(0, 3) == "OK|")
                                        {
                                            return answerResponse.Remove(0, 3);
                                        }
                                        else if (answerResponse != "CAPCHA_NOT_READY")
                                        {
                                            return string.Empty;
                                        }
                                    }
                                    client.ClientManager.LogCaller(new LoggerEventArgs($"Waiting response captcha from 2Captcha workers...", LoggerTypes.Captcha));
                                }

                                await Task.Delay(3000);
                            }
                            return string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                client.ClientManager.LogCaller(new LoggerEventArgs($"2Captcha Error", LoggerTypes.Exception, ex));
            }
            return string.Empty;
        }
    }

    public enum ProxyType
    {
        HTTP,
        HTTPS,
        SOCKS4,
        SOCKS5
    }
}