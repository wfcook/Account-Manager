using POGOLib.Official.Util.Device;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Settings.Master;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.Enums;
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
                return returnDelay <= 500 ? 500 : returnDelay;
            }
        }

        public MethodResult<AccountExportModel> GetAccountExport()
        {
            if(Stats == null)
            {
                LogCaller(new LoggerEventArgs(String.Format("No stats found for {0}. Please update details", UserSettings.Username), LoggerTypes.Warning));

                return new MethodResult<AccountExportModel>();
            }

            if (!Items.Any()) {
                LogCaller(new LoggerEventArgs(String.Format("No items found for {0}. Please update details", UserSettings.Username), LoggerTypes.Warning));

                return new MethodResult<AccountExportModel>();
            }

            if (!Pokedex.Any()) {
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
                Pokemon = Pokemon.Select(x => new PokemonDataExportModel(x, CalculateIVPerfection(x))).ToList(),
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
                    MethodResult result = await AcLogin();

                    if (!result.Success)
                    {
                        return new MethodResult<Dictionary<PokemonId, PokemonSettings>>
                        {
                            Message = result.Message
                        };
                    }
                }

                var pokemonSettings = new Dictionary<PokemonId, PokemonSettings>();
                var moveSettings = new Dictionary<PokemonMove, MoveSettings>();
                var badgeSettings = new Dictionary<BadgeType, BadgeSettings>();
                var itemSettings = new Dictionary<ItemId, ItemSettings>();
                var battleSettings = new GymBattleSettings();
                var upgradeSettings = new PokemonUpgradeSettings();
                var moveSequenceSettings = new MoveSequenceSettings();
                var encounterSettings = new EncounterSettings();
                var iapItemDisplay = new IapItemDisplay();
                var iapSettings = new IapSettings();
                var equippedBadge = new EquippedBadgeSettings();
                var questSettings = new QuestSettings();
                var avatarCustomization = new AvatarCustomizationSettings();
                var formSettings = new FormSettings();
                var genderSettings = new GenderSettings();
                var gymBadgeSettings = new GymBadgeGmtSettings();
                var weatherAffinities = new WeatherAffinity();
                var weatherBonusSettings = new WeatherBonus();
                var pokemonScaleSettings = new PokemonScaleSetting();
                var typeEffective = new TypeEffectiveSettings();
                var camera = new CameraSettings();
                var gymLevel = new GymLevelSettings();

                foreach (var template in _client.ClientSession.Templates.ItemTemplates)
                {
                    if (template.PlayerLevel != null)
                    {
                        LevelSettings = template.PlayerLevel;

                        continue;
                    }

                    if (template.PokemonSettings != null)
                    {
                        if (pokemonSettings.ContainsKey(template.PokemonSettings.PokemonId))
                            pokemonSettings.Remove(template.PokemonSettings.PokemonId);
                        pokemonSettings.Add(template.PokemonSettings.PokemonId, template.PokemonSettings);
                    }
                    else if (template.MoveSettings != null)
                    {
                        if (moveSettings.ContainsKey(template.MoveSettings.MovementId))
                            moveSettings.Remove(template.MoveSettings.MovementId);
                        moveSettings.Add(template.MoveSettings.MovementId, template.MoveSettings);
                    }
                    else if (template.BadgeSettings != null)
                    {
                        if (badgeSettings.ContainsKey(template.BadgeSettings.BadgeType))
                            badgeSettings.Remove(template.BadgeSettings.BadgeType);
                        badgeSettings.Add(template.BadgeSettings.BadgeType, template.BadgeSettings);
                    }
                    else if (template.ItemSettings != null)
                    {
                        if (itemSettings.ContainsKey(template.ItemSettings.ItemId))
                            itemSettings.Remove(template.ItemSettings.ItemId);
                        itemSettings.Add(template.ItemSettings.ItemId, template.ItemSettings);
                    }
                    else if (template.EncounterSettings != null)
                    {
                        encounterSettings = template.EncounterSettings;
                    }
                    else if (template.MoveSequenceSettings != null)
                    {
                        moveSequenceSettings = template.MoveSequenceSettings;
                    }
                    else if (template.BattleSettings != null)
                    {
                        battleSettings = template.BattleSettings;
                    }
                    else if (template.IapItemDisplay != null)
                    {
                        iapItemDisplay = template.IapItemDisplay;
                    }
                    else if (template.IapSettings != null)
                    {
                        iapSettings = template.IapSettings;
                    }
                    else if (template.EquippedBadges != null)
                    {
                        equippedBadge = template.EquippedBadges;
                    }
                    else if (template.QuestSettings != null)
                    {
                        questSettings = template.QuestSettings;
                    }
                    else if (template.AvatarCustomization != null)
                    {
                        avatarCustomization = template.AvatarCustomization;
                    }
                    else if (template.FormSettings != null)
                    {
                        formSettings = template.FormSettings;
                    }
                    else if (template.GenderSettings != null)
                    {
                        genderSettings = template.GenderSettings;
                    }
                    else if (template.GymBadgeSettings != null)
                    {
                        gymBadgeSettings = template.GymBadgeSettings;
                    }
                    else if (template.WeatherAffinities != null)
                    {
                        weatherAffinities = template.WeatherAffinities;
                    }
                    else if (template.WeatherBonusSettings != null)
                    {
                        weatherBonusSettings = template.WeatherBonusSettings;
                    }
                    else if (template.PokemonScaleSettings != null)
                    {
                        pokemonScaleSettings = template.PokemonScaleSettings;
                    }
                    else if (template.TypeEffective != null)
                    {
                        typeEffective = template.TypeEffective;
                    }
                    else if (template.Camera != null)
                    {
                        camera = template.Camera;
                    }
                    else if (template.GymLevel != null)
                    {
                        gymLevel = template.GymLevel;
                    }
                    else if (template.PokemonUpgrades != null)
                    {
                        upgradeSettings = template.PokemonUpgrades;
                    }
                }

                PokeSettings = pokemonSettings;
                MoveSettings = moveSettings;
                BadgeSettings = badgeSettings;
                ItemSettings = itemSettings;
                BadgeSettings = badgeSettings;
                UpgradeSettings = upgradeSettings;
                GetMoveSequenceSettings = moveSequenceSettings;
                GetEncounterSettings = encounterSettings;
                GetIapItemDisplay = iapItemDisplay;
                GetIapSettings = iapSettings;
                GetEquippedBadgeSettings = equippedBadge;
                GetQuestSettings = questSettings;
                GetAvatarCustomizationSettings = avatarCustomization;
                GetFormSettings = formSettings;
                GetGenderSettings = genderSettings;
                GetGymBadgeGmtSettings = gymBadgeSettings;
                GetWeatherAffinity = weatherAffinities;
                GetWeatherBonus = weatherBonusSettings;
                GetPokemonScaleSetting = pokemonScaleSettings;
                GetTypeEffectiveSettings = typeEffective;
                GetCameraSettings = camera;
                GetGymLevelSettings = gymLevel;

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

                //new values added 

                settings.Latitude = settings.Latitude;
                settings.Longitude = settings.Longitude;
                settings.Altitude = settings.Altitude;
                settings.Country = settings.Country;
                settings.Language = settings.Language;
                settings.TimeZone = settings.TimeZone;
                settings.POSIX = settings.POSIX;
                //Randomize device id
                var device = DeviceInfoUtil.GetRandomDevice();                
                settings.DeviceId = device.DeviceInfo.DeviceId;
                settings.DeviceBrand = settings.DeviceBrand;
                settings.DeviceModel = settings.DeviceModel;
                settings.DeviceModelBoot = settings.DeviceModelBoot;
                settings.HardwareManufacturer = settings.HardwareManufacturer;
                settings.HardwareModel = settings.HardwareModel;
                settings.FirmwareBrand = settings.FirmwareBrand;
                settings.FirmwareType = settings.FirmwareType;

                foreach (var element in settings.EvolveSettings)
                {
                    var pokemonSetting = settings.EvolveSettings.FirstOrDefault(x => x.Id == element.Id);
                    if (pokemonSetting != null)
                    {
                        pokemonSetting.Evolve = element.Evolve;
                        pokemonSetting.MinCP = element.MinCP;
                    }
                }
                foreach (var element in settings.TransferSettings)
                {
                    var pokemonSetting = settings.TransferSettings.FirstOrDefault(x => x.Id == element.Id);
                    if (pokemonSetting != null)
                    {
                        pokemonSetting.Transfer = element.Transfer;
                        pokemonSetting.MinCP = element.MinCP;
                        pokemonSetting.IVPercent = element.IVPercent;
                        pokemonSetting.KeepMax = element.KeepMax;
                        pokemonSetting.Type = element.Type;
                    }
                }

                UserSettings = settings;

                if (String.IsNullOrEmpty(UserSettings.DeviceBrand))
                {
                    UserSettings.RandomizeDevice();
                }

                LogCaller(new LoggerEventArgs("Successfully imported config file", LoggerTypes.Info));

                return new MethodResult
                {
                    Message = "Success",
                    Success = true
                };
            }
            catch (Exception ex)
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
