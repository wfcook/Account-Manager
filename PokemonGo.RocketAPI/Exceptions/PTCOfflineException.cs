using System;

namespace PokemonGo.RocketAPI.Exceptions
{
    public class PtcOfflineException : Exception
    {
        public PtcOfflineException() : base("The Ptc server is offline. Please try again later")
        {

        }
    }
}