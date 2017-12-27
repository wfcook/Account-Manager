using Google.Protobuf;
using POGOProtos.Enums;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings.Master;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager.Models;
using PokemonGoGUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        private int CalculateDelay(int baseDelay, int offset)
        {
            lock(_rand)
            {
                int maxOffset = offset * 2;

                int currentOffset = _rand.Next(0, maxOffset + 1) - offset;

                int returnDelay = baseDelay + currentOffset;

                //API throttles
                if(returnDelay <= 500)
                {
                    return 500;
                }

                return returnDelay;
            }
        }

        public MethodResult<AccountExportModel> GetAccountExport()
        {
            if(Stats == null)
            {
                LogCaller(new LoggerEventArgs(String.Format("No stats found for {0}. Please update details", UserSettings.Username), LoggerTypes.Warning));

                return new MethodResult<AccountExportModel>();
            }

            var AllItems =  _client.ClientSession.Player.Inventory.InventoryItems;
            if (AllItems == null || AllItems.Count == 0)
            {
                LogCaller(new LoggerEventArgs(String.Format("No items found for {0}. Please update details", UserSettings.Username), LoggerTypes.Warning));

                return new MethodResult<AccountExportModel>();
            }

            if (Pokedex == null || Pokedex.Count == 0)
            {
                LogCaller(new LoggerEventArgs(String.Format("No pokedex found for {0}. Please update details", UserSettings.Username), LoggerTypes.Warning));

                return new MethodResult<AccountExportModel>();
            }

            var exportModel = new AccountExportModel
            {
                Level = Stats.Level,
                Type = UserSettings.AuthType,
                Username = UserSettings.Username,
                Password = UserSettings.Password,
                Pokedex = Pokedex.Select(x => new PokedexEntryExportModel(x)).ToList(),
                Pokemon = Pokemon.Select(x => new PokemonDataExportModel(x, CalculateIVPerfection(x).Data)).ToList(),
                Items = Items.Select(x => new ItemDataExportModel(x)).ToList(),
                Eggs = Eggs.Select(x => new EggDataExportModel(x)).ToList()
            };

            return new MethodResult<AccountExportModel>
            {
                Data = exportModel,
                Success = true
            };
        }

        public async Task<MethodResult<Dictionary<PokemonId, PokemonSettings>>> GetItemTemplates()
        {
            if (PokeSettings != null && PokeSettings.Count != 0)
            {
                return new MethodResult<Dictionary<PokemonId, PokemonSettings>>
                {
                    Data = PokeSettings,
                    Message = "Settings already grabbed",
                    Success = true
                };
            }

            try
            {
                if (!_client.LoggedIn)
                {
                    MethodResult result = await Login_();

                    if (!result.Success)
                    {
                        return new MethodResult<Dictionary<PokemonId, PokemonSettings>>
                        {
                            Message = result.Message
                        };
                    }
                }

                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.DownloadItemTemplates,
                    RequestMessage = new DownloadItemTemplatesMessage
                    {
                        PageOffset = 0,
                        Paginate = false,
                        PageTimestamp = 0
                    }.ToByteString()
                });

                DownloadItemTemplatesResponse downloadItemTemplatesResponse = null;

                downloadItemTemplatesResponse = DownloadItemTemplatesResponse.Parser.ParseFrom(response);
                var pokemonSettings = new Dictionary<PokemonId, PokemonSettings>();

                foreach (var template in downloadItemTemplatesResponse.ItemTemplates)
                {
                    if (template.PlayerLevel != null)
                    {
                        LevelSettings = template.PlayerLevel;

                        continue;
                    }

                    if (template.PokemonSettings == null)
                    {
                        continue;
                    }

                    pokemonSettings.Add(template.PokemonSettings.PokemonId, template.PokemonSettings);
                }

                PokeSettings = pokemonSettings;

                return new MethodResult<Dictionary<PokemonId, PokemonSettings>>
                {
                    Data = pokemonSettings,
                    Message = "Success",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to get setting templates", LoggerTypes.Exception, ex));

                return new MethodResult<Dictionary<PokemonId, PokemonSettings>>
                {
                    Message = "Failed to get setting templates"
                };
            }
        }

        public MethodResult<PokemonSettings> GetPokemonSetting(PokemonId pokemon)
        {
            if(PokeSettings == null)
            {
                return new MethodResult<PokemonSettings>
                {
                    Message = "Settings haven't been grabbed yet"
                };
            }

            if (pokemon == PokemonId.Missingno)
            {
                return new MethodResult<PokemonSettings>
                {
                    Message = "No settings on Missingno"
                };
            }

            //Shouldn't happen
            PokemonSettings pokemonSettings;
            if (!PokeSettings.TryGetValue(pokemon, out  pokemonSettings))
            {
                return new MethodResult<PokemonSettings>()
                {
                    Message = "Pokemon Id does not exist"
                };
            }

            return new MethodResult<PokemonSettings>()
            {
                Data = pokemonSettings,
                Message = "Success",
                Success = true
            };
        }

        public async Task<MethodResult> ExportConfig(string filename)
        {
            try
            {
                string usernameTemp = UserSettings.Username;
                string passwordTemp = UserSettings.Password;
                string nameTemp = UserSettings.AccountName;

                UserSettings.AccountName = String.Empty;
                UserSettings.Password = String.Empty;
                UserSettings.Username = String.Empty;

                string data = Serializer.ToJson<Settings>(UserSettings);

                UserSettings.AccountName = nameTemp;
                UserSettings.Password = passwordTemp;
                UserSettings.Username = usernameTemp;

                await Task.Run(() => File.WriteAllText(filename, data));

                LogCaller(new LoggerEventArgs("Successfully exported config file", LoggerTypes.Info));

                return new MethodResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to export config file", LoggerTypes.Exception, ex));

                return new MethodResult
                {
                    Message = ex.Message
                };
            }
        }

        public MethodResult ImportConfig(string data)
        {
            try
            {
                Settings settings = Serializer.FromJson<Settings>(data);
                settings.AccountName = UserSettings.AccountName;
                settings.Password = UserSettings.Password;
                settings.Username = UserSettings.Username;
                settings.AuthType = UserSettings.AuthType;
                settings.ProxyIP = UserSettings.ProxyIP;
                settings.ProxyPassword = UserSettings.ProxyPassword;
                settings.ProxyPort = UserSettings.ProxyPort;
                settings.ProxyUsername = UserSettings.ProxyUsername;
                settings.GroupName = UserSettings.GroupName;

                UserSettings = settings;

                if (!String.IsNullOrEmpty(UserSettings.DeviceBrand))
                {
                    UserSettings.RandomizeDevice();
                }
                else
                {
                    UserSettings.LoadDeviceSettings();
                }

                LogCaller(new LoggerEventArgs("Successfully imported config file", LoggerTypes.Info));

                return new MethodResult
                {
                    Message = "Success",
                    Success = true
                };
            }
            catch(Exception ex)
            {
                string message = String.Format("Failed to import config. Ex: {0}", ex.Message);

                LogCaller(new LoggerEventArgs(message, LoggerTypes.Exception, ex));

                return new MethodResult
                {
                    Message = message
                };
            }
        }

        public async Task<MethodResult> ImportConfigFromFile(string fileName)
        {
            try
            {
                if(!File.Exists(fileName))
                {
                    return new MethodResult
                    {
                        Message = "File does not exist"
                    };
                }

                string data = await Task.Run(() => File.ReadAllText(fileName));

                return ImportConfig(data);
            }
            catch(Exception ex)
            {
                string message = String.Format("Failed to load config file.");

                LogCaller(new LoggerEventArgs(message, LoggerTypes.Exception, ex));

                return new MethodResult
                {
                    Message = message
                };
            }
        }

        public void RestoreCatchDefaults()
        {
            UserSettings.LoadCatchSettings();
        }

        public void RestoreInventoryDefaults()
        {
            UserSettings.LoadInventorySettings();
        }

        public void RestoreEvolveDefaults()
        {
            UserSettings.LoadEvolveSettings();
        }

        public void RestoreTransferDefaults()
        {
            UserSettings.LoadTransferSettings();
        }

        public void RestoreDeviceDefaults()
        {
            UserSettings.LoadDeviceSettings();
        }

        public void RandomDeviceId()
        {
            UserSettings.RandomizeDeviceId();
        }
        public void RandomDevice()
        {
            UserSettings.RandomizeDevice();
        }
        
        public override bool Equals(object obj)
        {
            var tempManager = obj as Manager;

            if(tempManager == null)
            {
                return base.Equals(obj);
            }

            return tempManager.UserSettings.Username == this.UserSettings.Username;
        }

        public override int GetHashCode()
        {
            return UserSettings.Username == null ? 0 : UserSettings.Username.GetHashCode();
        }
    }
}
