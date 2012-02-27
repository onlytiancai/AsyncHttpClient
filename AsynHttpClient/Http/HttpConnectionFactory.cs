using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsynHttpClient.Tcp;

namespace AsynHttpClient.Http
{

    internal class HttpConnectionFactory
    {
        public static TcpClientConnection<HttpConnectionState> CreateConnection(string url)
        {
            HttpConnectionState state = BuildConnectionState(url);
            HttpRequest request = state.Request;
            Uri uri = request.RequestUri;
            string host = uri.Host;
            int port = uri.Port;
            return new TcpClientConnection<HttpConnectionState>(host, port, state);
        }
        private static HttpConnectionState BuildConnectionState(string url)
        {
            HttpRequest request = new HttpRequest(url);
            HttpResponseParser parser = new HttpResponseParser();
            HttpConnectionState state = new HttpConnectionState(request, parser);
            return state;
        }

    }
}
