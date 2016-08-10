using System;

namespace PokemonGo.RocketAPI.Exceptions
{
    public class LoginFailedException : Exception
    {
        public LoginFailedException() : base("Failed to login.")
        {

        }
    }

    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException(string message)
            : base(message)
        {

        }
    }

    public class IPBannedException : Exception
    {
        public IPBannedException(string message)
            : base(message)
        {

        }
    }
}