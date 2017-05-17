using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Texnomic.AdbNet.Protocol;

namespace Texnomic.AdbNet.Models
{
    public class Root
    {
        private Flows Flows;
        private IPEndPoint EndPoint;
        private bool IsRoot;
        private uint LocalID;
        private uint RemoteID;

        public Root(IPEndPoint EndPoint)
        {
            this.EndPoint = EndPoint;
            IsRoot = false;
            LocalID = (uint)(new Random().Next(0, 9999));
        }

        public async Task<string> Enable()
        {
            if (IsRoot) return "adbd is already running as root";

            Flows = new Flows(EndPoint);

            await Flows.ConnectFlow<WriteMessage>(Systems.Host);

            OkayMessage OpenOkay = await Flows.OpenFlow<OkayMessage>(LocalID, $"root:\0");

            RemoteID = OpenOkay.Argument1;

            WriteMessage Result = await Flows.RecievePayloadFlow<WriteMessage>(LocalID, RemoteID, false);

            CloseMessage CloseRequest = await Flows.RecieveCloseFlow<CloseMessage>(LocalID, RemoteID);

            IsRoot = true;

            return Encoding.UTF8.GetString(Result.Payload);
        }
    }
}
