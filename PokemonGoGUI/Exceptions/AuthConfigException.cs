using System;

namespace PokemonGoGUI.Exceptions
{
    public class AuthConfigException  : Exception
    {
            public AuthConfigException(string message): base(message)
        {

        }
    }
}
