using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PokemonGoGUI.Exceptions
{
    [Serializable]
    public class CaptchaException : Exception       
    {
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected virtual new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            GetObjectData(info, context);
        }

        protected CaptchaException(SerializationInfo info, StreamingContext context)
               : base(info, context)
        {
            //not implanted
        }

        public string Url { get; set; }

        public CaptchaException(string url)
        {
            Url = url;
        }
    }
}
