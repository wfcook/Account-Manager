using GeoCoordinatePortable;
using Google.Common.Geometry;
using Google.Protobuf;
using Google.Protobuf.Collections;
using POGOLib.Official.Pokemon;
using POGOProtos.Map;
using POGOProtos.Map.Fort;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using PokemonGoGUI.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        private async Task<MethodResult<List<MapPokemon>>> GetCatchablePokemon()
        {
            MethodResult<RepeatedField<MapCell>> mapCellResponse = await GetMapObjects();

            if (!mapCellResponse.Success)
            {
                return new MethodResult<List<MapPokemon>>
                {
                    Message = mapCellResponse.Message,
                    Data = new List<MapPokemon>(),
                    Success = mapCellResponse.Success
                };
            }

            return new MethodResult<List<MapPokemon>>
            {
                Data = mapCellResponse.Data.
                                        SelectMany(x => x.CatchablePokemons).
                                        Where(x => PokemonWithinCatchSettings(x)).
                                        ToList(),
                Success = mapCellResponse.Success,
                Message = mapCellResponse.Message
            };
        }

        private async Task<MethodResult<List<FortData>>> GetPokeStops()
        {
            MethodResult<List<FortData>> allFortsResponse = await GetAllForts();

            if (!allFortsResponse.Success)
            {
                return allFortsResponse;
            }

            List<FortData> fortData = new List<FortData>();

            if (allFortsResponse.Data.Count == 0)
            {
                return new MethodResult<List<FortData>>
                {
                    Data = fortData,
                    Message = "No pokestop data found. Potential temp IP ban or bad location",
                    Success = true
                };
            }

            foreach (FortData fort in allFortsResponse.Data)
            {
                if (fort.CooldownCompleteTimestampMs > DateTime.UtcNow.ToUnixTime())
                {
                    continue;
                }

                GeoCoordinate defaultLocation = new GeoCoordinate(_client.ClientSession.Player.Latitude, _client.ClientSession.Player.Longitude);
                GeoCoordinate fortLocation = new GeoCoordinate(fort.Latitude, fort.Longitude);

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

            fortData = fortData.OrderBy(x => CalculateDistanceInMeters(_client.ClientSession.Player.Latitude, _client.ClientSession.Player.Longitude, x.Latitude, x.Longitude)).ToList();

            return new MethodResult<List<FortData>>
            {
                Data = fortData,
                Message = "Success",
                Success = true
            };
        }

        private async Task<MethodResult<List<FortData>>> GetGyms()
        {
            MethodResult<List<FortData>> allFortsResponse = await GetAllForts();

            if (!allFortsResponse.Success)
            {
                return allFortsResponse;
            }

            List<FortData> fortData = new List<FortData>();

            foreach (FortData fort in allFortsResponse.Data)
            {
                if (fort.Type != FortType.Gym)
                {
                    continue;
                }

                GeoCoordinate defaultLocation = new GeoCoordinate(_client.ClientSession.Player.Latitude, _client.ClientSession.Player.Longitude);
                GeoCoordinate fortLocation = new GeoCoordinate(fort.Latitude, fort.Longitude);

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
                Message = allFortsResponse.Message,
                Success = allFortsResponse.Success
            };
        }

        private async Task<MethodResult<List<FortData>>> GetAllForts()
        {
            MethodResult<RepeatedField<MapCell>> mapCellResponse = await GetMapObjects();

            if (!mapCellResponse.Success)
            {
                return new MethodResult<List<FortData>>
                {
                    Data = new List<FortData>(),
                    Message = mapCellResponse.Message,
                    Success = mapCellResponse.Success
                };
            }

            return new MethodResult<List<FortData>>
            {
                Data = mapCellResponse.Data.SelectMany(x => x.Forts).ToList(),
                Success = mapCellResponse.Success,
                Message = mapCellResponse.Message
            };
        }

        private async Task<MethodResult<RepeatedField<MapCell>>> GetMapObjects()
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await Login();

                if (!result.Success)
                {
                    return new MethodResult<RepeatedField<MapCell>> { Success = false, Message = "Failed to get map objets.", Data = new RepeatedField<MapCell>() };
                }
            }

            if (_client.ClientSession.Map.Cells.Count < 0)
            {
                return new MethodResult<RepeatedField<MapCell>> { Success = false, Message = "Failed to get map objets.", Data = new RepeatedField<MapCell>() };
            }

            return new MethodResult<RepeatedField<MapCell>>
            {
                Data = _client.ClientSession.Map.Cells,
                Success = true,
                Message = "Success"
            };
        }
    }
}
