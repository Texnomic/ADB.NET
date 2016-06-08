﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Texnomic.AdbNet.Protocol
{
    class Base
    {
        internal async Task SendMessage(NetworkStream Stream, Message Message)
        {
            byte[] RawMessage = Message.GetPacket();
            await Stream.WriteAsync(RawMessage, 0, RawMessage.Length);
            await Stream.FlushAsync();
        }
        internal async Task<Message> RecieveMessageWithPayload(NetworkStream Stream, StreamReader Reader)
        {
            Message Message = new Message();

            Message.SetCommand(await Recieve(Stream, 4));
            Message.SetArgument1(await Recieve(Stream, 4));
            Message.SetArgument2(await Recieve(Stream, 4));

            int Length = BitConverter.ToInt32(await Recieve(Stream, 4), 0);
            uint FakeCRC32 = BitConverter.ToUInt32(await Recieve(Stream, 4), 0);
            byte[] Magic = await Recieve(Stream, 4);


            if (Message.Command == Commands.Okay || Message.Command == Commands.Close)
            {
                Message.SetPayload("");
                return Message;
            }

            char[] Payload = await Recieve(Reader, Length);
            Message.SetPayload(Payload);

            if (FakeCRC32 != Message.FakeCRC32)
            {
                throw new InvalidMessageCRC32Exception();
            }

            return Message;
        }
        internal async Task<Message> RecieveMessageWithoutPayload(NetworkStream Stream)
        {
            Message Message = new Message();

            Message.SetCommand(await Recieve(Stream, 4));
            Message.SetArgument1(await Recieve(Stream, 4));
            Message.SetArgument2(await Recieve(Stream, 4));

            int Length = BitConverter.ToInt32(await Recieve(Stream, 4), 0);
            uint FakeCRC32 = BitConverter.ToUInt32(await Recieve(Stream, 4), 0);
            byte[] Magic = await Recieve(Stream, 4);

            return Message;
        }
        internal async Task<Message> RecieveStreamingMessage(NetworkStream Stream, StreamReader Reader, OkayMessage OkayMessage)
        {
            Message Message = new Message();

            Message.SetCommand(await Recieve(Stream, 4));
            Message.SetArgument1(await Recieve(Stream, 4));
            Message.SetArgument2(await Recieve(Stream, 4));

            int Length = BitConverter.ToInt32(await Recieve(Stream, 4), 0);
            uint FakeCRC32 = BitConverter.ToUInt32(await Recieve(Stream, 4), 0);
            byte[] Magic = await Recieve(Stream, 4);


            if (Message.Command == Commands.Okay)
            {
                Message.SetPayload("");
                return Message;
            }

            char[] Payload = await RecieveStreaming(Stream, Reader, OkayMessage);
            Message.SetPayload(Payload);

            //if (FakeCRC32 != Message.FakeCRC32)
            //{
            //    throw new InvalidMessageCRC32Exception();
            //}

            return Message;
        }
        private async Task<byte[]> Recieve(NetworkStream Stream, int Lenght)
        {
            byte[] Payload = new byte[Lenght];
            await Stream.ReadAsync(Payload, 0, Payload.Length);
            return Payload;
        }
        private async Task<char[]> Recieve(StreamReader Reader, int Lenght)
        {
            char[] Payload = new char[Lenght];
            await Reader.ReadBlockAsync(Payload, 0, Payload.Length);
            return Payload;
        }
        private async Task<char[]> RecieveStreaming(NetworkStream Stream, StreamReader Reader, OkayMessage OkayMessage)
        {
            List<char> Payload = new List<char>();
            char[] Buffer = new char[1024];
            byte[] Okay = OkayMessage.GetPacket();
            int Recieved = 0;

            while (Stream.DataAvailable)
            {
                Recieved = await Reader.ReadAsync(Buffer, 0, Buffer.Length);
                await Stream.WriteAsync(Okay, 0, Okay.Length);
                await Stream.FlushAsync();
                Payload.AddRange(Buffer.Take(Recieved));
                Buffer = new char[1024];
            }

            return Payload.ToArray();
        }
    }
}