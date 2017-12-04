using System;

namespace PokemonGoGUI.Exceptions
{
    [Serializable]
    public class HasherException :Exception
    {
        public HasherException(string message): base(message)
        {

        }
    }
}
