using PokemonGoGUI.AccountScheduler;
using PokemonGoGUI.GoManager;
using PokemonGoGUI.ProxyManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoGUI.Models
{
    public class ProgramExportModel
    {
        public List<Manager> Managers { get; set; }
        public ProxyHandler ProxyHandler { get; set; }
        public List<Scheduler> Schedulers { get; set; }
        public bool SPF { get; set; }
        public bool ShowWelcomeMessage { get; set; }
    }
}
