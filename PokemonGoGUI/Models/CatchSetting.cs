using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoGUI.Models
{
    public class CatchSetting
    {
        public PokemonId Id { get; set; }
        public bool Catch { get; set; }
        public bool Snipe { get; set; }

        public CatchSetting()
        {
            Id = PokemonId.Missingno;
            Catch = true;
            Snipe = false;
        }

        public string Name
        {
            get
            {
                return Id.ToString();
            }
        }
    }
}
