using POGOProtos.Enums;
using PokemonGoGUI.Enums;

namespace PokemonGoGUI.Models
{
    public class CatchSetting
    {
        public PokemonId Id { get; set; }
        public bool Catch { get; set; }
        public bool UsePinap { get; set; }
        public bool Evolve { get; set; }
        public int MinEvolveCP { get; set; }
        public bool Transfer { get; set; }
        public TransferType TransferType { get; set; }
        public int KeepMax { get; set; }
        public int MinTransferCP { get; set; }
        public int IVPercent { get; set; }

        public CatchSetting()
        {
            Id = PokemonId.Missingno;
            Catch = true;
            TransferType = TransferType.KeepStrongestX;
            KeepMax = 1;
            MinTransferCP = 0;
            IVPercent = 80;
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
