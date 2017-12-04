#region using directives

using System;

#endregion

namespace PokemonGoGUI.Exceptions
{
    [Serializable]
    public class GoogleTwoFactorException : Exception
    {
        public GoogleTwoFactorException(string message) : base(message)
        {
        }
    }
}