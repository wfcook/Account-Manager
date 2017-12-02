using System;

namespace PokemonGoGUI.Exceptions
{
    public class APIBadRequestException:Exception
    {
        public APIBadRequestException(string message) : base(message)
        {

        }
    }
}
