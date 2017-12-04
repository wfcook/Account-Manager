#region using directives

using System;

#endregion

namespace PokemonGoGUI.Exceptions
{
    [Serializable]
    public class LoginFailedException : Exception
    {
        public LoginFailedException()
        {
        }

        public LoginFailedException(string message) : base(message)
        {
        }
    }

    [Serializable]
    public class TokenRefreshException : Exception
    {
        public TokenRefreshException()
        {
        }

        public TokenRefreshException(string message) : base(message)
        {
        }
    }

    [Serializable]
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException(string message)
            : base(message)
        {

        }
    }

    [Serializable]
    public class IPBannedException : Exception
    {
        public IPBannedException(string message)
            : base(message)
        {

        }
    }
}