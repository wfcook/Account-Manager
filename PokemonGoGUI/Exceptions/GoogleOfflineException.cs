using System;

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
