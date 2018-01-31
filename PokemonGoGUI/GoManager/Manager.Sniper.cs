using POGOLib.Official.Extensions;
using POGOProtos.Enums;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;
using PokemonGoGUI.Enums;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        private ulong _lastPokeSniperId = 0;

        private MethodResult<List<NearbyPokemon>> RequestPokeSniperRares()
        {
            if (_client.ClientSession.Map.Cells.Count == 0 || _client.ClientSession.Map == null)
            {
                return new MethodResult<List<NearbyPokemon>>();
            }

            var cells = _client.ClientSession.Map.Cells;
            List<NearbyPokemon> newNearbyPokemons = cells.SelectMany(x => x.NearbyPokemons).ToList();

            if (newNearbyPokemons.Count == 0)
            {
                LogCaller(new LoggerEventArgs("Local sniper return no pokemon to snipe", LoggerTypes.Snipe));

                return new MethodResult<List<NearbyPokemon>>
                {
                    Message = "No pokemon found"
                };
            }

            return new MethodResult<List<NearbyPokemon>>
            {
                Data = newNearbyPokemons,
                Success = true
            };
        }
        
        public async Task<MethodResult> SnipeAllNearyPokemon()
        {
            MethodResult<List<NearbyPokemon>> pokeSniperResult = RequestPokeSniperRares();

            if(!pokeSniperResult.Success)
            {
                return new MethodResult
                {
                    Message = pokeSniperResult.Message
                };
            }

            List<NearbyPokemon> pokemonToSnipe = pokeSniperResult.Data.Where(x => x.EncounterId != _lastPokeSniperId && UserSettings.CatchSettings.FirstOrDefault(p => p.Id == x.PokemonId).Snipe && x.DistanceInMeters < UserSettings.MaxTravelDistance).ToList();
            //_lastPokeSniperId = pokeSniperResult.Data.OrderByDescending(x => x.EncounterId).First().EncounterId;

            if(pokemonToSnipe.Count == 0) 
            {
                LogCaller(new LoggerEventArgs("No pokemon to snipe within catch settings", LoggerTypes.Info));

                return new MethodResult
                {
                    Message = "No catchable pokemon found"
                };
            }

            LogCaller(new LoggerEventArgs(String.Format("Sniping {0} pokemon", pokemonToSnipe.Count), LoggerTypes.Snipe));

            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

            //Long running, so can't let this continue
            while (pokemonToSnipe.Any() && IsRunning)
            {
                NearbyPokemon nearbyPokemon = pokemonToSnipe.First();
                pokemonToSnipe.Remove(nearbyPokemon);

                var forts = _client.ClientSession.Map.Cells.SelectMany(x => x.Forts);
                var fortNearby = forts.Where(x => x.Id == nearbyPokemon.FortId).FirstOrDefault();

                if (fortNearby == null)
                { 
                    continue;
                }

                GeoCoordinate coords = new GeoCoordinate
                {
                    Latitude = fortNearby.Latitude,
                    Longitude = fortNearby.Longitude
                };

                var captureSnipe = await CaptureSnipePokemon(coords.Latitude, coords.Longitude, nearbyPokemon.PokemonId);

                if (captureSnipe.Success)
                    _lastPokeSniperId = nearbyPokemon.EncounterId;

                await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                pokemonToSnipe = pokemonToSnipe.Where(x => UserSettings.CatchSettings.FirstOrDefault(p => p.Id == x.PokemonId).Snipe && fortNearby.CooldownCompleteTimestampMs >= DateTime.Now.AddSeconds(30).ToUnixTime()).ToList();
            }

            return new MethodResult
            {
                Success = true
            };
        }

        private async Task<MethodResult> CaptureSnipePokemon(double latitude, double longitude, PokemonId pokemon)
        {
            LogCaller(new LoggerEventArgs(String.Format("Sniping {0} at location {1}, {2}", pokemon, latitude, longitude), LoggerTypes.Snipe));

            GeoCoordinate originalLocation = new GeoCoordinate(_client.ClientSession.Player.Latitude, _client.ClientSession.Player.Longitude, _client.ClientSession.Player.Altitude);

            //Update location           
            //MethodResult result = await UpdateLocation(new GeoCoordinate(latitude, longitude));
            //Update location           
            MethodResult result = await GoToLocation(new GeoCoordinate(latitude, longitude));           

            if(!result.Success)
            {
                return result;
            }

            await Task.Delay(10000);

            int retries = 3;

            //Get catchable pokemon
            retry:

            MethodResult<List<MapPokemon>> pokemonResult = GetCatchablePokemon();

            if(!pokemonResult.Success)
            {
                return new MethodResult
                {
                    Message = pokemonResult.Message
                };
            }

            MapPokemon pokemonToSnipe = pokemonResult.Data.FirstOrDefault(x => x.PokemonId == pokemon);

            if(pokemonToSnipe == null)
            {
                if (retries > 0)
                {
                    LogCaller(new LoggerEventArgs(String.Format("Snipe Pokemon {0} not found. Retries #{1}", pokemon, retries), LoggerTypes.Info));
                    retries--;
                    await Task.Delay(800);
                    goto retry;
                }

                LogCaller(new LoggerEventArgs(String.Format("Snipe Pokemon {0} not found. Possible despawn. Going back to original location", pokemon), LoggerTypes.Warning));

                //await UpdateLocation(originalLocation);
                await GoToLocation(originalLocation);

                return new MethodResult
                {
                    Message = "Pokemon not found"
                };
            }

            //Encounter
            MethodResult<EncounterResponse> eResponseResult = await EncounterPokemon(pokemonToSnipe);

            if (!eResponseResult.Success)
            {
                LogCaller(new LoggerEventArgs("Snipe failed to encounter pokemon. Going back to original location", LoggerTypes.Warning));

                //Failed, update location back
                //await UpdateLocation(originalLocation);
                await GoToLocation(originalLocation);

                return new MethodResult
                {
                    Message = eResponseResult.Message
                };
            }

            //Update location back
            //MethodResult locationResult = await RepeatAction(() => UpdateLocation(originalLocation), 2);
            MethodResult locationResult = await RepeatAction(() => GoToLocation(originalLocation), 2);

            if (!locationResult.Success)
            {
                return locationResult;
            }

            //Catch pokemon
            MethodResult catchResult = await CatchPokemon(eResponseResult.Data, pokemonToSnipe, true); //Handles logging

            return catchResult;
        }
    }
}
