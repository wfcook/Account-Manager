using GeoCoordinatePortable;
using Google.Protobuf;
using POGOProtos.Map;
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
        private async Task<MethodResult<List<MapPokemon>>> GetCatchablePokemon()
        {
            MethodResult<List<MapCell>> mapCellResponse = await GetMapObjects();

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
                if(fort.Type != FortType.Checkpoint || fort.CooldownCompleteTimestampMs > DateTime.UtcNow.ToUnixTime())
                {
                    continue;
                }

                GeoCoordinate defaultLocation = new GeoCoordinate(UserSettings.DefaultLatitude, UserSettings.DefaultLongitude);
                GeoCoordinate fortLocation = new GeoCoordinate(fort.Latitude, fort.Longitude);

                double distance = CalculateDistanceInMeters(defaultLocation, fortLocation);

                if(distance > UserSettings.MaxTravelDistance)
                {
                    continue;
                }

                fortData.Add(fort);
            }

            if(fortData.Count == 0)
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
            MethodResult<List<MapCell>> mapCellResponse = await GetMapObjects();

            if (!mapCellResponse.Success)
            {
                return new MethodResult<List<FortData>>
                {
                    Message = mapCellResponse.Message
                };
            }

            return new MethodResult<List<FortData>>
            {
                Data = mapCellResponse.Data.SelectMany(x => x.Forts).ToList(),
                Success = true,
                Message = "Success"
            };
        }

        private async Task<MethodResult<List<MapCell>>> GetMapObjects()
        {
            TimeSpan secondsSinceLastRequest = DateTime.Now - _lastMapRequest;

            if (secondsSinceLastRequest < TimeSpan.FromSeconds(6))
            {
                return new MethodResult<List<MapCell>>
                {
                    Data = new List<MapCell>(),
                    Success = true
                };
            }

            await Task.Delay(0);           
            var cells = _client.ClientSession.Map.Cells.ToList();
            if (cells.Count > 1)
            {
                _lastMapRequest = DateTime.Now;

                return new MethodResult<List<MapCell>>
                {
                    Data = cells,
                    Message = "Success",
                    Success = true
                };
            }
            else
            {
                return new MethodResult<List<MapCell>>
                {
                    Data = new List<MapCell>(),
                    Message = "Failed to get map objects"
                };
            }
            /*/ Retrieve the closest fort to your current player coordinates.
            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.GetMapObjects,
                RequestMessage = new GetMapObjectsMessage
                {
                    //CellId,
                    Latitude = _client.ClientSession.Player.Latitude,
                    Longitude = _client.ClientSession.Player.Longitude,
                    //SinceTimestampMs 
                }.ToByteString()
            });

            GetMapObjectsResponse getMapObjectsResponse = null;
            try
            {
                getMapObjectsResponse = GetMapObjectsResponse.Parser.ParseFrom(response);
                _lastMapRequest = DateTime.Now;
 
                return new MethodResult<List<MapCell>>
                {
                    Data = getMapObjectsResponse.MapCells.ToList(),
                    Message = "Success",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                if (response.IsEmpty)
                    LogCaller(new LoggerEventArgs("Failed to get map objects", Models.LoggerTypes.Exception, ex));

                return new MethodResult<List<MapCell>>
                {
                    Data = new List<MapCell>(),
                    Message = "Failed to get map objects"
                };
            }
            */
        }
    }
}
