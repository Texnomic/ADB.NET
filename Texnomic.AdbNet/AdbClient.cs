using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using Texnomic.AdbNet.Models;

namespace Texnomic.AdbNet
{
    public class AdbClient
    {
        private const string Localhost = "127.0.0.1";
        private const int Port = 5037;
        private const uint LocalID = 1234;
        private IPGlobalProperties IPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        private List<Task> BackgroundTasks = new List<Task>();
        private List<Emulator> Emulators = new List<Emulator>();
        private List<TcpClient> MonitoredClients = new List<TcpClient>();

        public AdbClient()
        {
            BackgroundTasks.Add(Task.Run(() => MonitorServer()));
        }


        public List<Emulator> GetEmulators()
        {
            Scanner();
            return Emulators;
        }

        private void MonitorServer()
        {
            CheckAdb();

            TcpListener Listener = new TcpListener(IPAddress.Parse(Localhost), Port);
            Listener.Start();

            while (true)
            {
                HandleClient(Listener.AcceptTcpClient());

                Scanner();
            }
        }
        private void Scanner()
        {
            List<IPEndPoint> EndPoints = GetEndPoints();

            foreach (Emulator Emulator in Emulators)
            {
                if (EndPoints.Contains(Emulator.EndPoint)) continue;
                Emulator.Cleanup();
                Emulators.Remove(Emulator);
            }

            foreach (IPEndPoint EndPoint in EndPoints)
            {
                if (Emulators.Exists(Emulator => Emulator.EndPoint.Port == EndPoint.Port)) continue;
                Emulators.Add(new Emulator(EndPoint, LocalID));
            }
        }
        private List<IPEndPoint> GetEndPoints()
        {
            return IPGlobalProperties.GetActiveTcpListeners()
                                     .Where(EP => EP.Port >= 5555)
                                     .Where(EP => EP.Port <= 5585)
                                     .Where(EP => EP.Port % 2 == 1)
                                     .Where(EP => EP.Address.ToString() == Localhost)
                                     .ToList();
        }

        private void CheckAdb()
        {
            if(Process.GetProcessesByName("adb").Length > 0)
            {
                KillAdbServer();
            }
        }
        private void KillAdbServer()
        {
            using (TcpClient Client = new TcpClient())
            {
                Client.Connect(Localhost, Port);

                using (StreamWriter Writer = new StreamWriter(Client.GetStream()))
                {
                    Writer.Write("0009host:kill");
                    Writer.Flush();
                    Process.GetProcessesByName("adb")[0].WaitForExit();
                    Console.WriteLine("ADB Server Killed.");
                }
            }
        }

        private void HandleClient(TcpClient Client)
        {
            StreamReader Reader = new StreamReader(Client.GetStream());
            StreamWriter Writer = new StreamWriter(Client.GetStream());

            char[] RawSize = new char[4];
            Reader.Read(RawSize, 0, 4);
            int Size = Convert.ToInt32(string.Concat(RawSize), 16);

            char[] RawMessage = new char[Size];
            Reader.Read(RawMessage, 0, Size);

            string Message = string.Concat(RawMessage);

            if (Message.StartsWith("host:emulator:"))
            {
                Console.WriteLine(Message);
                Client.Close();

            }

            if (Message == "host:track-devices")
            {
                Console.WriteLine(Message);
                Writer.Write("OKAY");
                Writer.Flush();

                Writer.Write("0016emulator-5564\toffline\n");
                Writer.Flush();


                Writer.Write("0015emulator-5564\tdevice\n");
                Writer.Flush();

                MonitoredClients.Add(Client);
            }
        }
    }

}
