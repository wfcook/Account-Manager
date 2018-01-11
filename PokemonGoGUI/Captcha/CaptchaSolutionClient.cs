using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PokemonGoGUI.GoManager;
using PokemonGoGUI.GoManager.Models;

namespace PokemonGoGUI.Captcha
{
    public class CaptchaSolutionClient
    {
        private const string API_ENDPOINT = "http://api.captchasolutions.com/solve?";

        public class APIObjectResponse
        {
            public string Captchasolutions { get; set; }
        }

        public string APIKey { get; set; }
        public string APISecret { get; set; }

        public int Timeout { get; set; }

        public CaptchaSolutionClient(string key, string secret, int timeout = 120)
        {
            APIKey = key;
            APISecret = secret;
            Timeout = timeout;
        }

        public async Task<string> ResolveCaptcha(string googleSiteKey, string captchaUrl, Manager manager)
        {
            if (string.IsNullOrEmpty(APIKey) || string.IsNullOrEmpty(APISecret))
            {
                manager.LogCaller(new LoggerEventArgs(
                    $"(CAPTCHA) - CaptchaSolutions API key or API Secret  not setup properly.",
                    LoggerTypes.FatalError));

                return string.Empty;
            }

            string contentstring = $"p=nocaptcha&googlekey={googleSiteKey}&pageurl={captchaUrl}&key={APIKey}&secret={APISecret}&out=json";

            //HttpContent content = new StringContent(contentstring);
            //content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
            //string url = "http://api.captchasolutions.com/solve?p=nocaptcha&googlekey=6Le-wvkSAAAAAPBMRTvw0Q4Muexq9bi0DJwx_mJ-&pageurl=https://www.google.com/recaptcha/api2/demo&key=dcbeece13cb658697f7e39264603fc70&secret=fb14ac29&out=json";
            var url = $"{API_ENDPOINT}{contentstring}";
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(Timeout);

                try
                {
                    var responseContent = await client.GetAsync(url).ConfigureAwait(false);
                    if (responseContent.StatusCode != HttpStatusCode.OK)
                    {
                        manager.LogCaller(new LoggerEventArgs(
                             $"(CAPTCHA) - Could not connect to solution captcha, please check your API config",
                           LoggerTypes.FatalError)
                        );
                        return string.Empty;
                    }
                    var responseJSON = await responseContent.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var response = JsonConvert.DeserializeObject<APIObjectResponse>(responseJSON);
                    return response.Captchasolutions;
                }
                catch (Exception)
                {
                    manager.LogCaller(new LoggerEventArgs($"(CAPTCHA) - An Error has occurred when solving captcha with Captcha Solutions", LoggerTypes.Exception));
                }
            }
            return string.Empty;
        }
    }
}