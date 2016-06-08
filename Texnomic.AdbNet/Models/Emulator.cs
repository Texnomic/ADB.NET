using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Texnomic.AdbNet.Protocol;

namespace Texnomic.AdbNet.Models
{
    public class Emulator
    {
        public IPEndPoint EndPoint { get; private set; }

        private uint LocalID { get; set; }
        private Shell Shell { get; set; }
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
        public async Task<string> ExcuteShell(string Command)
        {
            return await Shell.ExcuteShell(Command);
        }
        public async Task<string> GetUITree()
        {
            return await Shell.ExcuteShell("uiautomator dump /dev/tty\r");
        }
        public void Cleanup()
        {
            Destroy();
        }

        private void Intialize()
        {
            Client = new TcpClient();
            Client.Connect(EndPoint);
            Client.ReceiveTimeout = 5;
            Client.SendTimeout = 5;
            Client.ReceiveBufferSize = 4096;
            Client.LingerState = new LingerOption(false, 5);
            Stream = Client.GetStream();
            Reader = new StreamReader(Stream);
            Writer = new StreamWriter(Stream);
            Shell = new Shell(Stream, Reader, Systems.Host, LocalID);
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
