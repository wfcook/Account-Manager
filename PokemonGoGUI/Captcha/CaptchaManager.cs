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

namespace PokemonGoGUI.Captcha
{
    public class CaptchaManager
    {
        const string POKEMON_GO_GOOGLE_KEY = "6LeeTScTAAAAADqvhqVMhPpr_vB9D364Ia-1dSgK";

        public static async Task<bool> SolveCaptcha(Manager manager, string captchaUrl)
        {
            string captchaResponse = "";
            bool resolved = false;
            bool needGetNewCaptcha = false;
            int retry = manager.UserSettings.AutoCaptchaRetries;

            while (retry-- > 0 && !resolved)
            {
                //Use captcha solution to resolve captcha
                if (manager.UserSettings.EnableCaptchaSolutions)
                {
                    if (needGetNewCaptcha)
                    {
                        captchaUrl = await GetNewCaptchaURL(manager).ConfigureAwait(false);
                    }
                    manager.LogCaller(new LoggerEventArgs("Auto resolving captcha by using captcha solution service, please wait..........", LoggerTypes.Info));
                    CaptchaSolutionClient client = new CaptchaSolutionClient(manager.UserSettings.CaptchaSolutionAPIKey,
                        manager.UserSettings.CaptchaSolutionsSecretKey, manager.UserSettings.AutoCaptchaTimeout);
                    captchaResponse = await client.ResolveCaptcha(POKEMON_GO_GOOGLE_KEY, captchaUrl, manager).ConfigureAwait(false);
                    needGetNewCaptcha = true;
                    if (!string.IsNullOrEmpty(captchaResponse))
                    {
                        resolved = await Resolve(manager, captchaResponse).ConfigureAwait(false);
                    }
                }         

                //use 2 captcha
                if (!resolved && manager.UserSettings.Enable2Captcha &&
                    !string.IsNullOrEmpty(manager.UserSettings.TwoCaptchaAPIKey))
                {
                    if (needGetNewCaptcha)
                    {
                        captchaUrl = await GetNewCaptchaURL(manager).ConfigureAwait(false);
                    }
                    if (string.IsNullOrEmpty(captchaUrl)) return true;

                    manager.LogCaller(new LoggerEventArgs("Auto resolving captcha by using 2Captcha service", LoggerTypes.Info));
                    captchaResponse = await GetCaptchaResposeBy2Captcha(manager, captchaUrl).ConfigureAwait(false);
                    needGetNewCaptcha = true;
                    if (!string.IsNullOrEmpty(captchaResponse))
                    {
                        resolved = await Resolve(manager, captchaResponse).ConfigureAwait(false);
                    }
                }

                if (!resolved && manager.UserSettings.EnableAntiCaptcha && !string.IsNullOrEmpty(manager.UserSettings.AntiCaptchaAPIKey))
                {
                    if (needGetNewCaptcha)
                    {
                        captchaUrl = await GetNewCaptchaURL(manager).ConfigureAwait(false);
                    }
                    if (string.IsNullOrEmpty(captchaUrl)) return true;

                    manager.LogCaller(new LoggerEventArgs("Auto resolving captcha by using anti captcha service", LoggerTypes.Info));
                    captchaResponse = await GetCaptchaResposeByAntiCaptcha(manager, captchaUrl).ConfigureAwait(false);
                    needGetNewCaptcha = true;
                    if (!string.IsNullOrEmpty(captchaResponse))
                    {
                        resolved = await Resolve(manager, captchaResponse).ConfigureAwait(false);
                    }

                }

            }

            //captchaRespose = "";
            if (!resolved)
            {
                if (needGetNewCaptcha)
                {
                    captchaUrl = await GetNewCaptchaURL(manager).ConfigureAwait(false);
                }

                if (manager.UserSettings.PlaySoundOnCaptcha)
                {
                    SystemSounds.Asterisk.Play();
                }

                captchaResponse = GetCaptchaResposeManually(manager, captchaUrl);
                //captchaResponse = await GetCaptchaTokenWithInternalForm(captchaUrl).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(captchaResponse))
                {
                    resolved = await Resolve(manager, captchaResponse).ConfigureAwait(false);
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
                await Task.Delay(1000).ConfigureAwait(false);
            }

            return response;
        }

        private static async Task<string> GetNewCaptchaURL(Manager manager)
        {
            var res = await manager.CheckChallenge();
            if (res.ShowChallenge)
            {
                return res.ChallengeUrl;
            }
            return string.Empty;
        }

        private static async Task<bool> Resolve(Manager manager, string captchaResponse)
        {
            if (string.IsNullOrEmpty(captchaResponse)) return false;
            try
            {
                var verifyChallengeResponse = await manager.VerifyChallenge(captchaResponse);
                if (!verifyChallengeResponse.Success)
                {
                    manager.LogCaller(new LoggerEventArgs($"(CAPTCHA) Failed to resolve captcha, try resolved captcha by official app. ", LoggerTypes.Warning));
                    return false;
                }
                manager.LogCaller(new LoggerEventArgs($"(CAPTCHA) Great!!! Captcha has been by passed",LoggerTypes.Success));
                return verifyChallengeResponse.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        private static async Task<string> GetCaptchaResposeByAntiCaptcha(Manager manager, string captchaUrl)
        {
            bool solved = false;
            string result = null;

            {
                result = await AntiCaptchaClient.SolveCaptcha(captchaUrl,
                    manager.UserSettings.AntiCaptchaAPIKey,
                    POKEMON_GO_GOOGLE_KEY,
                    manager.UserSettings.ProxyHostCaptcha.ToString(),
                    manager.UserSettings.ProxyPortCaptcha).ConfigureAwait(false);
                solved = !string.IsNullOrEmpty(result);
            }
            if (solved)
            {
                manager.LogCaller(new LoggerEventArgs("Captcha has been resolved automatically by Anti-Captcha ", LoggerTypes.Info));
            }
            return result;
        }


        private static async Task<string> GetCaptchaResposeBy2Captcha(Manager manager, string captchaUrl)
        {
            bool solved = false;
            int retries = manager.UserSettings.AutoCaptchaRetries;
            string result = null;

            while (retries-- > 0 && !solved)
            {
                TwoCaptchaClient client = new TwoCaptchaClient(manager.UserSettings.TwoCaptchaAPIKey);

                result = await client.SolveRecaptchaV2(POKEMON_GO_GOOGLE_KEY, captchaUrl, string.Empty, ProxyType.HTTP, manager).ConfigureAwait(false);
                solved = !string.IsNullOrEmpty(result);
            }
            if (solved)
            {
                manager.LogCaller(new LoggerEventArgs("Captcha has been resolved automatically by 2Captcha ", LoggerTypes.Info));
            }
            return result;
        }

        public static string GetCaptchaResposeManually(Manager manager, string url)
        {
            if (!manager.UserSettings.AllowManualCaptchaResolve) return null;

            if (!File.Exists("chromedriver.exe"))
            {
                manager.LogCaller(new LoggerEventArgs(
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
                manager.LogCaller(new LoggerEventArgs($"Captcha is being show in separate thread window, please check your chrome browser and resolve it before {manager.UserSettings.ManualCaptchaTimeout} seconds", LoggerTypes.Info));

                var wait = new WebDriverWait(
                    webDriver,
                    TimeSpan.FromSeconds(manager.UserSettings.ManualCaptchaTimeout)
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
                manager.LogCaller(new LoggerEventArgs($"You didn't resolve captcha in the given time: {ex.Message} ", LoggerTypes.Exception));
            }
            finally
            {
                if (webDriver != null) webDriver.Close();
            }

            return null;
        }
    }
}