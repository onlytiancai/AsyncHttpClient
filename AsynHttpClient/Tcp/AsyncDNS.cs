using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace AsynHttpClient.Tcp
{
    public class AsyncDNS
    {
        //info:canthrow
        public static void GetAddress(string host, Action<IPAddress, Exception> completed)
        {
            if (completed == null) throw new ArgumentNullException("completed");
            Dns.BeginGetHostAddresses(host, GetHostAddressesCallback, completed);
        }
        private static void GetHostAddressesCallback(IAsyncResult result)
        {
            Action<IPAddress, Exception> completed = (Action<IPAddress, Exception>)result.AsyncState;
            try
            {
                IPAddress[] adds = Dns.EndGetHostAddresses(result);
                completed(adds[0], null);
            }
            catch (Exception ex)
            {
                completed(null, ex);
            }
        }
    }
}
