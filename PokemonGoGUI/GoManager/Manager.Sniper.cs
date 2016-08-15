using POGOProtos.Enums;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;
using PokemonGo.RocketAPI;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager.Models;
using PokemonGoGUI.Models;
using System;
using System.Collections.Generic;
using System.Device.Location;
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

        public async Task<MethodResult> SnipeAllPokemon()
        {
            MethodResult<PokeSniperObject> pokeSniperResult = RequestPokeSniperRares();

            if(!pokeSniperResult.Success)
            {
                return new MethodResult
                {
                    Message = pokeSniperResult.Message
                };
            }

            if(pokeSniperResult.Data.results.Count == 0)
            {
                LogCaller(new LoggerEventArgs("PokeSniper return no pokemon to snipe", LoggerTypes.Info));

                return new MethodResult
                {
                    Message = "No pokemon found"
                };
            }

            List<PokeSniperResult> pokemonToSnipe = pokeSniperResult.Data.results.Where(x => x.id > _lastPokeSniperId && PokemonWithinCatchSettings(x.PokemonId, true) && x.DespawnTime >= DateTime.Now.AddSeconds(30)).TakeLast(UserSettings.MaxPokemonPerSnipe).ToList();

            _lastPokeSniperId = pokeSniperResult.Data.results.OrderByDescending(x => x.id).First().id;

            if(pokemonToSnipe.Count == 0) 
            {
                LogCaller(new LoggerEventArgs("No pokemon to snipe within catch settings", LoggerTypes.Info));

                return new MethodResult
                {
                    Message = "No catchable pokemon found"
                };
            }

            LogCaller(new LoggerEventArgs(String.Format("Sniping {0} pokemon", pokemonToSnipe.Count), LoggerTypes.Info));

            await Task.Delay(CalculateDelay(UserSettings.DelayBetweenSnipes, UserSettings.BetweenSnipesDelayRandom));

            bool hasPokeballs = HasPokeballsLeft();
            int failedAttempts = 0;
            int maxFailedAttempted = 3;

            //Long running, so can't let this continue
            while(pokemonToSnipe.Any() && hasPokeballs && failedAttempts < maxFailedAttempted && IsRunning)
            {
                PokeSniperResult pokemon = pokemonToSnipe.First();
                pokemonToSnipe.Remove(pokemon);

                if(pokemon.Latitude < -90 || pokemon.Latitude > 90 || pokemon.Longitude < -180 || pokemon.Longitude > 180)
                {
                    LogCaller(new LoggerEventArgs(String.Format("Invalid location {0}, {1} given for {2}. Skipping", pokemon.Latitude, pokemon.Longitude, pokemon.name), LoggerTypes.Info));

                    continue;
                }


                MethodResult<bool> captureResult = await CaptureSnipePokemon(pokemon.Latitude, pokemon.Longitude, pokemon.PokemonId);

                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenSnipes, UserSettings.BetweenSnipesDelayRandom));
                
                if(!captureResult.Success)
                {
                    ++failedAttempts;
                }

                pokemonToSnipe = pokemonToSnipe.Where(x => PokemonWithinCatchSettings(x.PokemonId) && x.DespawnTime >= DateTime.Now.AddSeconds(30)).ToList();

                if(_fleeingPokemonResponses >= _fleeingPokemonUntilBan)
                {
                    LogCaller(new LoggerEventArgs("Too many fleeing pokemon. Will stop sniping ...", LoggerTypes.Warning));
                    break;
                }

                hasPokeballs = HasPokeballsLeft();

                if(!hasPokeballs)
                {
                    LogCaller(new LoggerEventArgs("No pokeballs remaining. Done sniping", LoggerTypes.Warning));
                }
            }

            return new MethodResult
            {
                Success = true
            };
        }

        private async Task<MethodResult<bool>> CaptureSnipePokemon(double latitude, double longitude, PokemonId pokemon)
        {
            LogCaller(new LoggerEventArgs(String.Format("Sniping {0} at location {1}, {2}", pokemon, latitude, longitude), LoggerTypes.Info));

            GeoCoordinate originalLocation = new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude, _client.CurrentAltitude);

            //Update location
            MethodResult result = await UpdateLocation(new GeoCoordinate(latitude, longitude, _client.CurrentAltitude));

            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

            if(!result.Success)
            {
                //Just attempt it to prevent anything bad.
                await UpdateLocation(originalLocation);

                return new MethodResult<bool>();
            }

            //Get catchable pokemon
            MethodResult<List<MapPokemon>> pokemonResult = await GetCatchablePokemon();

            if(!pokemonResult.Success)
            {
                await UpdateLocation(originalLocation);

                return new MethodResult<bool>
                {
                    Message = pokemonResult.Message
                };
            }

            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

            MapPokemon pokemonToSnipe = pokemonResult.Data.FirstOrDefault(x => x.PokemonId == pokemon);

            if(pokemonToSnipe == null)
            {
                LogCaller(new LoggerEventArgs(String.Format("Pokemon {0} not found. Possible despawn. Going back to original location", pokemon), LoggerTypes.Warning));

                await UpdateLocation(originalLocation);

                return new MethodResult<bool>
                {
                    Message = "Pokemon not found"
                };
            }

            //Encounter
            MethodResult<EncounterResponse> eResponseResult = await EncounterPokemon(pokemonToSnipe);

            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

            if(!eResponseResult.Success)
            {
                LogCaller(new LoggerEventArgs("Failed to encounter pokemon. Going back to original location", LoggerTypes.Warning));

                //Failed, update location back
                await UpdateLocation(originalLocation);

                return new MethodResult<bool>
                {
                    Message = eResponseResult.Message
                };
            }

            //Update location back
            MethodResult locationResult = await RepeatAction(() => UpdateLocation(originalLocation), 2);

            if(!locationResult.Success)
            {
                return new MethodResult<bool>();
            }

            //Catch pokemon
            MethodResult catchResult = await CatchPokemon(eResponseResult.Data, pokemonToSnipe); //Handles logging


            if(catchResult.Success)
            {
                return new MethodResult<bool>
                {
                    Success = true
                };
            }

            return new MethodResult<bool>();
        }
    }
}
