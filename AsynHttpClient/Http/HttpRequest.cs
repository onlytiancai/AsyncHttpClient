using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsynHttpClient.Http
{
    public class HttpRequest
    {
        public HttpRequest(string url)
        {
            RequestUri = new Uri(url);
        }
        public Uri RequestUri { get; private set; }
        public byte[] GetBytes()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("HEAD {0} HTTP/1.0\r\n", RequestUri.PathAndQuery);
            sb.AppendFormat("Accept: */*\r\n");
            sb.AppendFormat("Host: {0}\r\n", RequestUri.Host);
            sb.AppendFormat("\r\n\r\n");
            string requestString = sb.ToString();
            return ASCIIEncoding.ASCII.GetBytes(requestString);
        }
        public override string ToString()
        {
            return this.RequestUri.ToString();
        }
    }
}
