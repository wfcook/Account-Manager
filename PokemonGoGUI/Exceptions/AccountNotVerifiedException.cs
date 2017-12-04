using System;

namespace PokemonGoGUI.Exceptions
{
    [Serializable]
    public class AccountNotVerifiedException : Exception
    {
        public AccountNotVerifiedException() : base("Account is not verified")
        {
            
        }
    }
}