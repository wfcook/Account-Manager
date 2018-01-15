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

        private async Task<MethodResult<List<MapPokemon>>> GetIncensePokemons()
        {
            var appliedItems = GetAppliedItems().ToDictionary(item => item.ItemId, item => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(item.ExpireMs));
            DateTime expires = new DateTime(0);
            ItemId itemUsed = ItemId.ItemUnknown;

            foreach (var item in Items)
            {
                if (appliedItems.ContainsKey(item.ItemId))
                {
                    expires = appliedItems[item.ItemId];
                    itemUsed = item.ItemId;
                }
            }

            var time = expires - DateTime.UtcNow;
            if (expires.Ticks == 0 || time.TotalSeconds < 0)
            {
                itemUsed = ItemId.ItemUnknown;
                if (UserSettings.UseIncense)
                {
                    // use basic incense only ...
                    var incenses = Items.Where(x => x.ItemId == ItemId.ItemIncenseOrdinary
                    //|| x.ItemId == ItemId.ItemIncenseCool
                    //|| x.ItemId == ItemId.ItemIncenseFloral
                    //|| x.ItemId == ItemId.ItemIncenseSpicy
                    );

                    if (incenses.Count() > 0)
                    {
                        await UseIncense(incenses.FirstOrDefault().ItemId);
                    }
                    else
                        return new MethodResult<List<MapPokemon>>();
                }
                else
                    return new MethodResult<List<MapPokemon>>();
            }
            else
            {
                if (itemUsed == ItemId.ItemUnknown)
                    return new MethodResult<List<MapPokemon>>();

                LogCaller(new LoggerEventArgs(String.Format("Incense {0} actived {1}m {2}s.", itemUsed, time.Minutes, Math.Abs(time.Seconds)), LoggerTypes.Info));

                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.GetIncensePokemon,
                    RequestMessage = new GetIncensePokemonMessage
                    {
                        PlayerLatitude = _client.ClientSession.Player.Latitude,
                        PlayerLongitude = _client.ClientSession.Player.Longitude
                    }.ToByteString()
                });

                GetIncensePokemonResponse getIncensePokemonResponse = GetIncensePokemonResponse.Parser.ParseFrom(response);

                switch (getIncensePokemonResponse.Result)
                {
                    case GetIncensePokemonResponse.Types.Result.IncenseEncounterAvailable:
                        var pokemon = new MapPokemon
                        {
                            EncounterId = getIncensePokemonResponse.EncounterId,
                            ExpirationTimestampMs = getIncensePokemonResponse.DisappearTimestampMs,
                            Latitude = getIncensePokemonResponse.Latitude,
                            Longitude = getIncensePokemonResponse.Longitude,
                            PokemonId = getIncensePokemonResponse.PokemonId,
                            SpawnPointId = getIncensePokemonResponse.EncounterLocation,
                            PokemonDisplay = getIncensePokemonResponse.PokemonDisplay
                        };
                        return new MethodResult<List<MapPokemon>>
                        {
                            Data = new List<MapPokemon> { pokemon },
                            Success = true,
                            Message = "Succes"                            
                        };
                    case GetIncensePokemonResponse.Types.Result.IncenseEncounterNotAvailable:
                        return new MethodResult<List<MapPokemon>>();
                    case GetIncensePokemonResponse.Types.Result.IncenseEncounterUnknown:
                        return new MethodResult<List<MapPokemon>>();
                }
            }
            return new MethodResult<List<MapPokemon>>();
        }
    }
}
