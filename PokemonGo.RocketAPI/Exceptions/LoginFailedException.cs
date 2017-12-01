#region using directives

using System;

#endregion

namespace PokemonGo.RocketAPI.Exceptions
{
    
    public class LoginFailedException : Exception
    {

        public LoginFailedException()
        {
        }

        public LoginFailedException(string message) : base(message)
        {
        }
    }

    public class TokenRefreshException : Exception
    {

        public TokenRefreshException()
        {
        }

        public TokenRefreshException(string message) : base(message)
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