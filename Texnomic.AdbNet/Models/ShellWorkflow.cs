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
            Message Message = await REPLWorkflow(Stream, Reader, LocalID, Terminal.RemoteID, Payload);
            Terminal.Lines.Add(Message.Payload);
            return Terminal;
        }
        public async Task<Terminal> GetShellWorkflow(NetworkStream Stream, Systems System, uint LocalID)
        {
            Message ClientConnect = await ConnectWorkflow(Stream, System);
            Message ClientShell = await OpenShellWorkflow(Stream, LocalID);
            Terminal Terminal = new Terminal();
            Terminal.RemoteID = ClientShell.Argument1;
            Terminal.Lines.Add(ClientShell.Payload);
            return Terminal;
        }
        private async Task<Message> OpenShellWorkflow(NetworkStream Stream, uint LocalID)
        {
            Message ClientShell = await OpenWorkflow(Stream, LocalID, "shell:\0");
            await ReadyWorkflow(Stream, LocalID, ClientShell.Argument1);
            return ClientShell;
        }
    }
}
