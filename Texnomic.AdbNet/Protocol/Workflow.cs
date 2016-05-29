#define DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Texnomic.AdbNet.Models;

#if DEBUG
#warning WriteWorkflow is hardcoded to Shell Ending.
#endif

namespace Texnomic.AdbNet.Protocol
{
    internal class Workflow
    {
        internal async Task<Message> REPLWorkflow(NetworkStream Stream, StreamReader Reader, uint LocalID, uint RemoteID, string Payload)
        {
            Message ClientReady = await WriteWorkflow(Stream, LocalID, RemoteID, Payload);

            string ConcatenatedPayload = "";

            while (true)
            {
                Message ClientWrite = await Recieve(Stream, Reader);
                await ReadyWorkflow(Stream, LocalID, RemoteID);
                ConcatenatedPayload += ClientWrite.Payload;
                if (ClientWrite.Payload.EndsWith("# "))
                {
                    ClientWrite.SetPayload(ConcatenatedPayload);
                    return ClientWrite;
                }
            }
        }
        internal async Task<Message> WriteWorkflow(NetworkStream Stream, uint LocalID, uint RemoteID, string Payload)
        {
            WriteMessage WriteMessage = new WriteMessage(LocalID, RemoteID, Payload);
            await Send(Stream, WriteMessage);
            Message ClientReady = await Recieve(Stream);
            return ClientReady;
        }
        internal async Task ReadyWorkflow(NetworkStream Stream, uint LocalID, uint RemoteID)
        {
            ReadyMessage ReadyMessage = new ReadyMessage(LocalID, RemoteID);
            await Send(Stream, ReadyMessage);
        }
        internal async Task<Message> OpenWorkflow(NetworkStream Stream, uint LocalID, string Destination)
        {
            OpenMessage ServerOpenMessage = new OpenMessage(LocalID, Destination);
            await Send(Stream, ServerOpenMessage);
            Message ClientOpenMessage = await Recieve(Stream);
            return await Recieve(Stream);
        }
        internal async Task<Message> ConnectWorkflow(NetworkStream Stream, Systems System, string Serial = Constants.Serial, string Banner = Constants.Banner)
        {
            ConnectMessage ConnectMessage = new ConnectMessage(Systems.Host, Serial, Banner);
            await Send(Stream, ConnectMessage);
            return await Recieve(Stream);
        }

        private async Task Send(NetworkStream Stream, Message Message)
        {
            byte[] RawMessage = Message.GetPacket();
            await Stream.WriteAsync(RawMessage, 0, RawMessage.Length);
            await Stream.FlushAsync();
        }
        private async Task<Message> Recieve(NetworkStream Stream)
        {
            Message Message = new Message();
            byte[] Header = await Read(Stream, 24);

            Message.SetCommand(Header.Take(4).ToArray());
            Message.SetArgument1(Header.Skip(4).Take(4).ToArray());
            Message.SetArgument2(Header.Skip(8).Take(4).ToArray());

            if (Message.Command == Commands.Okay)
            {
                Message.SetPayload("");
                return Message;
            }

            int Length = BitConverter.ToInt32(Header.Skip(12).Take(4).ToArray(), 0);
            byte[] Payload = await Read(Stream, Length);
            Message.SetPayload(Payload);

            uint FakeCRC32 = BitConverter.ToUInt32(Header.Skip(16).Take(4).ToArray(), 0);

            if (FakeCRC32 != Message.FakeCRC32)
            {
               throw new InvalidMessageCRC32Exception();
            }

            return Message;
        }
        private async Task<Message> Recieve(NetworkStream Stream, StreamReader Reader)
        {
            Message Message = new Message();
            byte[] Header = await Read(Stream, 24);

            Message.SetCommand(Header.Take(4).ToArray());
            Message.SetArgument1(Header.Skip(4).Take(4).ToArray());
            Message.SetArgument2(Header.Skip(8).Take(4).ToArray());

            if(Message.Command == Commands.Okay)
            {
                Message.SetPayload("");
                return Message;
            }

            int Length = BitConverter.ToInt32(Header.Skip(12).Take(4).ToArray(), 0);
            char[] Payload = await Read(Reader, Length);
            Message.SetPayload(Payload);

            uint FakeCRC32 = BitConverter.ToUInt32(Header.Skip(16).Take(4).ToArray(), 0);

            if (FakeCRC32 != Message.FakeCRC32)
            {
                throw new InvalidMessageCRC32Exception();
            }

            return Message;
        }
        private async Task<byte[]> Read(NetworkStream Stream, int Lenght)
        {
            byte[] Payload = new byte[Lenght];

            int Retries = 0;
            int Recieved = 0;
            int Index = 0;

            while (Retries <= 3)
            {
                Recieved += await Stream.ReadAsync(Payload, Index, Payload.Length - Recieved);
                if (Recieved == Lenght) break;
                Index = Recieved - 1;
                Retries++;
            }

            return Payload;
        }
        private async Task<char[]> Read(StreamReader Reader, int Lenght)
        {
            char[] Payload = new char[Lenght];

            int Retries = 0;
            int Recieved = 0;
            int Index = 0;

            while (Retries <= 3)
            {
                Recieved += await Reader.ReadBlockAsync(Payload, Index, Payload.Length - Recieved);
                if (Recieved == Lenght) break;
                Index = Recieved - 1;
                Retries++;
            }

            return Payload;
        }
    }
}

