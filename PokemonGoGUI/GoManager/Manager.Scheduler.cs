using PokemonGoGUI.AccountScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        public void AddScheduler(Scheduler scheduler)
        {
            scheduler.OnSchedule += scheduler_OnSchedule;
        }

        public void RemoveScheduler(Scheduler scheduler)
        {
            scheduler.OnSchedule -=scheduler_OnSchedule;
        }

        private void scheduler_OnSchedule(object sender, SchedulerEventArgs e)
        {

        }

    }
}
