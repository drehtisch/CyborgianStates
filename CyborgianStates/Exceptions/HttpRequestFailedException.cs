using System;
using System.Runtime.Serialization;

namespace CyborgianStates.Exceptions
{
    public class HttpRequestFailedException : Exception
    {
        public HttpRequestFailedException()
        {
        }

        public HttpRequestFailedException(string message) : base(message)
        {
        }

        public HttpRequestFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected HttpRequestFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}