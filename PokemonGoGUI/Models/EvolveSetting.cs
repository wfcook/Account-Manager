using POGOProtos.Enums;

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
