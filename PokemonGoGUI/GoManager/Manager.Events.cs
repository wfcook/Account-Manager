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
        public delegate void LoggerHandler(object sender, LoggerEventArgs e);
        public event LoggerHandler OnLog;

        public event EventHandler OnInventoryUpdate;

        private void InventoryUpdateCaller(EventArgs args)
        {
            EventHandler caller = OnInventoryUpdate;

            if(caller != null)
            {
                caller(this, args);
            }
        }

        private void LogCaller(LoggerEventArgs args)
        {
            string eMessage = String.Empty;

            if(args.Exception != null)
            {
                eMessage = args.Exception.Message;
            }

            AddLog(new Log(args.LogType, args.Message, eMessage));

            LoggerHandler caller = OnLog;

            if (caller != null)
            {
                caller(this, args);
            }
        }
    }

    public class LoggerEventArgs : EventArgs
    {
        public LoggerTypes LogType { get; private set; }
        public string Message { get; private set; }
        public Exception Exception { get; private set; }

        public LoggerEventArgs(string message, LoggerTypes logtype, Exception exception = null)
        {
            LogType = logtype;
            Message = message;
            Exception = exception;
        }
    }
}
