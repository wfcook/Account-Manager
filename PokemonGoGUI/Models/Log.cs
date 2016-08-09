using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager.Models
{
    public enum LoggerTypes { Debug, Info, Warning, Exception, PokemonEscape, PokemonFlee, LocationUpdate, Transfer, Evolve, Incubate, Recycle, Success };

    public class Log
    {
        public LoggerTypes LoggerType { get; private set; }
        public DateTime Date { get; private set; }
        public string Message { get; private set; }
        //public Exception Exception { get; private set; }
        public string ExceptionMessage { get; private set; }

        /*
        public string ExceptionMessage
        {
            get
            {
                if(Exception == null)
                {
                    return String.Empty;

                }

                return Exception.Message;
            }
        }*/

        public Log(LoggerTypes type, string message, string exceptionMessage = null)
        {
            this.LoggerType = type;
            this.Message = message;
            this.Date = DateTime.Now;
            this.ExceptionMessage = exceptionMessage;
        }
    }
}
