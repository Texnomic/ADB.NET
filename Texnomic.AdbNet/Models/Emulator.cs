using System;
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
        public IPEndPoint EndPoint { get; }
        public Shell Shell { get; }
        public Sync Sync { get; }
        public Root Root { get; }
        public Install Install { get; }

        public Emulator(IPEndPoint EndPoint)
        {
            Shell = new Shell(EndPoint);
            Sync = new Sync(EndPoint);
            Root = new Root(EndPoint);
            Install = new Install(EndPoint);
            this.EndPoint = EndPoint;
        }

    }
}
