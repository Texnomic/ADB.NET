using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Texnomic.AdbNet.Protocol
{
    public class Core
    {
        private TcpClient Client;
        private NetworkStream Stream;

        public Core(IPEndPoint EndPoint)
        {
            Client = new TcpClient();
            Client.Connect(EndPoint);
            Client.ReceiveTimeout = 30 * 1000;
            Client.SendTimeout = 30 * 1000;
            Client.ReceiveBufferSize = 4096;
            Client.LingerState = new LingerOption(false, 30);
            Stream = Client.GetStream();
        }

        public async Task SendMessage(Message Message)
        {
            byte[] RawMessage = Message.GetPacket();
            await Stream.WriteAsync(RawMessage, 0, RawMessage.Length);
            await Stream.FlushAsync();
        }

        public async Task<T> RecieveMessage<T>(bool CheckCRC32 = true, bool StreamingPayload = false, OkayMessage OkayMessage = null) where T: Message, new()
        {
            T Message = await RecieveHeaders<T>();

            if (Message.PayloadLength > 0)
            {
                if (StreamingPayload)
                {
                    Message.Payload = await RecieveStreamingPayload(Message.PayloadLength, OkayMessage);
                }
                else
                {
                    Message.Payload = await RecieveBytes(Message.PayloadLength);

                    if (CheckCRC32)
                    {
                        if (Message.GenerateFakeCRC32(Message.Payload) != Message.FakeCRC32)
                        {
                            throw new InvalidMessageCRC32Exception();
                        }
                    }
                }
            }

            return Message;
        }

        private async Task<byte[]> RecieveStreamingPayload(int Length, OkayMessage OkayMessage)
        {
            List<byte> Data = new List<byte>(Length);
            byte[] Buffer = new byte[Length];
            int Recieved = 0;

            while (true)
            {
                Recieved += await Stream.ReadAsync(Buffer, 0, Buffer.Length);
                await SendMessage(OkayMessage);
                Data.AddRange(Buffer.Take(Recieved));
                if (Recieved == Length) break;
                Buffer = new byte[Length - Recieved];
                await Task.Delay(100);
            }

            return Data.ToArray();
        }

        private async Task<T> RecieveHeaders<T>() where T : Message, new()
        {
            T Message = new T()
            {
                Command = (Command)Enum.Parse(typeof(Command), await RecieveString(4)),
                Argument1 = await RecieveUInt(4),
                Argument2 = await RecieveUInt(4),
                PayloadLength = await RecieveInt(4),
                FakeCRC32 = await RecieveUInt(4),
                Magic = await RecieveUInt(4)
            };

            return Message;
        }
        private async Task<byte[]> RecieveBytes(int Length)
        {
            byte[] Payload = new byte[Length];
            int Read = await Stream.ReadAsync(Payload, 0, Length);
            return Payload;
        }
        private async Task<string> RecieveString(int Length)
        {
            byte[] Payload = await RecieveBytes(Length);
            return Encoding.UTF8.GetString(Payload);
        }
        private async Task<int> RecieveInt(int Length)
        {
            return BitConverter.ToInt32(await RecieveBytes(Length), 0); ;
        }
        private async Task<uint> RecieveUInt(int Length)
        {
            return BitConverter.ToUInt32(await RecieveBytes(Length), 0); ;
        }

        public void Close()
        {
            Stream.Close();
            Client.Close();
        }
    }

}
