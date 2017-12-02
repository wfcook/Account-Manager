#region using directives

using System;

#endregion

namespace PokemonGoGUI.Exceptions
{
    public class GoogleException : Exception
    {
        public GoogleException(string message) : base(message)
        {
        }
    }
}