using GeoCoordinatePortable;
using Google.Protobuf;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        public MethodResult<List<MapPokemon>> GetCatchablePokemon()
        {

            var cells = _client.ClientSession.Map.Cells;

            //         Where(PokemonWithinCatchSettings) <-- Unneeded, will be filtered after.
            List<MapPokemon> newCatchablePokemons = cells.SelectMany(x => x.CatchablePokemons).ToList();

            return new MethodResult<List<MapPokemon>>
            {
                Data = newCatchablePokemons,
                Success = true,
                Message = "Success"
            };
        }

        public MethodResult<List<FortData>> GetPokeStops()
        {
            var forts = _client.ClientSession.Map.Cells.SelectMany(x => x.Forts);

            var fortData = new List<FortData>();

            if (!forts.Any()) {
                return new MethodResult<List<FortData>> {
                    Data = fortData,
                    Message = "No pokestop data found. Potential temp IP ban or bad location",
                    Success = true
                };
            }

            foreach (FortData fort in forts)
            {
                if (fort.CooldownCompleteTimestampMs >= DateTime.UtcNow.ToUnixTime())
                {
                    continue;
                }

                var defaultLocation = new GeoCoordinate(_client.ClientSession.Player.Latitude, _client.ClientSession.Player.Longitude);
                var fortLocation = new GeoCoordinate(fort.Latitude, fort.Longitude);

                double distance = CalculateDistanceInMeters(defaultLocation, fortLocation);

                if (distance > UserSettings.MaxTravelDistance)
                {
                    continue;
                }

                fortData.Add(fort);
            }

            if (fortData.Count == 0)
            {
                return new MethodResult<List<FortData>>
                {
                    Data = fortData,
                    Message = "No searchable pokestops found within range",
                    Success = true
                };
            }

            if (UserSettings.ShufflePokestops) {
                var rnd = new Random();
                fortData = fortData.OrderBy(x => rnd.Next()).ToList();
            } else {
                fortData = fortData.OrderBy(x => CalculateDistanceInMeters(_client.ClientSession.Player.Latitude, _client.ClientSession.Player.Longitude, x.Latitude, x.Longitude)).ToList();
            }

            return new MethodResult<List<FortData>>
            {
                Data = fortData,
                Message = "Success",
                Success = true
            };
        }

        private MethodResult<List<FortData>> GetGyms()
        {
            var forts = _client.ClientSession.Map.Cells.SelectMany(x => x.Forts).Where(y => y.Type == FortType.Gym);

            var fortData = new List<FortData>();
            foreach (FortData fort in forts)
            {

                var defaultLocation = new GeoCoordinate(_client.ClientSession.Player.Latitude, _client.ClientSession.Player.Longitude);
                var fortLocation = new GeoCoordinate(fort.Latitude, fort.Longitude);

                double distance = CalculateDistanceInMeters(defaultLocation, fortLocation);

                if (distance > UserSettings.MaxTravelDistance)
                {
                    continue;
                }

                fortData.Add(fort);
            }

            return new MethodResult<List<FortData>>
            {
                Data = fortData,
                Message = "Success",
                Success = true
            };
        }

        private async Task<MethodResult<MapPokemon>> GetIncensePokemons()
        {
            if (!_client.ClientSession.IncenseUsed)
            {
                if (UserSettings.UseIncense)
                {
                    var incenses = Items.Where(x => x.ItemId == ItemId.ItemIncenseOrdinary
                    || x.ItemId == ItemId.ItemIncenseCool
                    || x.ItemId == ItemId.ItemIncenseFloral
                    || x.ItemId == ItemId.ItemIncenseSpicy
                    );

                    if (incenses.Count() > 0)
                    {
                        await UseIncense(incenses.FirstOrDefault().ItemId);

                        if (_client.ClientSession.Map.IncensePokemon != null)
                        {
                            return new MethodResult<MapPokemon>
                            {
                                Data = _client.ClientSession.Map.IncensePokemon,
                                Success = true,
                                Message = "Succes"
                            };
                        }
                    }
                }
            }

            if (_client.ClientSession.Map.IncensePokemon != null)
            {
                return new MethodResult<MapPokemon>
                {
                    Data = _client.ClientSession.Map.IncensePokemon,
                    Success = true,
                    Message = "Succes"
                };
            }
            return new MethodResult<MapPokemon>();
        }
    }
}
