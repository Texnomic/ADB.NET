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
    class ShellWorkflow : Workflow
    {
        public async Task<Terminal> ExcuteShellWorkflow(NetworkStream Stream, StreamReader Reader, uint LocalID, string Payload, Terminal Terminal)
        {
            List<Message> Messages = await REPLWorkflow(Stream, Reader, LocalID, Terminal.RemoteID, Payload);
            string Result = "";
            Messages.ForEach(Message => Result += Message.Payload);
            Terminal.Lines.Add(Result);
            return Terminal;
        }
        public async Task<Terminal> IntializeShellWorkflow(NetworkStream Stream, StreamReader Reader, Systems System, uint LocalID)
        {
            Message ClientConnect = await ConnectWorkflow(Stream, Reader, System);
            Message ClientShell = await OpenShellWorkflow(Stream, Reader, LocalID);
            Terminal Terminal = new Terminal();
            Terminal.RemoteID = ClientShell.Argument1;
            Terminal.Lines.Add(ClientShell.Payload);
            return Terminal;
        }
        private async Task<Message> OpenShellWorkflow(NetworkStream Stream, StreamReader Reader, uint LocalID)
        {
            Message ClientShell = await OpenWorkflow(Stream, Reader, LocalID, "shell:\0");
            await OkayWorkflow(Stream, LocalID, ClientShell.Argument1);
            return ClientShell;
        }
    }
}
