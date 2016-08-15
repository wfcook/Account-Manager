using Newtonsoft.Json;
using POGOProtos.Data;
using POGOProtos.Data.Player;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings.Master;
using PokemonGo.RocketAPI;
using PokemonGoGUI.Enums;
using PokemonGoGUI.GoManager.Models;
using PokemonGoGUI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        public Settings UserSettings { get; set; }

        public byte[] LogHeaderSettings { get; set; }

        public AccountState AccountState { get; set; }

        [JsonIgnore]
        public string GroupName
        {
            get
            {
                if (String.IsNullOrEmpty(UserSettings.GroupName))
                {
                    return String.Empty;
                }

                return UserSettings.GroupName;
            }
        }

        [JsonIgnore]
        public string Proxy
        {
            get
            {
                ProxyEx proxyEx = new ProxyEx
                {
                    Address = UserSettings.ProxyIP,
                    Port = UserSettings.ProxyPort,
                    Username = UserSettings.ProxyUsername,
                    Password = UserSettings.ProxyPassword
                };

                return proxyEx.ToString();
            }
        }

        [JsonIgnore]
        public BotState State { get; set; }

        [JsonIgnore]
        public PlayerData PlayerData { get; private set; }

        [JsonIgnore]
        public List<Log> Logs { get; private set; }

        [JsonIgnore]
        public PlayerStats Stats { get; private set; }

        [JsonIgnore]
        public List<InventoryItem> AllItems { get; private set; }

        [JsonIgnore]
        public List<ItemData> Items { get; private set; }

        [JsonIgnore]
        public List<PokemonData> Pokemon { get; private set; }

        [JsonIgnore]
        public List<PokedexEntry> Pokedex { get; private set; }

        [JsonIgnore]
        public List<Candy> PokemonCandy { get; private set; }

        [JsonIgnore]
        public List<EggIncubator> Incubators { get; private set; }

        [JsonIgnore]
        public List<PokemonData> Eggs { get; private set; }

        [JsonIgnore]
        public Dictionary<PokemonId, PokemonSettings> PokeSettings { get; private set; }

        [JsonIgnore]
        public PlayerLevelSettings LevelSettings { get; private set; }

        [JsonIgnore]
        public List<FarmLocation> FarmLocations { get; private set; }

        [JsonIgnore]
        public bool IsRunning { get; private set; }

        [JsonIgnore]
        public bool StartingUp { get; private set; }

        [JsonIgnore]
        public int TotalLogs
        {
            get
            {
                if (Logs == null)
                {
                    return 0;
                }

                return Logs.Count;
            }
        }

        [JsonIgnore]
        public string LastLogMessage
        {
            get
            {
                if (Logs == null || Logs.Count == 0)
                {
                    return String.Empty;
                }

                lock (Logs)
                {
                    string message = Logs.LastOrDefault(x => x.LoggerType != LoggerTypes.LocationUpdate).Message;

                    if (String.IsNullOrEmpty(message))
                    {
                        return String.Empty;
                    }

                    return message;
                }
            }
        }

        [JsonIgnore]
        public string AccountName
        {
            get
            {
                if (UserSettings == null)
                {
                    return "???";
                }

                return UserSettings.AccountName;
            }
        }

        [JsonIgnore]
        public int Level
        {
            get
            {
                if (Stats == null)
                {
                    return 0;
                }

                return Stats.Level;
            }
        }

        [JsonIgnore]
        public int MaxLevel
        {
            get
            {
                if(UserSettings == null)
                {
                    return 0;
                }

                return UserSettings.MaxLevel;
            }
        }

        [JsonIgnore]
        public string TillLevelUp
        {
            get
            {
                if(ExpPerHour == 0)
                {
                    return "Unknown";
                }

                long currentExp = Stats.Experience - Stats.PrevLevelXp;
                long required = Stats.NextLevelXp - Stats.PrevLevelXp;
                long needed = required - currentExp;

                int expPerHour = ExpPerHour;

                double totalHours = (double)needed / expPerHour;

                if(totalHours <= 0)
                {
                    return "Now";
                }

                TimeSpan time = TimeSpan.FromHours(totalHours);

                if(time.TotalHours < 1)
                {
                    return String.Format("{0:0}m {1:00}s", time.Minutes, time.Seconds);
                }

                if(time.TotalHours >= 24)
                {
                    return String.Format("{0:0}d {1:0}h {2:00}m", time.Days, time.Hours, time.Seconds);
                }

                return String.Format("{0:0}h {1:00}m {2:00}s", time.Hours, time.Minutes, time.Seconds);
            }
        }

        [JsonIgnore]
        public string ExpRatio
        {
            get
            {
                if (Stats == null)
                {
                    return "??/??";
                }

                long currentExp = Stats.Experience - Stats.PrevLevelXp;
                long required = Stats.NextLevelXp - Stats.PrevLevelXp;

                double ratio = 0;

                if (required != 0)
                {
                    ratio = (double)currentExp / required * 100;
                }

                return String.Format("{0}/{1} ({2:0.00}%)", currentExp, required, ratio);
            }
        }

        [JsonIgnore]
        public int MaxItemStorage
        {
            get
            {
                if (PlayerData == null)
                {
                    return 350;
                }

                return PlayerData.MaxItemStorage;
            }
        }

        [JsonIgnore]
        public int MaxPokemonStorage
        {
            get
            {
                if(PlayerData == null)
                {
                    return 250;
                }

                return PlayerData.MaxPokemonStorage;
            }
        }

        [JsonIgnore]
        public int TotalStardust
        {
            get
            {
                if(PlayerData == null)
                {
                    return 0;
                }

                Currency stardust = PlayerData.Currencies.FirstOrDefault(x => x.Name == "STARDUST");

                if(stardust == null)
                {
                    return 0;
                }

                return stardust.Amount;
            }
        }

        [JsonIgnore]
        public int ExpPerHour
        {
            get
            {
                double totalHours = _runningStopwatch.Elapsed.TotalHours;

                if(totalHours == 0)
                {
                    return 0;
                }

                double expPerHour = _expGained / totalHours;

                return (int)expPerHour;
            }
        }

        [JsonIgnore]
        public string RunningTime
        {
            get
            {
                //return String.Format("{0:c}", _runningStopwatch.Elapsed);
                return _runningStopwatch.Elapsed.ToString(@"dd\.hh\:mm\:ss");
            }
        }

        [JsonIgnore]
        public int ExpGained
        {
            get
            {
                return _expGained;
            }
        }

        [JsonIgnore]
        public int PokemonCaught { get; set; }

        [JsonIgnore]
        public int PokestopsFarmed { get; set; }

        [JsonIgnore]
        public int ItemsFarmed { get; set; }

        [JsonIgnore]
        public int TotalPokeStopExp { get; set; }

        private Stopwatch _runningStopwatch = new Stopwatch();
        private int _expGained = 0;

        private void ExpIncrease(int amount)
        {
            _expGained += amount;
            Stats.Experience += amount;
        }

        private int RemainingPokeballs()
        {
            int total = 0;

            ItemData data = Items.FirstOrDefault(x => x.ItemId == ItemId.ItemPokeBall);

            if(data != null)
            {
                total += data.Count;
            }

            data = Items.FirstOrDefault(x => x.ItemId == ItemId.ItemGreatBall);

            if (data != null)
            {
                total += data.Count;
            }
            data = Items.FirstOrDefault(x => x.ItemId == ItemId.ItemUltraBall);

            if (data != null)
            {
                total += data.Count;
            }

            data = Items.FirstOrDefault(x => x.ItemId == ItemId.ItemMasterBall);

            if (data != null)
            {
                total += data.Count;
            }

            return total;
        }

        private bool HasPokeballsLeft()
        {
            return RemainingPokeballs() > 0;
        }
    }
}
