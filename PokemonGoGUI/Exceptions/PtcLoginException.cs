using System;

namespace PokemonGoGUI.Exceptions
{
    public class PtcLoginException  : Exception
    {
        public PtcLoginException(string message) : base(message) { }
    }
}
