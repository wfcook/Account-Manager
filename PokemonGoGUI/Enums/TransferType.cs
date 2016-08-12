using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoGUI.Enums
{
    public enum TransferType
    {
        All,
        KeepStrongestX,
        KeepPossibleEvolves,
        BelowIVPercentage,
        BelowCP,
        KeepXHighestIV,
        BelowCPOrIVAmount,
        BelowCPAndIVAmount
    };
}
