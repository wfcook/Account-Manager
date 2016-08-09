using PokemonGoGUI.GoManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        public void AddLog(Log log)
        {
            lock (Logs)
            {
                if (log.LoggerType != LoggerTypes.LocationUpdate)
                {
                    Logs.Add(log);
                }

                if(Logs.Count >= 100)
                {
                    IEnumerable<Log> tempLogs = Logs.Reverse<Log>().Take(50);

                    Logs = tempLogs.Reverse<Log>().ToList();
                }
            }
        }

        public void ClearLog()
        {
            lock(Logs)
            {
                Logs.Clear();
            }
        }
    }
}
