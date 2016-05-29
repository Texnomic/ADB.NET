using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Texnomic.AdbNet.Models;
using Texnomic.AdbNet.Protocol;

namespace Texnomic.AdbNet
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

        private void Intialize()
        {
            Client = new TcpClient();
            Client.Connect(EndPoint);
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
        public async Task<string> ExcuteShell(string Command)
        {
            return await Shell.ExcuteShell(Command);
        }
    }
}
