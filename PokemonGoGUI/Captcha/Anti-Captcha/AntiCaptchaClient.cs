using PokemonGoGUI.GoManager;
using PokemonGoGUI.GoManager.Models;
using System.Threading.Tasks;

namespace PokemonGoGUI.Captcha.Anti_Captcha
{
    public class AntiCaptchaClient
    {
       
        private const string Host = "api.anti-captcha.com";
        //private  string ClientKey = "xxxxxxx";
        private const string ProxyHost = "xx.xx.xx.xx";
        private const int ProxyPort = 8282;
        private const string ProxyLogin = "";
        private const string ProxyPassword = "";

        private const string UserAgent =
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36"
            ;


        public static async Task<string> SolveCaptcha(string captchaURL, string apiKey, string googleSiteKey,
            string proxyHost, int proxyPort, string proxyAccount = "", string proxyPassword = "", Manager manager = null)
        {
            var task1 = AnticaptchaApiWrapper.CreateNoCaptchaTaskProxyless(
                Host,
                apiKey,
                captchaURL, //target website address
                googleSiteKey,
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36"
            );

            return await ProcessTask(task1, apiKey, manager).ConfigureAwait(false);
        }

        private static async Task<string> ProcessTask(AnticaptchaTask task, string apikey, Manager manager)
        {
            AnticaptchaResult response;

            do
            {
                response = AnticaptchaApiWrapper.GetTaskResult(Host, apikey, task);

                if (response.GetStatus().Equals(AnticaptchaResult.Status.ready))
                {
                    break;
                }

                await Task.Delay(3000).ConfigureAwait(false);
            } while (response != null && response.GetStatus().Equals(AnticaptchaResult.Status.processing));

            if (response == null || response.GetSolution() == null)
            {
                manager.LogCaller(new LoggerEventArgs("Unknown error occurred...", LoggerTypes.FatalError));
                //Console.WriteLine("Response dump:");
                //Console.WriteLine(response);
            }
            else
            {
                //Console.WriteLine("The answer is '" + response.GetSolution() + "'");
            }

            return response.GetSolution();
        }
    }
}