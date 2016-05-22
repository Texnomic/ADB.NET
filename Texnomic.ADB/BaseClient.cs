using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Texnomic.AdbNet
{
    public class BaseClient
    {
        internal bool Connected;
        internal TcpClient Client;
        internal Stream Stream;
        internal StreamReader Reader;
        internal StreamWriter Writer;

        internal async Task Connect()
        {
            CheckADB();
            await IntiateTcp();
            Stream = Client.GetStream();
            Reader = new StreamReader(Client.GetStream());
            Writer = new StreamWriter(Client.GetStream());
            Connected = true;
        }
        internal void Disconnect()
        {
            Reader.Close();
            Writer.Close();
            Stream.Close();
            Client.Close();
            Connected = false;
        }

        private void CheckADB()
        {
            Process[] Processes = Process.GetProcessesByName("adb");
            if (Processes.Length == 0) throw new AdbServerNotRunningException();
        }
        private async Task IntiateTcp()
        {
            Client = new TcpClient();
            await Client.ConnectAsync("127.0.0.1", 5037);
            if (!Client.Connected) throw new ConnectionFailedException();
        }

    }
}
