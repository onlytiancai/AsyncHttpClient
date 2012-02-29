using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;

namespace AsynHttpClient.Tcp
{
    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="">所有的方法都不能抛出异常，错误从Error抛出</exception>
    /// <typeparam name="T"></typeparam>
    public class TcpClientConnection<T>
    {

        public event Action<TcpClientConnection<T>, T, BufferData> DataReceived;
        public event Action<TcpClientConnection<T>, T, Exception> Error;
        public event Action<TcpClientConnection<T>, T> Connected;
        public event Action<TcpClientConnection<T>, T> RemoteDisconnected;
        public bool IsConnected { get; private set; }

        public TcpClientConnection(string host, int port, T state)
        {
            if (string.IsNullOrEmpty(host)) throw new ArgumentException("host");
            if (port < 0) throw new ArgumentOutOfRangeException("port");
            _host = host;
            _port = port;
            _state = state;
            InitSocket();
        }

        public void Connect()
        {
            try
            {
                AsyncDNS.GetAddress(_host, GetRemoteAddressCompleted);
            }
            catch (Exception ex)
            {
                TriggerError(new ApplicationException("GetRemoteAddress error", ex));
            }
        }

        public void SendData(byte[] data)
        {
            if (!IsConnected)
            {
                TriggerError(new ApplicationException("Not Connected"));
                return;
            }
            try
            {
                if (_pendingSend)
                {
                    _sendBuffers.Add(data);
                    return;
                }
                SendDataIfNoPending(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                TriggerError(new ApplicationException("SendData error", ex));
            }
        }

        public void Close()
        {
            try
            {
                IsConnected = false;
                _connArgs.Dispose();
                _sendArgs.Dispose();
                _receiveArgs.Dispose();
                UnSubscribeArgsEvent();
                if (_client != null)
                {
                    _client.Close();
                }
            }
            catch (Exception ex)
            {
                TriggerError(new ApplicationException("Close error", ex));
            }
        }

        private void TriggerDataReceived(BufferData data)
        {
            try
            {
                Action<TcpClientConnection<T>, T, BufferData> temp = DataReceived;
                if (temp != null)
                {
                    temp(this, _state, data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("TriggerDataReceived Error:{0} {1}", _state, ex);
            }
        }

        private void TriggerConnected()
        {
            try
            {
                Action<TcpClientConnection<T>, T> temp = Connected;
                if (temp != null)
                {
                    temp(this, _state);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("TriggerConnected Error:{0} {1}", _state, ex);
            }

        }

        private void TriggerRemoteDisconnected()
        {
            try
            {
                Action<TcpClientConnection<T>, T> temp = RemoteDisconnected;
                if (temp != null)
                {
                    temp(this, _state);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("TriggerRemoteDisconnected Error:{0} {1}", _state, ex);
            }

        }

        private void TriggerError(Exception error)
        {
            try
            {
                UnSubscribeArgsEvent();
                Action<TcpClientConnection<T>, T, Exception> temp = Error;
                if (temp != null)
                {
                    temp(this, _state, error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("TriggerError Error:{0} {1}", _state, ex);
            }
        }

        private void GetRemoteAddressCompleted(IPAddress addr, Exception ex)
        {
            if (ex != null)
            {
                TriggerError(ex);
                return;
            }
            try
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(addr, _port);
                _connArgs.RemoteEndPoint = remoteEndPoint;
                _connArgs.Completed += ConnectCompleted;
                _client.ConnectAsync(_connArgs);
            }
            catch (Exception connectErr)
            {
                TriggerError(new ApplicationException("ConnectAsync error", connectErr));
            }
        }

        private void ConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (HasSocketError(e)) return;
            IsConnected = true;
            SubscribeArgsEvent();
            StartReceive();
            TriggerConnected();
            return;
        }

        private void SendComplete(object sender, SocketAsyncEventArgs e)
        {
            if (HasSocketError(e)) return;
            if (CheckRemainingData(e)) return;
            if(CheckSendQueue()) return;
            _pendingSend = false;
        }

        private void ReceiveComplete(object sender, SocketAsyncEventArgs e)
        {
            if (HasSocketError(e)) return;
            if (e.BytesTransferred == 0)
            {
                TriggerRemoteDisconnected();
                return;
            }
            BufferData data = new BufferData(e);
            TriggerDataReceived(data);//info:可能阻塞接受线程
            StartReceive();
        }

        private bool HasSocketError(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success) return false;
            SocketException error = new SocketException((Int32)e.SocketError);
            TriggerError(new ApplicationException("on_connected error", error));
            return true;

        }

        private void InitSocket()
        {
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _connArgs = new SocketAsyncEventArgs();
            _sendArgs = new SocketAsyncEventArgs();
            _receiveArgs = new SocketAsyncEventArgs();
        }

        private void SubscribeArgsEvent()
        {
            _sendArgs.Completed += SendComplete;
            _receiveArgs.Completed += ReceiveComplete;
        }

        private void UnSubscribeArgsEvent()
        {
            _sendArgs.Completed -= SendComplete;
            _receiveArgs.Completed -= ReceiveComplete;
        }

        private void StartReceive()
        {
            if (!IsConnected) return;
            int bufferLength = _receiveBuffer.Length;
            _receiveArgs.SetBuffer(_receiveBuffer, 0, bufferLength);
            if (!_client.ReceiveAsync(_receiveArgs))
            {
                ReceiveComplete(_receiveArgs, _receiveArgs);
            }
        }
        private void SendDataIfNoPending(byte[] data, int offset, int length)
        {
            _pendingSend = true;
            _sendArgs.SetBuffer(data, offset, length);
            _sendArgs.UserToken = data.Length;
            if (!_client.SendAsync(_sendArgs))
            {
                SendComplete(_sendArgs, _sendArgs);
            }
        }
        private bool CheckSendQueue()//info:needreflacter 名字不像返回bool值
        {
            byte[] data = null;
            if (_sendBuffers.TryTake(out data))
            {
                SendDataIfNoPending(data, 0, data.Length);
                return true;
            }
            return false;
        }

        private bool CheckRemainingData(SocketAsyncEventArgs e)//info:needreflacter 名字不像返回bool值
        {
            int sendDataLength = (int)e.UserToken;
            int remainingLength = sendDataLength - e.BytesTransferred;
            if (remainingLength > 0)
            {
                SendDataIfNoPending(e.Buffer, e.BytesTransferred, remainingLength);
                return true;
            }
            return false;
        }
        private string _host;
        private int _port;
        private Socket _client;
        private T _state;
        private SocketAsyncEventArgs _connArgs;
        private SocketAsyncEventArgs _sendArgs;
        private SocketAsyncEventArgs _receiveArgs;
        private byte[] _receiveBuffer = new byte[1024];
        private ConcurrentBag<byte[]> _sendBuffers = new ConcurrentBag<byte[]>();
        private bool _pendingSend = false;
    }
}
