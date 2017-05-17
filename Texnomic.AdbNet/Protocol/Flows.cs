using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Texnomic.AdbNet.Models;


namespace Texnomic.AdbNet.Protocol
{
    class Flows
    {
        private Core Core;

        public Flows(IPEndPoint EndPoint)
        {
            Core = new Core(EndPoint);
        }

        public async Task<T> RecievePayloadFlow<T>(uint LocalID, uint RemoteID, bool SendOkay = true, bool EnableFakeCRC32 = true) where T : WriteMessage, new()
        {
            T Message = await Core.RecieveMessage<T>(EnableFakeCRC32);
            if (SendOkay) await OkayFlow(LocalID, RemoteID);
            return Message;
        }
        public async Task<T> RecieveStreamingPayloadFlow<T>(uint LocalID, uint RemoteID) where T : WriteMessage, new ()
        {
            T Message = await Core.RecieveMessage<T>(false, true, new OkayMessage(LocalID, RemoteID));
            return Message;
        }
        public async Task<T> WriteFlow<T>(uint LocalID, uint RemoteID, string Payload) where T : OkayMessage, new()
        {
            WriteMessage WriteMessage = new WriteMessage(LocalID, RemoteID, Payload);
            return await WriteFlow<T>(WriteMessage);
        }
        public async Task<T> WriteFlow<T>(uint LocalID, uint RemoteID, byte[] Payload) where T : OkayMessage, new()
        {
            WriteMessage WriteMessage = new WriteMessage(LocalID, RemoteID, Payload);
            return await WriteFlow<T>(WriteMessage);
        }
        public async Task<T> WriteFlow<T>(WriteMessage WriteMessage) where T: OkayMessage, new()
        {
            await Core.SendMessage(WriteMessage);
            T ClientOkay = await Core.RecieveMessage<T>();
            return ClientOkay;
        }
        public async Task OkayFlow(uint LocalID, uint RemoteID)
        {
            OkayMessage ReadyMessage = new OkayMessage(LocalID, RemoteID);
            await Core.SendMessage(ReadyMessage);
        }
        public async Task<T> OpenFlow<T>(uint LocalID, string Destination) where T : OkayMessage, new()
        {
            OpenMessage ServerOpenMessage = new OpenMessage(LocalID, Destination);
            await Core.SendMessage(ServerOpenMessage);
            return await Core.RecieveMessage<T>();
        }
        public async Task<T> RecieveCloseFlow<T>(uint LocalID, uint RemoteID) where T : CloseMessage, new()
        {
            T DeamonCloseMessage = await Core.RecieveMessage<T>();
            await Core.SendMessage(new CloseMessage(LocalID, RemoteID));
            Core.Close();
            return DeamonCloseMessage;
        }
        public async Task<T> SendCloseFlow<T>(uint LocalID, uint RemoteID) where T : CloseMessage, new()
        {
            CloseMessage ServerCloseMessage = new CloseMessage(LocalID, RemoteID);
            await Core.SendMessage(ServerCloseMessage);
            T ClientCloseMessage = await Core.RecieveMessage<T>();
            Core.Close();
            return ClientCloseMessage;
        }
        public async Task<T> ConnectFlow<T>(Systems System, string Serial = Constants.Serial, string Banner = Constants.Banner) where T : WriteMessage, new()
        {
            ConnectMessage ConnectMessage = new ConnectMessage(Systems.Host, Serial, Banner);
            await Core.SendMessage(ConnectMessage);
            return await Core.RecieveMessage<T>();
        }
    }
}

