using POGOProtos.Enums;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager.Models;
using PokemonGoGUI.Models;
using PokemonGoGUI.Snipers;
using System;
using System.Collections.Generic;
using GeoCoordinatePortable;
using System.Linq;
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
                return PokeSniper.RequestPokemon();
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to request PokeSniper website", LoggerTypes.Warning, ex));

                return new MethodResult<PokeSniperObject>();
            }
        }

        public async Task<MethodResult> SnipeAllPokemon()
        {
            if(!UserSettings.CatchSettings.Any(x => x.Snipe))
            {
                LogCaller(new LoggerEventArgs("No pokemon set to snipe.", LoggerTypes.Info));

                return new MethodResult
                {
                    Message = "No pokemon set to snipe"
                };
            }

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

            if(pokemonToSnipe.Count == 0) 
            {
                LogCaller(new LoggerEventArgs("No pokemon to snipe within catch settings", LoggerTypes.Info));

                return new MethodResult
                {
                    Message = "No catchable pokemon found"
                };
            }

            _lastPokeSniperId = pokemonToSnipe.OrderByDescending(x => x.id).First().id;

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

        public async Task<MethodResult> ManualSnipe(double latitude, double longitude, PokemonId pokemon)
        {
            if (State != Enums.BotState.Stopped)
            {
                Pause();
            }
            else
            {
                MethodResult result = await UpdateDetails();

                if(!result.Success)
                {
                    return result;
                }
            }

            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

            //Wait for actual pause ...
            //Using pausing to prevent infinite loop. Possible to manual exit this by stopping/starting
            while(State == Enums.BotState.Pausing || State == Enums.BotState.Stopping)
            {
                await Task.Delay(100);
            }

            //Make sure it's a paused state or stopped. 
            if(State != Enums.BotState.Paused && State != Enums.BotState.Stopped)
            {
                LogCaller(new LoggerEventArgs("Manual intervention on pausing. Aborting snipe", LoggerTypes.Info));

                return new MethodResult();
            }

            //All good
            await CaptureSnipePokemon(latitude, longitude, pokemon);

            if (State == Enums.BotState.Paused)
            {
                UnPause();
            }

            return new MethodResult
            {
                Success = true
            };
        }

        private async Task<MethodResult<bool>> CaptureSnipePokemon(double latitude, double longitude, PokemonId pokemon)
        {
            LogCaller(new LoggerEventArgs(String.Format("Sniping {0} at location {1}, {2}", pokemon, latitude, longitude), LoggerTypes.Info));

            GeoCoordinate originalLocation = new GeoCoordinate(_client.ClientSession.Player.Latitude, _client.ClientSession.Player.Longitude);

            //Update location
            MethodResult result = await UpdateLocation(new GeoCoordinate(latitude, longitude));

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
            MethodResult locationResult = await UpdateLocation(originalLocation);

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
