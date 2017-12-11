using GeoCoordinatePortable;
using Google.Common.Geometry;
using Google.Protobuf;
using Google.Protobuf.Collections;
using POGOProtos.Map;
using POGOProtos.Map.Fort;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using PokemonGoGUI.Extensions;
using System;
using System.Collections.Generic;
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
                    Message = mapCellResponse.Message
                };
            }

            List<MapPokemon> pokemon = mapCellResponse.Data.
                                        SelectMany(x => x.CatchablePokemons).
                                        Where(x => PokemonWithinCatchSettings(x)).
                                        ToList();

            return new MethodResult<List<MapPokemon>>
            {
                Data = pokemon,
                Success = true,
                Message = "Succes"
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

                GeoCoordinate defaultLocation = new GeoCoordinate(UserSettings.DefaultLatitude, UserSettings.DefaultLongitude);
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

            fortData = fortData.OrderBy(x => CalculateDistanceInMeters(UserSettings.DefaultLatitude, UserSettings.DefaultLongitude, x.Latitude, x.Longitude)).ToList();

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

                GeoCoordinate defaultLocation = new GeoCoordinate(UserSettings.DefaultLatitude, UserSettings.DefaultLongitude);
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
                Message = "Success",
                Success = true
            };
        }

        private async Task<MethodResult<List<FortData>>> GetAllForts()
        {
            MethodResult<RepeatedField<MapCell>> mapCellResponse = await GetMapObjects();

            if (!mapCellResponse.Success)
            {
                return new MethodResult<List<FortData>>
                {
                    Message = mapCellResponse.Message,
                    Success = false
                };
            }

            return new MethodResult<List<FortData>>
            {
                Data = mapCellResponse.Data.SelectMany(x => x.Forts).ToList(),
                Success = true,
                Message = "Success"
            };
        }

        private async Task<MethodResult<RepeatedField<MapCell>>> GetMapObjects()
        {
            // Update map.
            // not sure or time laps 

            //TODO: Review needed
            TimeSpan secondsSinceLastRequest = DateTime.Now - _lastMapRequest;
            RepeatedField<MapCell> ClientCells = new RepeatedField<MapCell>() { _client.ClientSession.Map.Cells };

            if (secondsSinceLastRequest < TimeSpan.FromSeconds(6)) 
            {
                return new MethodResult<RepeatedField<MapCell>>
                {
                    Data = ClientCells,
                    Success = true
                };
            }

            await _client.ClientSession.RpcClient.RefreshMapObjectsAsync();

            _lastMapRequest = DateTime.Now;

            return new MethodResult<RepeatedField<MapCell>>
            {
                Data = _client.ClientSession.Map.Cells,
                Success = true
            };

            /*/ Or this....
            var cellIds = GetCellIdsForLatLong(_client.ClientSession.Player.Latitude, _client.ClientSession.Player.Longitude);
            var sinceTimeMs = cellIds.Select(x => (long)0).ToArray();

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.GetMapObjects,
                RequestMessage = new GetMapObjectsMessage
                {
                    CellId = { cellIds },
                    Latitude = _client.ClientSession.Player.Latitude,
                    Longitude = _client.ClientSession.Player.Longitude,
                    SinceTimestampMs = { sinceTimeMs }
                }.ToByteString()
            });

            GetMapObjectsResponse getMapObjectsResponse = new GetMapObjectsResponse() { Status = MapObjectsStatus.UnsetStatus };

            try
            {
                getMapObjectsResponse = GetMapObjectsResponse.Parser.ParseFrom(response);

                if (getMapObjectsResponse.MapCells.Count > 1)
                {
                    _lastMapRequest = DateTime.Now;

                    return new MethodResult<RepeatedField<MapCell>>
                    {
                        Data = getMapObjectsResponse.MapCells,
                        Message = "Success",
                        Success = true
                    };
                }
                else
                {
                    return new MethodResult<RepeatedField<MapCell>>
                    {
                        Data = ClientCells,
                        Message = "Success",
                        Success = true
                    };
                }
            }
            catch (Exception ex)
            {
                if (response.IsEmpty)
                    LogCaller(new LoggerEventArgs("Failed to get map objects", Models.LoggerTypes.Exception, ex));

                return new MethodResult<RepeatedField<MapCell>>
                {
                    Data = ClientCells,
                    Message = "Failed to get map objects",
                    Success = false
                };
            }
            */
        }

        /*/TODO referencies to line 195
        private static ulong[] GetCellIdsForLatLong(double latitude, double longitude)
        {
            var latLong = S2LatLng.FromDegrees(latitude, longitude);
            var cell = S2CellId.FromLatLng(latLong);
            var cellId = cell.ParentForLevel(15);
            var cells = cellId.GetEdgeNeighbors();
            var cellIds = new List<ulong>
            {
                cellId.Id
            };

            foreach (var cellEdge1 in cells)
            {
                if (!cellIds.Contains(cellEdge1.Id)) cellIds.Add(cellEdge1.Id);

                foreach (var cellEdge2 in cellEdge1.GetEdgeNeighbors())
                {
                    if (!cellIds.Contains(cellEdge2.Id)) cellIds.Add(cellEdge2.Id);
                }
            }

            return cellIds.ToArray();
        }
        */
    }
}
