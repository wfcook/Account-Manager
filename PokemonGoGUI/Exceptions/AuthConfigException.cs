using System;

namespace PokemonGoGUI.Exceptions
{
    [Serializable]
    public class AuthConfigException  : Exception
    {
            public AuthConfigException(string message): base(message)
        {

        }
    }
}
