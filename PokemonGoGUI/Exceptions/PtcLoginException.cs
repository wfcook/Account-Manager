using System;

namespace PokemonGoGUI.Exceptions
{
    [Serializable]
    public class PtcLoginException  : Exception
    {
        public PtcLoginException(string message) : base(message) { }
    }
}
