using System;

namespace PokemonGoGUI.Models
{
    public class AccountImport
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }
        public int MaxLevel { get; set; }

        public AccountImport()
        {
        }

        public bool ParseAccount(string accountInfo)
        {
            string[] parts = accountInfo.Split(':');

            int maxLevel = 0;

            if (parts.Length < 2 || parts.Length > 7)
            {
                return false;
            }

            //Account info
            Username = parts[0];
            Password = parts[1];

            //Contains max level
            if(parts.Length % 2 == 1)
            {
                string stringMaxLevel = parts[parts.Length - 1];

                if(!Int32.TryParse(stringMaxLevel, out maxLevel))
                {
                    return false;
                }
            }

            //Max level
            MaxLevel = maxLevel;

            //Username and pass
            Username = parts[0];
            Password = parts[1];

            //Proxy info
            if (parts.Length >= 4)
            {
                string stringProxyPort = String.Empty;
                int port = 0;

                if(!Int32.TryParse(parts[3], out port))
                {
                    return false;
                }

                Address = parts[2];
                Port = port;

                //Proxy credentials
                if(parts.Length >= 6)
                {
                    ProxyUsername = parts[4];
                    ProxyPassword = parts[5];
                }
            }

            return true;
        }
    }
}
