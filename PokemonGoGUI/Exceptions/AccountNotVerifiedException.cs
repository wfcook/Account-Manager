using System;

namespace PokemonGoGUI.Exceptions
{
    public class AccountNotVerifiedException : Exception
    {
        public AccountNotVerifiedException() : base("Account is not verified")
        {
            
        }
    }
}