using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsynHttpClient.Tcp;

namespace AsynHttpClient.Http
{

    public class HttpClient
    {
        public event Action<HttpFetchResult> Completed;
        public int Count { get; set; }
        public void Fetch(string url)
        {
            Count += 1;
            if (string.IsNullOrEmpty(url)) return;
            TcpClientConnection<HttpConnectionState> conn = HttpConnectionPool.CheckOut(url);
            conn.Error += conn_Error;
            conn.Connected += conn_Connected;
            conn.DataReceived += conn_DataReceived;
            conn.Connect();
        }

        private void TriggerCompleted(HttpFetchResult result)
        {
            try
            {
                Action<HttpFetchResult> temp = Completed;
                if (temp != null)
                {
                    temp(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("TriggerCompleted Error:{0} {1}", result.Url, ex);
            }
        }

        private void conn_Error(TcpClientConnection<HttpConnectionState> conn, HttpConnectionState state, Exception error)
        {
            TriggerCompleted(new HttpFetchResult(state.Url, error: error));            
        }

        private void conn_Connected(TcpClientConnection<HttpConnectionState> conn, HttpConnectionState state)
        {

            HttpRequest request = state.Request;
            conn.SendData(request.GetBytes());//info:noexception
        }

        private void conn_DataReceived(TcpClientConnection<HttpConnectionState> conn, HttpConnectionState state, BufferData data)
        {
            HttpResponseParser parser = state.ResponseParser;
            string response = parser.Parse(data);//info:noexception
            if (response == null) return;
            conn.Close();//info:noexception
            TriggerCompleted(new HttpFetchResult(state.Url, response: response));
        }

    }
}
