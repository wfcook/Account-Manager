using System;

namespace PokemonGo.RocketAPI.Exceptions
{
    public class AccountNotVerifiedException : Exception
    {
        public AccountNotVerifiedException() : base("Account is not verified")
        {
            
        }
    }
}