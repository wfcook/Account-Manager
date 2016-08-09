using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
