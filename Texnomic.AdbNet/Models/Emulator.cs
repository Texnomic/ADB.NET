using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Texnomic.AdbNet.Protocol;

namespace Texnomic.AdbNet.Models
{
    public class Emulator
    {
        public IPEndPoint EndPoint { get; private set; }
        public Shell Shell { get; set; }

        private uint LocalID { get; set; }
        private TcpClient Client { get; set; }
        private NetworkStream Stream { get; set; }
        private StreamReader Reader { get; set; }
        private StreamWriter Writer { get; set; }

        public Emulator(IPEndPoint EndPoint, uint LocalID)
        {
            this.EndPoint = EndPoint;
            this.LocalID = LocalID;

            Intialize();
        }

        public void Cleanup()
        {
            Destroy();
        }

        private void Intialize()
        {
            Client = new TcpClient();
            Client.Connect(EndPoint);
            Client.ReceiveTimeout = 30 * 1000;
            Client.SendTimeout = 30 * 1000;
            Client.ReceiveBufferSize = 4096;
            Client.LingerState = new LingerOption(false, 30);
            Stream = Client.GetStream();
            Reader = new StreamReader(Stream, Encoding.UTF8);
            Writer = new StreamWriter(Stream, Encoding.UTF8);
            Shell = new Shell(Stream, Reader, LocalID);
        }
        private void Destroy()
        {
            Stream.Close();
            Client.Close();
            Writer.Close();
            Reader.Close();
        }

    }
}
