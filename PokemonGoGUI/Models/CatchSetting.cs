using POGOProtos.Enums;

namespace PokemonGoGUI.Models
{
    public class CatchSetting
    {
        public PokemonId Id { get; set; }
        public bool Catch { get; set; }
        public bool UsePinap { get; set; }
        public bool Snipe { get; set; }

        public CatchSetting()
        {
            Id = PokemonId.Missingno;
            Catch = true;
            //Snipe = true;
            UsePinap = true;
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
