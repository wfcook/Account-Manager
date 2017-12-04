using System;

namespace PokemonGoGUI.Exceptions
{
    [Serializable]
    public class AccessTokenExpiredException : Exception
    {
        public AccessTokenExpiredException() : base("Server access token has expired")
        {

        }
    }
}