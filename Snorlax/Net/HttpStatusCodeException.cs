using System;
using System.Net;

namespace Snorlax.Net
{
    public class HttpStatusCodeException : Exception
    {
        public HttpStatusCodeException(HttpStatusCode statusCode, string message) : base($"{statusCode}: {message}")
        {

        }
    }
}
