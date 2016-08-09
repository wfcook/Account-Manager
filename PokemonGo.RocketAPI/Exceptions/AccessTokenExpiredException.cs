using System;

namespace PokemonGo.RocketAPI.Exceptions
{
    public class AccessTokenExpiredException : Exception
    {
        public AccessTokenExpiredException() : base("Server access token has expired")
        {

        }
    }
}