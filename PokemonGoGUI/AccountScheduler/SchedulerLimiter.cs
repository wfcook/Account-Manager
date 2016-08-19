using PokemonGoGUI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoGUI.AccountScheduler
{
    public class SchedulerLimiter
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public int Offset { get; set; }

        public SchedulerOption Option { get; set; }

        public SchedulerLimiter()
        {
            Option = SchedulerOption.DisableEnable;
        }
    }
}
