#region using directives

using System;

#endregion

namespace PokemonGoGUI.Exceptions
{
    
    public class SessionInvalidatedException : Exception
    {
        public SessionInvalidatedException()
        {
        }

        public SessionInvalidatedException(string message) : base(message)
        {
        }
    }
}