using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsynHttpClient.Tcp;

namespace AsynHttpClient.Http
{

    internal class HttpConnectionPool
    {
        public static TcpClientConnection<HttpConnectionState> CheckOut(string url)
        {
            //todo:从池里取出连接
            HttpConnectionState state = BuildConnectionState(url);
            HttpRequest request = state.Request;
            Uri uri = request.RequestUri;
            string host = uri.Host;
            int port = uri.Port;
            return new TcpClientConnection<HttpConnectionState>(host, port, state);
        }
        public static void CheckIn(TcpClientConnection<HttpConnectionState> conn)
        {
            //todo:把链接入池
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
