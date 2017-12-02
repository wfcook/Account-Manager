#region using directives

using System;

#endregion

namespace PokemonGoGUI.Exceptions
{
    public class GoogleTwoFactorException : Exception
    {
        public GoogleTwoFactorException(string message) : base(message)
        {
        }
    }
}