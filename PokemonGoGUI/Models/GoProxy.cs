using PokemonGo.RocketAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoGUI.Models
{
    public class GoProxy : ProxyEx
    {
        public int MaxConcurrentFails { get; set; }
        public int CurrentConcurrentFails { get; set; }
        public bool IsBanned { get; set; }
        public int MaxAccounts { get; set; }
        public int CurrentAccounts { get; set; }

        public GoProxy()
        {
            MaxConcurrentFails = 3;
            MaxAccounts = 1;
        }

    }
}
