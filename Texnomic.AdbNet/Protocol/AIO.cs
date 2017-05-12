using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Texnomic.AdbNet
{
    class Protocolv2
    {

        const uint Version = 0x01000000;
        const uint MaxData = 0x00040000;
        const string Serial = "";
        const string Banner = "features=stat_v2,shell_2,cmd";
        internal enum Commands { SYNC = 0x434e5953, CNXN = 0x4e584e43, AUTH = 0x48545541, OPEN = 0x4e45504f, OKAY = 0x59414b4f, CLSE = 0x45534c43, WRTE = 0x45545257 };
        internal enum Systems { Bootloader, Device, Host }

        internal class Packet
        {
            public Commands? Command { get; set; }
            public uint Argument1 { get; set; }
            public uint Argument2 { get; set; }
            public uint PayloadLength { get; set; }
            public uint FakeCRC32 { get; set; }
            public uint Magic { get; set; }
            public string Payload { get; set; }
        }
        internal class OkayMessage : Packet
        {
            public OkayMessage(uint LocalID, uint RemoteID)
            {
                Command = Commands.OKAY;
                Argument1 = LocalID;
                Argument1 = RemoteID;
                Payload = "";
            }
        }
        internal class OpenMessage : Packet
        {
            public OpenMessage(uint LocalID, string Destination)
            {
                Command = Commands.OPEN;
                Argument1 = LocalID;
                Argument2 = 0x0;
                Payload = Destination;
            }
        }
        internal class ConnectMessage : Packet
        {
            public ConnectMessage()
            {
                Command = Commands.CNXN;
                Argument1  = Version; //Version
                Argument2 = MaxData; //Max Data
                Payload = $"{Systems.Host.ToString()}:{Serial}:{Banner}";
            }
        }
        internal class WriteMessage : Packet
        {
            public WriteMessage(uint LocalID, uint RemoteID, string Payload)
            {
                Command = Commands.WRTE;
                Argument1 = LocalID;
                Argument2 = RemoteID;
                this.Payload = Payload;
            }
        }
        internal class CloseMessage : Packet
        {
            public CloseMessage(uint LocalID, uint RemoteID)
            {
                Command = Commands.CLSE;
                Argument1 = LocalID;
                Argument2 = RemoteID;
                Payload = "";
            }
        }

        private async Task<Commands> ReadCommand(StreamReader Reader)
        {
            char[] CommandChars = await Read(Reader, 4);
            string CommandString = string.Concat(CommandChars);

            switch (CommandString)
            {
                case "CNXN": return Commands.CNXN;
                case "OPEN": return Commands.OPEN;
                case "OKAY": return Commands.OKAY;
                case "WRTE": return Commands.WRTE;
                case "CLSE": throw new NotImplementedException("Message with CLSE Command.");
                case "AUTH": throw new NotImplementedException("Message with AUTH Command.");
                case "SYNC": throw new NotImplementedException("Message with SYNC Command.");
                default: throw new InvalidMessageTypeException();
            }
        }
        private async Task<string> ReadArgument(StreamReader Reader)
        {
            char[] ArgumentChars = await Read(Reader, 4);
            string ArgumentString = string.Concat(ArgumentChars);
            return ArgumentString;
        }
        private async Task<byte[]> Read(NetworkStream Stream, int Lenght)
        {
            byte[] Payload = new byte[Lenght];
            await Stream.ReadAsync(Payload, 0, Payload.Length);
            return Payload;
        }
        private async Task<char[]> Read(StreamReader Reader, int Lenght)
        {
            char[] Payload = new char[Lenght];
            await Reader.ReadBlockAsync(Payload, 0, Payload.Length);
            return Payload;
        }
    }
}
