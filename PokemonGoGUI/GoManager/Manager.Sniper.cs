using POGOProtos.Enums;
using PokemonGo.RocketAPI;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager.Models;
using PokemonGoGUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        private int _lastPokeSniperId = 0;

        private MethodResult<PokeSniperObject> RequestPokeSniperRares()
        {
            try
            {
                using(WebClient wc = new WebClient())
                {
                    string response = wc.DownloadString("http://pokesnipers.com/api/v1/pokemon.json");

                    PokeSniperObject pkObject = Serializer.FromJson<PokeSniperObject>(response);

                    return new MethodResult<PokeSniperObject>
                    {
                        Data = pkObject,
                        Success = true
                    };
                }
            }
            catch(Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to request PokeSniper website", LoggerTypes.Warning, ex));

                return new MethodResult<PokeSniperObject>();
            }
        }

        public async Task<MethodResult> GetPokemonToSnipe()
        {
            return null;
        }

        private async Task<MethodResult> CaptureSnipePokemon(double latitude, double longitude, PokemonId pokemon)
        {
            LogCaller(new LoggerEventArgs(String.Format("Sniping {0} at location {1}, {2}", pokemon, latitude, longitude), LoggerTypes.Info));

            return null;
        }
    }
}
