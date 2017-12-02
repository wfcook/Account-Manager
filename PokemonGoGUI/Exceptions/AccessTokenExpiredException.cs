using System;

namespace PokemonGoGUI.Exceptions
{
    public class AccessTokenExpiredException : Exception
    {
        public AccessTokenExpiredException() : base("Server access token has expired")
        {

        }
    }
}