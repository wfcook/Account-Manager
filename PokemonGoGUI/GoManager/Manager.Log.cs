using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

                if(Logs.Count >= UserSettings.MaxLogs)
                {
                    IEnumerable<Log> tempLogs = Logs.Reverse<Log>().Take(UserSettings.MaxLogs / 4); ;

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

        public async Task<MethodResult> ExportLogs(string filename)
        {
            try
            {
                string data = String.Empty;

                lock(Logs)
                {
                    data = Serializer.ToJson<List<Log>>(Logs);
                }

                await Task.Run(() => File.WriteAllText(filename, data));

                return new MethodResult
                {
                    Message = "Success",
                    Success = true
                };
            }
            catch(Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to export logs", LoggerTypes.Exception, ex));

                return new MethodResult
                {
                    Message = ex.Message
                };
            }

        }
    }
}
