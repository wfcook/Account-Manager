using PokemonGoGUI.GoManager.Models;
using System;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        public async Task<bool> ChangeProxy()
        {
            if (_proxyIssue && CurrentProxy != null)
            {
                ProxyHandler.IncreaseFailCounter(CurrentProxy);
            }

            RemoveProxy();

            //Get new
            //This call will increment the proxy
            CurrentProxy = ProxyHandler.GetRandomProxy();

            if (CurrentProxy == null)
            {
                LogCaller(new LoggerEventArgs("No available proxies left. Will recheck every 5 seconds", LoggerTypes.Warning));
            }

            while (CurrentProxy == null && IsRunning)
            {
                await Task.Delay(5000);

                CurrentProxy = ProxyHandler.GetRandomProxy();
            }

            //Program is stopping
            if (CurrentProxy == null)
            {
                return false;
            }

            UserSettings.ProxyIP = CurrentProxy.Address;
            UserSettings.ProxyPort = CurrentProxy.Port;
            UserSettings.ProxyUsername = CurrentProxy.Username;
            UserSettings.ProxyPassword = CurrentProxy.Password;


            LogCaller(new LoggerEventArgs(String.Format("Changing proxy to {0}", CurrentProxy.ToString()), LoggerTypes.Info));

            return true;
        }

        public void RemoveProxy()
        {
            //Decrease usage
            if (CurrentProxy != null)
            {
                ProxyHandler.ProxyUsed(CurrentProxy, false);
                CurrentProxy = null;

                UserSettings.ProxyIP = String.Empty;
                UserSettings.ProxyPort = 0;
                UserSettings.ProxyUsername = String.Empty;
                UserSettings.ProxyPassword = String.Empty;
            }
        }
    }
}
