using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoGUI.AccountScheduler
{
    public class Scheduler
    {
        public double StartTime { get; set; }
        public double EndTime { get; set; }

        public SchedulerLimiter PokeStoplimiter { get; set; }
        public SchedulerLimiter PokemonLimiter { get; set; }

        public Scheduler()
        {
            PokeStoplimiter = new SchedulerLimiter();
            PokemonLimiter = new SchedulerLimiter();
        }
    }
}
