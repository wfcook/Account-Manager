using POGOProtos.Enums;

namespace PokemonGoGUI.Models
{
    public class UpgradeSetting
    {
        public PokemonId Id { get; set; }
        public bool Upgrade { get; set; }

        public UpgradeSetting()
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