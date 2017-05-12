using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Texnomic.AdbNet.Protocol;

namespace Texnomic.AdbNet.Models
{
    class ShellFlow
    {
        private Flows Flows = new Flows();
        private bool IsIntialized = false;
        private uint LocalID;
        private uint RemoteID;
        private NetworkStream Stream;
        private StreamReader Reader;
 
        public ShellFlow(NetworkStream Stream, StreamReader Reader, uint LocalID)
        {
            this.Stream = Stream;
            this.Reader = Reader;
            this.LocalID = LocalID;
        }

        public async Task<string> ExcuteShellFlow(string Command)
        {
            if (!IsIntialized) await IntializeShellFlow(Stream, Reader, LocalID);


            foreach (char Character in Command)
            {
                await Flows.WriteFlow(Stream, LocalID, RemoteID, "\0\x01\0\0\0" + Character);
                await Flows.RecievePayloadFlow(Stream, Reader, LocalID, RemoteID);
            }

            await Flows.WriteFlow(Stream, LocalID, RemoteID, "\0\x01\0\0\0\r");

            await Flows.RecievePayloadFlow(Stream, Reader, LocalID, RemoteID);

            List<Message> ShellMessages = new List<Message>();

            while (true)
            {
                Message Message = await Flows.RecievePayloadFlow(Stream, Reader, LocalID, RemoteID, false);

                ShellMessages.Add(Message);

                if (Message.Payload.EndsWith(":/ $ "))
                {
                    break;
                }
            }

            return string.Join(Environment.NewLine, ShellMessages.Select(Msg => GetPrintableCharacters(Msg.Payload)));
        }
        private async Task<uint> IntializeShellFlow(NetworkStream Stream, StreamReader Reader, uint LocalID)
        {
            Message ConnectOkay = await Flows.ConnectFlow(Stream, Reader, Systems.Host);

            Message OpenOkay = await Flows.OpenFlow(Stream, Reader, LocalID, $"shell,v2,cmd:\0"); //try cmd instrad of pty
            //Message OpenOkay = await Flows.OpenFlow(Stream, Reader, LocalID, $"shell,v2,pty:\0"); //try cmd instrad of pty

            RemoteID = OpenOkay.Argument1;

            Message WriteOkay = await Flows.WriteFlow(Stream, LocalID, RemoteID, "\x05\v\0\0\030x120,0x0\0");

            Message ConsoleWelcome = await Flows.RecievePayloadFlow(Stream, Reader, LocalID, RemoteID);

            IsIntialized = true;

            return RemoteID;
        }
        private string GetPrintableCharacters(string Data)
        {
            StringBuilder Builder = new StringBuilder();

            foreach (char Character in Data)
            {
                if (Character == 10)
                {
                    Builder.Append(Character);
                    continue;
                }

                if (Character >= 32 && Character <= 126)
                {
                    Builder.Append(Character);
                }
            }

            return Builder.ToString();
        }
    }
}
