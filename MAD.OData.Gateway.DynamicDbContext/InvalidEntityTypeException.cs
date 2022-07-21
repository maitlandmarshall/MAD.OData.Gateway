using System;
using System.Runtime.Serialization;

namespace MAD.OData.Gateway.DynamicDbContext
{
    [Serializable]
    internal class InvalidEntityTypeException : Exception
    {
        public InvalidEntityTypeException()
        {
        }

        public InvalidEntityTypeException(string message) : base(message)
        {
        }

        public InvalidEntityTypeException(string message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidEntityTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}