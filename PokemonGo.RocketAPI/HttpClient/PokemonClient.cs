#region using directives

using System.Net;
using System.Net.Http;
using PokemonGo.RocketAPI.Helpers;
using System;

#endregion

namespace PokemonGo.RocketAPI.HttpClient
{
    public class PokemonHttpClient : System.Net.Http.HttpClient
    {
        public PokemonHttpClient(HttpClientHandler handler) : base(handler)
        {
            DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Niantic App");
            DefaultRequestHeaders.ExpectContinue = false;
            DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
            DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            //DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
            DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/binary");
            DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "identity, gzip");
            this.Timeout = TimeSpan.FromSeconds(10);
        }
    }
}