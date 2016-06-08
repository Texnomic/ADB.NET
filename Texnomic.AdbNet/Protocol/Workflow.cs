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
    internal class Workflow : Base
    {
        internal async Task<List<Message>> REPLWorkflow(NetworkStream Stream, StreamReader Reader, uint LocalID, uint RemoteID, string Payload)
        {
            Message ClientOkay = await WriteWorkflow(Stream, LocalID, RemoteID, Payload);

            OkayMessage OkayMessage = new OkayMessage(LocalID, RemoteID);
            List<Message> ClientWrites = new List<Message>();

            while (true)
            {
                Message ClientWrite = await RecieveStreamingMessage(Stream, Reader, OkayMessage);

                ClientWrites.Add(ClientWrite);

                await OkayWorkflow(Stream, LocalID, RemoteID);

                if (ClientWrite.Payload.EndsWith("# "))
                {
                    return ClientWrites;
                }
            }
        }
        internal async Task<Message> WriteWorkflow(NetworkStream Stream, uint LocalID, uint RemoteID, string Payload)
        {
            WriteMessage WriteMessage = new WriteMessage(LocalID, RemoteID, Payload);
            await SendMessage(Stream, WriteMessage);
            Message ClientOkay = await RecieveMessageWithoutPayload(Stream);
            return ClientOkay;
        }
        internal async Task OkayWorkflow(NetworkStream Stream, uint LocalID, uint RemoteID)
        {
            OkayMessage ReadyMessage = new OkayMessage(LocalID, RemoteID);
            await SendMessage(Stream, ReadyMessage);
        }
        internal async Task<Message> OpenWorkflow(NetworkStream Stream, StreamReader Reader, uint LocalID, string Destination)
        {
            OpenMessage ServerOpenMessage = new OpenMessage(LocalID, Destination);
            await SendMessage(Stream, ServerOpenMessage);
            Message ClientOpenMessage = await RecieveMessageWithPayload(Stream, Reader);
            return await RecieveMessageWithPayload(Stream, Reader);
        }
        internal async Task<Message> CloseWorkflow(NetworkStream Stream, uint LocalID, uint RemoteID)
        {
            CloseMessage ServerCloseMessage = new CloseMessage(LocalID, RemoteID);
            await SendMessage(Stream, ServerCloseMessage);
            Message ClientCloseMessage = await RecieveMessageWithoutPayload(Stream);
            return ClientCloseMessage;
        }
        internal async Task<Message> ConnectWorkflow(NetworkStream Stream, StreamReader Reader, Systems System, string Serial = Constants.Serial, string Banner = Constants.Banner)
        {
            ConnectMessage ConnectMessage = new ConnectMessage(Systems.Host, Serial, Banner);
            await SendMessage(Stream, ConnectMessage);
            return await RecieveMessageWithPayload(Stream, Reader);
        }
    }
}

