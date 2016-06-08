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
    internal class Shell
    {
        private bool IsIntialized = false;
        private ShellWorkflow ShellWorkflow { get; set; }
        private Terminal Terminal { get; set; }
        private StreamReader Reader { get; set; }
        private NetworkStream Stream { get; set; }
        private Systems System { get; set; }
        private uint LocalID { get; set; }

        public Shell(NetworkStream Stream, StreamReader Reader, Systems System, uint LocalID)
        {
            this.Stream = Stream;
            this.Reader = Reader;
            this.System = System;
            this.LocalID = LocalID;
            ShellWorkflow = new ShellWorkflow();
        }

        private async Task<string> Intialize()
        {
            Terminal = await ShellWorkflow.IntializeShellWorkflow(Stream, Reader, Systems.Host, LocalID);
            IsIntialized = true;
            return Terminal.Lines.Last();
        }

        public async Task<string> ExcuteShell(string Command)
        {
            if (!IsIntialized) await Intialize();
            int Count = Terminal.Lines.Count;
            Terminal = await ShellWorkflow.ExcuteShellWorkflow(Stream, Reader, LocalID, $"{Command}", Terminal);
            return Terminal.Lines.Last();
        }
    }
}
