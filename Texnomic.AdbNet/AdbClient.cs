using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Texnomic.AdbNet.Protocol;
using Texnomic.AdbNet.Models;
using System.Net.NetworkInformation;

namespace Texnomic.AdbNet
{
    public class AdbClient
    {
        private const string Localhost = "127.0.0.1";
        private const int Port = 5037;
        private const uint LocalID = 1234;
        private IPGlobalProperties IPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        private List<Task> BackgroundThreads = new List<Task>();
        private List<Emulator> Emulators = new List<Emulator>();

        //public AdbClient()
        //{
        //    BackgroundThreads.Add(Task.Run(() => Server()));
        //}
        //private async Task Server()
        //{
        //    TcpListener Listener = new TcpListener(IPAddress.Parse(Localhost), Port);
        //    Listener.Start();

        //    while (true)
        //    {
        //        TcpClient Client = await Listener.AcceptTcpClientAsync();
        //        List<IPEndPoint> NewEmulators = Scan();
        //        NewEmulators.ForEach(NEP => Emulators.Add(new Emulator(NEP, LocalID)));
        //    }
        //}

        public List<Emulator> GetEmulators()
        {
            return CreateEmulators();
        }

        private List<Emulator> CreateEmulators()
        {
            var Test = IPGlobalProperties.GetActiveTcpListeners();
            List<IPEndPoint> EndPoints = GetEndPoints();
            foreach (IPEndPoint EndPoint in EndPoints)
            {
                Emulators.Add(new Emulator(EndPoint, LocalID));
            }

            return Emulators;
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
    }

}
