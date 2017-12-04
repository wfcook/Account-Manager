using System;

namespace PokemonGoGUI.Exceptions
{
    [Serializable]
    public class APIBadRequestException:Exception
    {
        public APIBadRequestException(string message) : base(message)
        {

        }
    }
}
