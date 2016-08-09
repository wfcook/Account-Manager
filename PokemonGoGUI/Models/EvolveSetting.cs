using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoGUI.Models
{
    public class EvolveSetting
    {
        public PokemonId Id { get; set; }
        public bool Evolve { get; set; }
        public int MinCP { get; set; }

        public EvolveSetting()
        {
            Id = PokemonId.Missingno;
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
