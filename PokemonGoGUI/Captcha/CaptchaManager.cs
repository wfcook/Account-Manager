using System;
using System.IO;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using PokemonGoGUI.Captcha.Anti_Captcha;
using PokemonGoGUI.UI;
using PokemonGoGUI.GoManager;
using PokemonGoGUI.GoManager.Models;
using PokemonGoGUI.Extensions;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using Google.Protobuf;
using POGOProtos.Networking.Responses;

namespace PokemonGoGUI.Captcha
{
    public class CaptchaManager
    {
        const string POKEMON_GO_GOOGLE_KEY = "6LeeTScTAAAAADqvhqVMhPpr_vB9D364Ia-1dSgK";

        public static async Task<bool> SolveCaptcha(Client client, string captchaUrl)
        {
            string captchaResponse = "";
            bool resolved = false;
            bool needGetNewCaptcha = false;
            int retry = client.ClientManager.UserSettings.AutoCaptchaRetries;

            while (retry-- > 0 && !resolved)
            {
                //Use captcha solution to resolve captcha
                if (client.ClientManager.UserSettings.EnableCaptchaSolutions)
                {
                    if (needGetNewCaptcha)
                    {
                        captchaUrl = await GetNewCaptchaURL(client);
                    }
                    client.ClientManager.LogCaller(new LoggerEventArgs("Auto resolving captcha by using captcha solution service, please wait..........", LoggerTypes.Info));
                    CaptchaSolutionClient _client = new CaptchaSolutionClient(client.ClientManager.UserSettings.CaptchaSolutionAPIKey,
                        client.ClientManager.UserSettings.CaptchaSolutionsSecretKey, client.ClientManager.UserSettings.AutoCaptchaTimeout);
                    captchaResponse = await _client.ResolveCaptcha(POKEMON_GO_GOOGLE_KEY, captchaUrl, client);
                    needGetNewCaptcha = true;
                    if (!string.IsNullOrEmpty(captchaResponse))
                    {
                        resolved = await Resolve(client, captchaResponse);
                    }
                }         

                //use 2 captcha
                if (!resolved && client.ClientManager.UserSettings.Enable2Captcha &&
                    !string.IsNullOrEmpty(client.ClientManager.UserSettings.TwoCaptchaAPIKey))
                {
                    if (needGetNewCaptcha)
                    {
                        captchaUrl = await GetNewCaptchaURL(client);
                    }
                    if (string.IsNullOrEmpty(captchaUrl)) return true;

                    client.ClientManager.LogCaller(new LoggerEventArgs("Auto resolving captcha by using 2Captcha service", LoggerTypes.Info));
                    captchaResponse = await GetCaptchaResposeBy2Captcha(client, captchaUrl);
                    needGetNewCaptcha = true;
                    if (!string.IsNullOrEmpty(captchaResponse))
                    {
                        resolved = await Resolve(client, captchaResponse);
                    }
                }

                if (!resolved && client.ClientManager.UserSettings.EnableAntiCaptcha && !string.IsNullOrEmpty(client.ClientManager.UserSettings.AntiCaptchaAPIKey))
                {
                    if (needGetNewCaptcha)
                    {
                        captchaUrl = await GetNewCaptchaURL(client);
                    }
                    if (string.IsNullOrEmpty(captchaUrl)) return true;

                    client.ClientManager.LogCaller(new LoggerEventArgs("Auto resolving captcha by using anti captcha service", LoggerTypes.Info));
                    captchaResponse = await GetCaptchaResposeByAntiCaptcha(client, captchaUrl);
                    needGetNewCaptcha = true;
                    if (!string.IsNullOrEmpty(captchaResponse))
                    {
                        resolved = await Resolve(client, captchaResponse);
                    }
                }
            }

            //captchaRespose = "";
            if (!resolved)
            {
                if (needGetNewCaptcha)
                {
                    captchaUrl = await GetNewCaptchaURL(client);
                }

                if (client.ClientManager.UserSettings.PlaySoundOnCaptcha)
                {
                    SystemSounds.Asterisk.Play();
                }

                captchaResponse = GetCaptchaResposeManually(client, captchaUrl);
                //captchaResponse = await GetCaptchaTokenWithInternalForm(captchaUrl);

                if (!string.IsNullOrEmpty(captchaResponse))
                {
                    resolved = await Resolve(client, captchaResponse);
                }
            }

            return resolved;
        }


        //NOT WORKING  SINCE WEB BROWSER CONTROL IS IE7, doesn't work with captcha page

        private static async Task<string> GetCaptchaTokenWithInternalForm(string captchaUrl)
        {
            string response = "";
            var t = new Thread(() =>
            {
                CaptchaSolveForm captcha = new CaptchaSolveForm(captchaUrl);
                if (captcha.ShowDialog() == DialogResult.OK)
                {
                    response = "Aaa";
                }

                //captcha.TopMost = true;
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            int count = 120;
            while (true && count > 0)
            {
                count--;
                //Thread.Sleep(1000);
                await Task.Delay(1000);
            }

            return response;
        }

        private static async Task<string> GetNewCaptchaURL(Client client)
        {
            try
            {
                if (!client.LoggedIn)
                {
                    MethodResult result = await client.ClientManager.AcLogin();

                    if (!result.Success)
                    {
                        return string.Empty;
                    }
                }

                var response = await client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.CheckChallenge,
                    RequestMessage = new CheckChallengeMessage
                    {
                        //DebugRequest = 
                    }.ToByteString()
                }, false);

                string message = CheckChallengeResponse.Parser.ParseFrom(response).ChallengeUrl;
                return String.IsNullOrEmpty(message) ? String.Empty : message;
            }
            catch (Exception ex)
            {
                client.ClientManager.LogCaller(new LoggerEventArgs("Failed to get challenge", LoggerTypes.Exception, ex));
                return string.Empty;
            }
        }

        private static async Task<bool> Resolve(Client client, string captchaResponse)
        {
            if (string.IsNullOrEmpty(captchaResponse)) return false;
            try
            {
                if (!client.LoggedIn)
                {
                    MethodResult result = await client.ClientManager.AcLogin();

                    if (!result.Success)
                    {
                        return false;
                    }
                }

                var response = await client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.VerifyChallenge,
                    RequestMessage = new VerifyChallengeMessage
                    {
                        Token = captchaResponse
                    }.ToByteString()
                }, false);

                var verifyChallengeResponse = VerifyChallengeResponse.Parser.ParseFrom(response);

                if (!verifyChallengeResponse.Success)
                {
                    client.ClientManager.LogCaller(new LoggerEventArgs($"(CAPTCHA) Failed to resolve captcha, try resolved captcha by official app. ", LoggerTypes.Warning));
                    return false;
                }
                client.ClientManager.LogCaller(new LoggerEventArgs($"(CAPTCHA) Great!!! Captcha has been by passed",LoggerTypes.Success));
                return verifyChallengeResponse.Success;
            }
            catch (Exception ex)
            {
                client.ClientManager.LogCaller(new LoggerEventArgs("(CAPTCHA) error.", LoggerTypes.Exception, ex));
            }
            return false;
        }

        private static async Task<string> GetCaptchaResposeByAntiCaptcha(Client client, string captchaUrl)
        {
            bool solved = false;
            string result = null;

            {
                result = await AntiCaptchaClient.SolveCaptcha(captchaUrl,
                    client.ClientManager.UserSettings.AntiCaptchaAPIKey,
                    POKEMON_GO_GOOGLE_KEY,
                    client.ClientManager.UserSettings.ProxyHostCaptcha.ToString(),
                    client.ClientManager.UserSettings.ProxyPortCaptcha);
                solved = !string.IsNullOrEmpty(result);
            }
            if (solved)
            {
                client.ClientManager.LogCaller(new LoggerEventArgs("Captcha has been resolved automatically by Anti-Captcha ", LoggerTypes.Success));
            }
            return result;
        }


        private static async Task<string> GetCaptchaResposeBy2Captcha(Client client, string captchaUrl)
        {
            bool solved = false;
            int retries = client.ClientManager.UserSettings.AutoCaptchaRetries;
            string result = null;

            while (retries-- > 0 && !solved)
            {
                TwoCaptchaClient _client = new TwoCaptchaClient(client.ClientManager.UserSettings.TwoCaptchaAPIKey);

                result = await _client.SolveRecaptchaV2(POKEMON_GO_GOOGLE_KEY, captchaUrl, string.Empty, ProxyType.HTTP, client);
                solved = !string.IsNullOrEmpty(result);
            }
            if (solved)
            {
                client.ClientManager.LogCaller(new LoggerEventArgs("Captcha has been resolved automatically by 2Captcha ", LoggerTypes.Success));
            }
            return result;
        }

        public static string GetCaptchaResposeManually(Client client, string url)
        {
            if (!client.ClientManager.UserSettings.AllowManualCaptchaResolve) return null;

            if (!File.Exists("chromedriver.exe"))
            {
                client.ClientManager.LogCaller(new LoggerEventArgs(
                     $"You enable manual captcha resolve but bot didn't setup files properly, please download chromedriver.exe put in same folder.",
                    LoggerTypes.FatalError));
                return null;
            }
            IWebDriver webDriver = null;
            try
            {
                webDriver = new ChromeDriver(Environment.CurrentDirectory, new ChromeOptions()
                {
                });

                webDriver.Navigate().GoToUrl(url);
                client.ClientManager.LogCaller(new LoggerEventArgs($"Captcha is being show in separate thread window, please check your chrome browser and resolve it before {client.ClientManager.UserSettings.ManualCaptchaTimeout} seconds", LoggerTypes.Info));

                var wait = new WebDriverWait(
                    webDriver,
                    TimeSpan.FromSeconds(client.ClientManager.UserSettings.ManualCaptchaTimeout)
                );
                //wait.Until(ExpectedConditions.ElementIsVisible(By.Id("recaptcha-verify-button")));
                wait.Until(d =>
                {
                    var ele = d.FindElement(By.Id("g-recaptcha-response"));

                    if (ele == null) return false;
                    return ele.GetAttribute("value").Length > 0;
                });

                string token = wait.Until<string>(driver =>
                {
                    var ele = driver.FindElement(By.Id("g-recaptcha-response"));
                    string t = ele.GetAttribute("value");
                    return t;
                });

                return token;
            }
            catch (Exception ex)
            {
                client.ClientManager.LogCaller(new LoggerEventArgs($"You didn't resolve captcha in the given time", LoggerTypes.Exception, ex));
            }
            finally
            {
                if (webDriver != null) webDriver.Close();
            }

            return null;
        }
    }
}