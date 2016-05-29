using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texnomic.AdbNet.Protocol
{
    internal class Message
    {
        public Commands? Command { get; private set; }
        public uint Argument1 { get; private set; }
        public uint Argument2 { get; private set; }
        public int PayloadLength { get; private set; }
        public uint FakeCRC32 { get; private set; }
        public uint Magic { get; private set; }
        public string Payload { get; private set; }

        internal byte[] RawCommand { get; set; }
        internal byte[] RawArgument1 { get; set; }
        internal byte[] RawArgument2 { get; set; }
        internal byte[] RawPayloadLength { get; set; }
        internal byte[] RawFakeCRC32 { get; set; }
        internal byte[] RawMagic { get; set; }
        internal byte[] RawPayload { get; set; }

        public void SetCommand(byte[] Command)
        {
            RawCommand = Command;
            SetCommand(BitConverter.ToUInt32(Command, 0));
        }
        public void SetCommand(uint Command)
        {
            string CommandName = Enum.GetName(typeof(Commands), Command);
            if (CommandName == null) { throw new InvalidMessageTypeException(); }
            SetCommand((Commands)Command);
        }
        public void SetCommand(Commands Command)
        {
            RawCommand = BitConverter.GetBytes((uint)Command);
            this.Command = Command;
            Magic = (uint)Command ^ 0xffffffff;
            RawMagic = BitConverter.GetBytes((uint)Magic);
        }
        public void SetArgument1(byte[] Argument1)
        {
            RawArgument1 = Argument1;
            this.Argument1 = BitConverter.ToUInt32(Argument1, 0);
        }
        public void SetArgument1(uint Argument1)
        {
            RawArgument1 = BitConverter.GetBytes(Argument1);
            this.Argument1 = Argument1;
        }
        public void SetArgument2(byte[] Argument2)
        {
            RawArgument2 = Argument2;
            this.Argument2 = BitConverter.ToUInt32(Argument2, 0);
        }
        public void SetArgument2(uint Argument2)
        {
            RawArgument2 = BitConverter.GetBytes(Argument2);
            this.Argument2 = Argument2;
        }
        public void SetPayload(byte[] Payload)
        {
            this.Payload = Encoding.ASCII.GetString(Payload);
            RawPayload = Payload;
            SetPayloadLength();
            SetFakeCRC32();
        }
        public void SetPayload(char[] Payload)
        {
            this.Payload = string.Concat(Payload);
            RawPayload = Encoding.ASCII.GetBytes(Payload);
            SetPayloadLength();
            SetFakeCRC32();
        }
        public void SetPayload(string Payload)
        {
            this.Payload = Payload;
            RawPayload = Encoding.ASCII.GetBytes(Payload);
            SetPayloadLength();
            SetFakeCRC32();
        }
        public byte[] GetPacket()
        {
            DoChecks();
            List<byte> Message = new List<byte>();
            Message.AddRange(RawCommand);
            Message.AddRange(RawArgument1);
            Message.AddRange(RawArgument2);
            Message.AddRange(RawPayloadLength);
            Message.AddRange(RawFakeCRC32);
            Message.AddRange(RawMagic);
            Message.AddRange(RawPayload);

            return Message.ToArray();
        }

        private void SetPayloadLength()
        {
            PayloadLength = Payload.Length;
            RawPayloadLength = BitConverter.GetBytes(RawPayload.Length);
        }
        private void SetFakeCRC32()
        {
            if (Payload == null || Payload == "")
            {
                FakeCRC32 = 0x0;
                RawFakeCRC32 = BitConverter.GetBytes(FakeCRC32);
            }
            else
            {
                FakeCRC32 = GenerateFakeCRC32(RawPayload);
                RawFakeCRC32 = BitConverter.GetBytes(FakeCRC32);
            }
        }
        private void DoChecks()
        {
            if (Command == null) throw new MessageException("Command Not Set.");
            if (RawArgument1 == null) throw new MessageException("Argument 1 Not Set.");
            if (RawArgument2 == null) throw new MessageException("Argument 2 Not Set.");
            if (RawPayloadLength == null) throw new MessageException("Message Length Not Set.", new MessageException("Message Not Set."));
            if (RawFakeCRC32 == null) throw new MessageException("Fake CRC32 Not Set.", new MessageException("Message Not Set."));
            if (RawMagic == null) throw new MessageException("Magic Not Set.", new MessageException("Command Not Set."));
            if (RawPayload == null) throw new MessageException("Message Not Set.");
        }
        private uint GenerateFakeCRC32(byte[] Payload)
        {
            uint CRC = 0x0;
            for (var i = 0; i < Payload.Length; i++) CRC = (CRC + Payload[i]) & 0xFFFFFFFF;
            return CRC;
        }
    }
    internal class ReadyMessage : Message
    {
        public ReadyMessage(uint LocalID, uint RemoteID)
        {
            SetCommand(Commands.Okay);
            SetArgument1(LocalID);
            SetArgument2(RemoteID);
            SetPayload("");
        }
    }
    internal class OpenMessage : Message
    {
        public OpenMessage(uint LocalID, string Destination)
        {
            SetCommand(Commands.Open);
            SetArgument1(LocalID);
            SetArgument2(0x0);
            SetPayload(Destination);
        }
    }
    internal class ConnectMessage : Message
    {
        public ConnectMessage(Systems System, string Serial = Constants.Serial, string Banner = Constants.Banner)
        {
            SetCommand(Commands.Connect);
            SetArgument1(Constants.Version); //Version
            SetArgument2(Constants.MaxData); //Max Data
            string SystemIdentity = $"{Enum.GetName(typeof(Systems), System)}:{Serial}:{Banner}";
            SetPayload(SystemIdentity);
        }
    }
    internal class WriteMessage : Message
    {
        public WriteMessage(uint LocalID, uint RemoteID, string Payload)
        {
            SetCommand(Commands.Write);
            SetArgument1(LocalID);
            SetArgument2(RemoteID);
            SetPayload(Payload);
        }
    }
}
