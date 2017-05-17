using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texnomic.AdbNet.Protocol
{
    public class Message
    {
        public Message() { }

        public Command? Command { get; set; }
        public uint Argument1 { get; set; }
        public uint Argument2 { get; set; }
        public int PayloadLength { get; set; }
        public uint FakeCRC32 { get; set; }
        public uint Magic { get; set; }
        public byte[] Payload { get; set; }


        public byte[] GetPacket()
        {
            List<byte> Message = new List<byte>();
            Message.AddRange(BitConverter.GetBytes((uint)Command));
            Message.AddRange(BitConverter.GetBytes(Argument1));
            Message.AddRange(BitConverter.GetBytes(Argument2));
            Message.AddRange(BitConverter.GetBytes(PayloadLength));
            Message.AddRange(BitConverter.GetBytes(FakeCRC32));
            Message.AddRange(BitConverter.GetBytes(Magic));
            Message.AddRange(Payload);
            return Message.ToArray();
        }

        internal uint GenerateFakeCRC32(string Payload)
        {
            return GenerateFakeCRC32(Encoding.ASCII.GetBytes(Payload));
        }
        internal uint GenerateFakeCRC32(byte[] Payload)
        {
            uint CRC = 0x0;
            for (var i = 0; i < Payload.Length; i++) CRC = (CRC + Payload[i]) & 0xFFFFFFFF;
            return CRC;
        }
    }
    public class OkayMessage : Message
    {
        public OkayMessage() : base() { }

        public OkayMessage(uint LocalID, uint RemoteID)
        {
            Command = Protocol.Command.OKAY;
            Argument1 = LocalID;
            Argument2 = RemoteID;
            Payload = new byte[0];
            PayloadLength = 0;
            FakeCRC32 = 0x0;
            Magic = (uint)Command ^ 0xffffffff;
        }
    }
    public class OpenMessage : Message
    {
        public OpenMessage() : base() { }

        public OpenMessage(uint LocalID, string Destination)
        {
            Command = Protocol.Command.OPEN;
            Argument1 = LocalID;
            Argument2 = 0x0;
            Payload = Encoding.ASCII.GetBytes(Destination);
            PayloadLength = Payload.Length;
            FakeCRC32 = GenerateFakeCRC32(Payload);
            Magic = (uint)Command ^ 0xffffffff;
        }
    }
    public class ConnectMessage : Message
    {
        public ConnectMessage(Systems System, string Serial = Constants.Serial, string Banner = Constants.Banner)
        {
            Command = Protocol.Command.CNXN;
            Argument1 = Constants.Version; //Version
            Argument2 = Constants.MaxData; //Max Data
            Payload = Encoding.ASCII.GetBytes($"{System.ToString()}:{Serial}:{Banner}"); //System Identity
            PayloadLength = Payload.Length;
            FakeCRC32 = GenerateFakeCRC32(Payload);
            Magic = (uint)Command ^ 0xffffffff;
        }
    }
    public class WriteMessage : Message
    {
        public WriteMessage() { }

        public WriteMessage(uint LocalID, uint RemoteID, string Payload)
        {
            Command = Protocol.Command.WRTE;
            Argument1 = LocalID;
            Argument2 = RemoteID;
            this.Payload = Encoding.ASCII.GetBytes(Payload);
            PayloadLength = Payload.Length;
            FakeCRC32 = GenerateFakeCRC32(this.Payload);
            Magic = (uint)Command ^ 0xffffffff;
        }
        public WriteMessage(uint LocalID, uint RemoteID, byte[] Payload)
        {
            Command = Protocol.Command.WRTE;
            Argument1 = LocalID;
            Argument2 = RemoteID;
            this.Payload = Payload;
            PayloadLength = Payload.Length;
            FakeCRC32 = GenerateFakeCRC32(this.Payload);
            Magic = (uint)Command ^ 0xffffffff;
        }
    }
    public class CloseMessage : Message
    {
        public CloseMessage() : base() { }

        public CloseMessage(uint LocalID, uint RemoteID)
        {
            Command = Protocol.Command.CLSE;
            Argument1 = LocalID;
            Argument2 = RemoteID;
            Payload = new byte[0];
            PayloadLength = 0;
            FakeCRC32 = 0x0;
            Magic = (uint)Command ^ 0xffffffff;
        }
    }

    public class SyncMessage
    {
        public WriteMessage Message { get; }
        public string ID { get; }
        public int Parameter { get; }

        public SyncMessage(WriteMessage Message)
        {
            this.Message = Message;
            ID = Encoding.UTF8.GetString(Message.Payload.Take(4).ToArray());
            Parameter = GetLittleEndianIntegerFromByteArray(Message.Payload.Skip(4).Take(4).ToArray(), 0);
        }

        public static int GetLittleEndianIntegerFromByteArray(byte[] data, int startIndex)
        {
            return (data[startIndex + 3] << 24)
                 | (data[startIndex + 2] << 16)
                 | (data[startIndex + 1] << 8)
                 | data[startIndex];
        }
        public static byte[] GetLittleEndianByteArrayFromInteger(int data)
        {
            byte[] b = new byte[4];
            b[0] = (byte)data;
            b[1] = (byte)(((uint)data >> 8) & 0xFF);
            b[2] = (byte)(((uint)data >> 16) & 0xFF);
            b[3] = (byte)(((uint)data >> 24) & 0xFF);
            return b;
        }
    }

    public class StatSyncMessage : SyncMessage
    {
        public string Payload { get; }

        public StatSyncMessage(WriteMessage Message) : base(Message)
        {
            Payload = Encoding.UTF8.GetString(Message.Payload.Skip(8).Take(Parameter).ToArray());
        }

        public StatSyncMessage(uint LocalID, uint RemoteID, string FilePath)
            : base(new WriteMessage(LocalID, RemoteID,
                Encoding.UTF8.GetBytes("STAT")
                .Concat(GetLittleEndianByteArrayFromInteger(FilePath.Length))
                .Concat(Encoding.UTF8.GetBytes(FilePath)).ToArray()))
        {
            Payload = Encoding.UTF8.GetString
                (
                Encoding.UTF8.GetBytes("STAT")
                .Concat(GetLittleEndianByteArrayFromInteger(FilePath.Length))
                .Concat(Encoding.UTF8.GetBytes(FilePath)).ToArray()
                );
        }
    }

    public class RecvSyncMessage : SyncMessage
    {
        public string Payload { get; }

        public RecvSyncMessage(WriteMessage Message) : base(Message)
        {
            Payload = Encoding.UTF8.GetString(Message.Payload.Skip(8).Take(Parameter).ToArray());
        }

        public RecvSyncMessage(uint LocalID, uint RemoteID, string FilePath)
            : base(new WriteMessage(LocalID, RemoteID,
                Encoding.UTF8.GetBytes("RECV")
                .Concat(GetLittleEndianByteArrayFromInteger(FilePath.Length))
                .Concat(Encoding.UTF8.GetBytes(FilePath)).ToArray()))
        {
            Payload = Encoding.UTF8.GetString
                (
                Encoding.UTF8.GetBytes("RECV")
                .Concat(GetLittleEndianByteArrayFromInteger(FilePath.Length))
                .Concat(Encoding.UTF8.GetBytes(FilePath)).ToArray()
                );
        }
    }

    public class DataSyncMessage : SyncMessage
    {
        public byte[] Payload { get; }
        public bool EndOfFile { get; }

        public DataSyncMessage(WriteMessage Message) : base(Message)
        {
            Payload = Message.Payload.Skip(8).Take(Parameter).ToArray();

            if (Message.PayloadLength - Parameter == 8)
            {
                string EOF = Encoding.UTF8.GetString(Message.Payload.Skip(Message.Payload.Length - 8).Take(4).ToArray());

                if (EOF == "DONE") EndOfFile = true;
            }
        }
    }
}
