using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using Shared.App.Utils;

namespace Snorlax.Net
{
    public sealed class RestClient : IDisposable
    {
        private string? requestMethod;

        private string? requestText;

        private readonly JsonSerializerSettings jsonConfig = new JsonSerializerSettings();

        private ConnectionListener? listener;

        private readonly Dictionary<string, string> headers = new Dictionary<string, string>();

        public string? URL { get; set; }

        public RestClient()
        {

        }

        public RestClient(string url)
        {
            this.URL = url;
        }

        public RestClient SetConfig(Action<JsonSerializerSettings> configAction)
        {
            configAction(jsonConfig);
            return this;
        }

        public RestClient SetConnectionListener(ConnectionListener? listener)
        {
            this.listener = listener;
            return this;
        }

        public RestClient SetMethod(string? requestMethod)
        {
            this.requestMethod = requestMethod;
            return this;
        }

        public RestClient SetHeader(string name, string value)
        {
            headers[name] = value;
            return this;
        }

        public RestClient SetCookie(string cookieValue)
        {
            headers["Cookie"] = cookieValue;
            return this;
        }

        public RestClient SetData(object? data)
        {
            this.requestText = JsonConvert.SerializeObject(data);
            return this;
        }

        public void Execute()
        {
            Execute<object>();
        }

        public T? Execute<T>() where T : class
        {
            Require.NonNull(URL, "URL");

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(URL!);
            request.ServerCertificateValidationCallback += RemoteCertificateValidationCallback;
            request.ContentType = "application/json; charset=utf-8";
            request.Method = (requestMethod ?? "GET");
            request.Proxy = null;

            if (request.Method == "GET")
            {
                request.ContentLength = 0;
            }
            
            foreach (KeyValuePair<string, string> header in headers)
            {
                request.Headers[header.Key] = header.Value;
            }
            
            listener?.OnRequest(request.Method, request.RequestUri, requestText);
            
            if (request.Method == "GET" && requestText != null)
            {
                throw new ArgumentException("GET requests cannot have a body");
            }
            
            if (requestText != null)
            {
                byte[] requestData = Encoding.UTF8.GetBytes(requestText);
                request.ContentLength = requestData.Length;
                using (Stream outputStream = request.GetRequestStream())
                {
                    outputStream.Write(requestData, 0, requestData.Length);
                }
            }
            
            try
            {
                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string responseText = reader.ReadToEnd();
                        listener?.OnResponse(response.StatusCode, responseText);
                        return JsonConvert.DeserializeObject<T>(responseText, jsonConfig);
                    }
                }
            }
            catch (WebException e)
            {
                HttpWebResponse response = (HttpWebResponse) e.Response;
                
                if (response == null)
                {
                    listener?.OnError(e.Status, null, null);
                }
                
                if (response != null)
                {
                    string errorMessage = DrainResponse(response);
                    e.Data["Response"] = errorMessage;
                    listener?.OnError(e.Status, response.StatusCode, (string?) e.Data["Response"]);
                    throw new HttpStatusCodeException(response.StatusCode, errorMessage);
                }
                throw;
            }
        }

        private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public void Dispose()
        {
            // Nothing to do here
        }

        private static string DrainResponse(WebResponse response)
        {
            try
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
