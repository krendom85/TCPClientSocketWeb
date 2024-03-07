using System.Net.Sockets;
using SocketTCPClientWeb.EthernetInterface.Buffer;

namespace SocketTCPClientWeb.EthernetInterface.SocketClient
{
    public class SoketClient
    {
        private SocketBuffer BufferRx { get; set; }
        private SocketBuffer BufferTx { get; set; }
        private TcpClient client { get; set; }
        private NetworkStream reader { get; set; }

        public event EventHandler<SocketBuffer> SocketBytesReceivedClientEvent;
        SynchronizationContext ctx;

        public SoketClient()
        {
            BufferRx = new SocketBuffer();
            BufferTx = new SocketBuffer();
            client = null;
            ctx = new SynchronizationContext();
        }

        public bool SocketConnection(string ipAddress, int port)
        {
            try
            {
                if (client == null)
                {
                    client = new TcpClient();
                    var result = client.BeginConnect(ipAddress, port, null, null);
                    result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1000));
                    if (!client.Connected)
                    {
                        return false;
                    }
                    client.EndConnect(result);
                    reader = client.GetStream();
                    reader.BeginRead(BufferRx.Buffer, 0, BufferRx.Buffer.Length, new AsyncCallback(SocketReceiver), client);
                    reader.Flush();
                    return true;
                }
                client.Close();
                client = null;
                return false;
            }catch(Exception ex)
            {
                return false;
            }
        }

        public void SocketClose()
        {
            try
            {
                reader.Close();
                client.Close();
                client = null;
            }
            catch (Exception ex)
            {

            }
      
        }

        public void SocketReceiver(IAsyncResult result)
        {
            try
            {
                if (client != null)
                {
                    var _client = (TcpClient)result.AsyncState;
                    if (reader != null)
                    {
                        reader = _client.GetStream();
                        if (reader.CanRead)
                        {
                            reader.BeginRead(BufferRx.Buffer, 0, BufferRx.Buffer.Length, new AsyncCallback(SocketReceiver), _client);
                            BufferRx.Size = reader.EndRead(result);
                            reader.Flush();
                            ctx.Send(new SendOrPostCallback(SendByteClient), BufferRx);
                        }
                    }
                }
            }catch (Exception ex)
            {

            }
        }

        private void SendByteClient(object o)
        {
            SocketBuffer bufferRx = o as SocketBuffer;
            SocketBytesReceivedClientEvent(this, bufferRx);
        }

        public bool SocketSendBytes(SocketBuffer bufferTx)
        {
            if (reader != null)
            {
                if (reader.CanWrite)
                {
                    reader.Write(bufferTx.Buffer, 0, (int)bufferTx.Size);
                    return true;
                }
                return false;
            }
            return false;
        }

        public void SocketSendString(string bufferTx, int size)
        {
            BufferTx.Buffer = System.Text.Encoding.UTF8.GetBytes(bufferTx);
            BufferTx.Size = size;
            SocketSendBytes(BufferTx);
        }

    }
    
}
