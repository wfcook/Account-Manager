using Newtonsoft.Json;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI.Hash
{
    public class PokefarmerHasher : IHasher
    {
        public static string PokeHashURL = "https://pokehash.buddyauth.com/";
        public static string PokeHashURL2 = "http://pokehash.buddyauth.com/";
        
        private string apiKey;
        private string apiEndPoint;

        public PokefarmerHasher(ISettings settings, string apiKey, string apiEndPoint)
        {
            this.apiKey = apiKey;
            this.apiEndPoint = apiEndPoint;
            if (settings.UseCustomAPI)
            {
                PokeHashURL = settings.UrlHashServices;
                PokeHashURL2 = settings.UrlHashServices;
                if (!string.IsNullOrEmpty(settings.EndPoint))
                    this.apiEndPoint = settings.EndPoint;
            }
        }

        public async Task<HashResponseContent> RequestHashesAsync(HashRequestContent request)
        {
            int retry = 3;
            do
            {
                try
                {
                    return await InternalRequestHashesAsync(request).ConfigureAwait(false);
                }
                catch (HasherException hashEx)
                {
                    throw hashEx;
                }
                catch (TimeoutException)
                {
                    throw new HasherException("Pokefamer Hasher server might down - timeout out");
                }
                catch (Exception ex)
                {
                    throw new HasherException(ex.Message);
                }
                finally
                {
                    retry--;
                }
            } while (retry > 0);

            throw new HasherException("Pokefamer Hash API server might be down");

        }
        private DateTime lastPrintVerbose = DateTime.Now;

        private async Task<HashResponseContent> InternalRequestHashesAsync(HashRequestContent request)
        {
            var maskedKey = apiKey.Substring(0, 4) + "".PadLeft(apiKey.Length - 8, 'X') + apiKey.Substring(apiKey.Length - 4, 4);
            // NOTE: This is really bad. Don't create new HttpClient's all the time.
            // Use a single client per-thread if you need one.
            using (var client = new System.Net.Http.HttpClient())
            {
                // The URL to the hashing server.
                // Do not include "api/v1/hash" unless you know why you're doing it, and want to modify this code.
                client.BaseAddress = new Uri(PokeHashURL);

                // By default, all requests (and this example) are in JSON.
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                // Set the X-AuthToken to the key you purchased from Bossland GmbH
                client.DefaultRequestHeaders.Add("X-AuthToken", apiKey);

                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.ASCII, "application/json");
                // An odd bug with HttpClient. You need to set the content type again.
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                Stopwatch watcher = new Stopwatch();
                HttpResponseMessage response = null;
                watcher.Start();

                try
                {
                    response = await client.PostAsync(apiEndPoint, content).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    try
                    {
                        client.BaseAddress = new Uri(PokeHashURL2);
                        response = await client.PostAsync(apiEndPoint, content).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
 
                // TODO: Fix this up with proper retry-after when we get rate limited.
                switch (response.StatusCode)
                {
                    // All good. Return the hashes back to the caller. :D
                    case HttpStatusCode.OK:
                        return JsonConvert.DeserializeObject<HashResponseContent>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

                    // Returned when something in your request is "invalid". Also when X-AuthToken is not set.
                    // See the error message for why it is bad.
                    case HttpStatusCode.BadRequest:
                        string responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        if (responseText.Contains("Unauthorized"))
                        {
                            throw new HasherException($"Your API Key: {maskedKey} is incorrect or expired, please check auth.json (Pokefamer Message : {responseText})");
                        }
                        Console.WriteLine($"Bad request sent to the hashing server! {responseText}");

                        break;

                    // This error code is returned when your "key" is not in a valid state. (Expired, invalid, etc)
                    case HttpStatusCode.Unauthorized:
                        throw new HasherException($"You are not authorized to use this service.  Please check that your API key {maskedKey} is correct.");

                    // This error code is returned when you have exhausted your current "hashes per second" value
                    // You should queue up your requests, and retry in a second.
                    case (HttpStatusCode)429:
                        long ratePeriodEndsAtTimestamp;
                        IEnumerable<string> ratePeriodEndHeaderValues;
                        if (response.Headers.TryGetValues("X-RatePeriodEnd", out ratePeriodEndHeaderValues))
                        {
                            // Get the rate-limit period ends at timestamp in seconds.
                            ratePeriodEndsAtTimestamp = Convert.ToInt64(ratePeriodEndHeaderValues.First());
                        }
                        else
                        {
                            // If for some reason we couldn't get the timestamp, just default to 2 second wait.
                            ratePeriodEndsAtTimestamp = Utils.GetTime(false) + 2;
                        }

                        long timeToWaitInSeconds = ratePeriodEndsAtTimestamp - Utils.GetTime(false);

                        if (timeToWaitInSeconds > 0)
                            await Task.Delay((int)(timeToWaitInSeconds * 1000)).ConfigureAwait(false);  // Wait until next rate-limit period begins.

                        return await RequestHashesAsync(request).ConfigureAwait(false);
                    default:
                        throw new HasherException($"Hash API server ({client.BaseAddress}{apiEndPoint}) might down!");
                }

            }

            return null;
        }
    }
}
