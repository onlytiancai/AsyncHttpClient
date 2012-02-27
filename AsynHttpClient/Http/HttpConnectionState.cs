using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsynHttpClient.Http
{
    internal class HttpConnectionState
    {
        public HttpConnectionState(HttpRequest request, HttpResponseParser parser)
        {
            Request = request;
            ResponseParser = parser;
        }
        public HttpRequest Request { get; set; }
        public HttpResponseParser ResponseParser { get; set; }
        public string Url { get { return Request.RequestUri.ToString(); } }
        public override string ToString()
        {
            return Request.RequestUri.ToString();
        }
    }
}
