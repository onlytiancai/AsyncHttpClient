using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsynHttpClient.Http
{
    public class HttpFetchResult
    {
        public HttpFetchResult(string url, string response = null, Exception error = null)
        {
            Url = url;
            Response = response;
            Error = error;
        }
        public string Url { get; set; }
        public string Response { get; set; }
        public Exception Error { get; set; }
    }
}
