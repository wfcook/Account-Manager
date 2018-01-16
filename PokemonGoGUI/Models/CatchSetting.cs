using POGOProtos.Enums;

namespace PokemonGoGUI.Models
{
    public class CatchSetting
    {
        public PokemonId Id { get; set; }
        public bool Catch { get; set; }
        public bool UsePinap { get; set; }

        public CatchSetting()
        {
            Id = PokemonId.Missingno;
            Catch = true;
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
