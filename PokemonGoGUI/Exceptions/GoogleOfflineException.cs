using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoGUI.Exceptions
{
    [Serializable]
    public class GoogleOfflineException : Exception
    {
        public GoogleOfflineException() : base("Google login servers are offline")
        {

        }
    }
}
