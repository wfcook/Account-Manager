using POGOProtos.Data;
using PokemonGo.RocketAPI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoGUI.Models
{
    public class AccountExportModel
    {
        public AuthType type { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public int level { get; set; }
        public List<PokedexEntryExportModel> pokedex { get; set; }
    }

    public class PokedexEntryExportModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public int timesEncountered { get; set; }
        public int timesCaught { get; set; }

        public PokedexEntryExportModel(PokedexEntry entry)
        {
            id = (int)entry.PokemonId;
            name = entry.PokemonId.ToString();
            timesEncountered = entry.TimesEncountered;
            timesCaught = entry.TimesCaptured;
        }
    }
}
