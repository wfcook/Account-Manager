using PokemonGoGUI.Extensions;
using PokemonGoGUI.Models;
using System;
using System.Net;

namespace PokemonGoGUI.Snipers
{
    public static class PokeSniper
    {
        private static DateTime _lastRequest = new DateTime();
        private static object _locker = new object();
        private static int _requestSpeed = 1;
        private static PokeSniperObject _lastRequestResponse = null;

        public static MethodResult<PokeSniperObject> RequestPokemon()
        {
            lock (_locker)
            {
                DateTime nextRequest = DateTime.Now.AddMinutes(_requestSpeed * -1);

                if (nextRequest > _lastRequest)
                {
                    _lastRequest = DateTime.Now;

                    using (WebClient wc = new WebClient())
                    {
                        string response = wc.DownloadString("http://pokesnipers.com/api/v1/pokemon.json");

                        PokeSniperObject pkObject = Serializer.FromJson<PokeSniperObject>(response);

                        _lastRequestResponse = pkObject;

                        return new MethodResult<PokeSniperObject>
                        {
                            Data = pkObject,
                            Success = true
                        };
                    }
                }
                else
                {
                    return new MethodResult<PokeSniperObject>
                    {
                        Data = _lastRequestResponse,
                        Success = true
                    };
                }
            }
        }
    }
}
