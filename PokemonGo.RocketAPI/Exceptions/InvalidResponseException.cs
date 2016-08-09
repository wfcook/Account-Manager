using System;

namespace PokemonGo.RocketAPI.Exceptions
{
    public class InvalidResponseException : Exception
    {
        public InvalidResponseException()
            : base("The server has returned an invalid response")
        { }
    }
}