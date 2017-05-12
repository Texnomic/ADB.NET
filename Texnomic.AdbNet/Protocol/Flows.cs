using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Texnomic.AdbNet.Models;


namespace Texnomic.AdbNet.Protocol
{
    public class Flows
    {
        private Core Core = new Core();

        public async Task<Message> RecievePayloadFlow(NetworkStream Stream, StreamReader Reader, uint LocalID, uint RemoteID, bool EnableFakeCRC32 = true)
        {
            Message Message = await Core.RecieveMessageWithPayload(Stream, Reader, EnableFakeCRC32);
            await OkayFlow(Stream, LocalID, RemoteID);
            return Message;
        }
        public async Task<Message> WriteFlow(NetworkStream Stream, uint LocalID, uint RemoteID, string Payload)
        {
            WriteMessage WriteMessage = new WriteMessage(LocalID, RemoteID, Payload);
            await Core.SendMessage(Stream, WriteMessage);
            Message ClientOkay = await Core.RecieveMessageWithoutPayload(Stream);
            return ClientOkay;
        }
        public async Task OkayFlow(NetworkStream Stream, uint LocalID, uint RemoteID)
        {
            OkayMessage ReadyMessage = new OkayMessage(LocalID, RemoteID);
            await Core.SendMessage(Stream, ReadyMessage);
        }
        public async Task<Message> OpenFlow(NetworkStream Stream, StreamReader Reader, uint LocalID, string Destination)
        {
            OpenMessage ServerOpenMessage = new OpenMessage(LocalID, Destination);
            await Core.SendMessage(Stream, ServerOpenMessage);
            return await Core.RecieveMessageWithoutPayload(Stream);
        }
        public async Task<Message> CloseFlow(NetworkStream Stream, uint LocalID, uint RemoteID)
        {
            CloseMessage ServerCloseMessage = new CloseMessage(LocalID, RemoteID);
            await Core.SendMessage(Stream, ServerCloseMessage);
            Message ClientCloseMessage = await Core.RecieveMessageWithoutPayload(Stream);
            return ClientCloseMessage;
        }
        public async Task<Message> ConnectFlow(NetworkStream Stream, StreamReader Reader, Systems System, string Serial = Constants.Serial, string Banner = Constants.Banner)
        {
            ConnectMessage ConnectMessage = new ConnectMessage(Systems.Host, Serial, Banner);
            await Core.SendMessage(Stream, ConnectMessage);
            return await Core.RecieveMessageWithPayload(Stream, Reader);
        }
    }
}

