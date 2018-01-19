using System;

namespace POGOLib.Official.Exceptions
{
    internal class SessionUnknowException : Exception
    {
        public SessionUnknowException()
        {
        }

        public SessionUnknowException(string message) : base(message)
        {
        }

        public SessionUnknowException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}