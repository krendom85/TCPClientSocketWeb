namespace SocketTCPClientWeb.EthernetInterface.Buffer
{
    public class SocketBuffer
    {
        public byte[] Buffer { get; set; }
        public long Size { get; set; }

        public SocketBuffer()
        {
            Buffer = new byte[4096];
            Size = 0;
        }
    }
}
