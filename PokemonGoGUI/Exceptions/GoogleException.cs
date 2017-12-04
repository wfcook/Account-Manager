#region using directives

using System;

#endregion

namespace PokemonGoGUI.Exceptions
{
    [Serializable]
    public class GoogleException : Exception
    {
        public GoogleException(string message) : base(message)
        {
        }
    }
}