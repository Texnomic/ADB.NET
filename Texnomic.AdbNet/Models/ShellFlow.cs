using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Texnomic.AdbNet.Protocol;

namespace Texnomic.AdbNet.Models
{
    class ShellFlow
    {
        private Flows Flows;
        private IPEndPoint EndPoint;
        private bool IsIntialized;
        private uint LocalID;
        private uint RemoteID;
        private string Username;

        public ShellFlow(IPEndPoint EndPoint)
        {
            this.EndPoint = EndPoint;
            IsIntialized = false;
            LocalID = (uint)(new Random().Next(0, 9999));
        }

        public async Task<List<string>> ExcuteShellFlow(string Command)
        {
            if (!IsIntialized) await OpenShellFlow();

            foreach (char Character in Command)
            {
                await Flows.WriteFlow<OkayMessage>(LocalID, RemoteID, "\0\x01\0\0\0" + Character);
                await Flows.RecievePayloadFlow<WriteMessage>(LocalID, RemoteID);
            }

            await Flows.WriteFlow<OkayMessage>(LocalID, RemoteID, "\0\x01\0\0\0\r");

            List<string> Console = new List<string>();
            Message Message;

            while (true)
            {
                Message = await Flows.RecieveStreamingPayloadFlow<WriteMessage>(LocalID, RemoteID);

                if (Message.Command == Protocol.Command.CLSE)
                {
                    //Happens Due To Sending Shell "exit" Command.
                    throw new NotImplementedException();
                }

                if (Message.Command != Protocol.Command.WRTE)
                {
                    throw new UnexpectedMessageException();
                }

                Console.Add(Encoding.UTF8.GetString(Message.Payload));

                if (Console.Last().Contains(Username))
                {
                    break;
                }
            }

            string CompletePayload = string.Concat(Console);

            return SimulateConsole(CompletePayload);
        }


        private async Task<Message> OpenShellFlow()
        {
            Flows = new Flows(EndPoint);

            WriteMessage WriteMessage = await Flows.ConnectFlow<WriteMessage>(Systems.Host);

            OkayMessage OpenOkay = await Flows.OpenFlow<OkayMessage>(LocalID, $"shell,v2,pty:\0");

            RemoteID = OpenOkay.Argument1;

            OkayMessage WriteOkay = await Flows.WriteFlow<OkayMessage>(LocalID, RemoteID, "\x05\v\0\0\030x120,0x0\0");

            WriteMessage ConsoleWelcome = await Flows.RecievePayloadFlow<WriteMessage>(LocalID, RemoteID);

            Username = SimulateConsole(Encoding.UTF8.GetString(ConsoleWelcome.Payload))[0];

            Username = Username.Split('/')[0];

            IsIntialized = true;

            return ConsoleWelcome;
        }

        private List<string> SimulateConsole(string Data)
        {
            List<string> Console = new List<string>();

            StringBuilder Builder = new StringBuilder();

            //File.WriteAllText("Data.txt", Data);

            for (int i = 0; i < Data.Length; i++)
            {
                if (Char.IsControl(Data[i]))
                {
                    if (Data[i] == 1) //Start Of Header (Strange Undocumented Behaviour)
                    {
                        i += 1;
                    }

                    if (Data[i] == 8) //Backspace
                    {
                        if (Builder.Length - 1 >= 0) //Index Check
                        {
                            if (char.IsControl(Data[i - 1]))
                            {
                                continue;
                            }
                            else
                            {
                                Builder.Remove(Builder.Length - 1, 1);
                            }
                        }
                    }

                    if (Data[i] == 13) //Carriage Return
                    {
                        if (Data.Length > i + 1) //Index Check
                        {
                            if (Data[i + 1] == 10) //New Line
                            {
                                if (Builder.Length > 0) //Avoid Empty Lines
                                {
                                    Console.Add(Builder.ToString());
                                }

                                Builder.Clear();
                                i += 1;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }

                    continue;
                }
                else
                {
                    if (Data[i] == 65533)
                    {
                        continue;
                    }

                    Builder.Append(Data[i]);
                }
            }

            Console.Add(Builder.ToString());

            return Console;
        }


    }
}
