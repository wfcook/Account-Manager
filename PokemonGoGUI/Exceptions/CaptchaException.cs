using System;

namespace PokemonGoGUI.Exceptions
{
    public class CaptchaException : Exception
    {
        public string Url { get; set; }

        public CaptchaException(string url)
        {
            Url = url;
        }
    }
}
