using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PokemonGoGUI.Exceptions
{
    [Serializable]
    public class MinimumClientVersionException : Exception
    {
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected virtual new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            GetObjectData(info, context);
        }

        protected MinimumClientVersionException(SerializationInfo info, StreamingContext context)
               : base(info, context)
        {
            //not implanted
        }

        public Version CurrentApiVersion;
        public Version MinimumClientVersion;
        public MinimumClientVersionException(Version currentApiVersion, Version minimumClientVersion) : base()
        {
            CurrentApiVersion = currentApiVersion;
            MinimumClientVersion = minimumClientVersion;
        }
    }
}
