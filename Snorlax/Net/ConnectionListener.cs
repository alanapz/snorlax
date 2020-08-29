using System;
using System.Net;

namespace Snorlax.Net
{
    public class ConnectionListener
    {
        public void OnRequest(string httpMethod, Uri requestUri, string? body)
        {
            
        }

        public void OnResponse(HttpStatusCode statusCode, string body)
        {

        }

        public void OnError(WebExceptionStatus errType, HttpStatusCode? statusCode, string? body)
        {

        }
    }
}
