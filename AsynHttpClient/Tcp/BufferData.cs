using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace AsynHttpClient.Tcp
{
    public class BufferData
    {
        public BufferData(SocketAsyncEventArgs args)
        {
            Buffer = args.Buffer;
            Offset = args.Offset;
            Length = args.BytesTransferred;
        }
        public byte[] Buffer { get; private set; }
        public int Offset { get; private set; }
        public int Length { get; private set; }
    }
}
