using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Texnomic.AdbNet.Protocol;

namespace Texnomic.AdbNet.Models
{
    public class Shell
    {
        private ShellFlow ShellFlow { get; set; }
        private StreamReader Reader { get; set; }
        private NetworkStream Stream { get; set; }
        public Systems System { get; set; }
        private uint LocalID { get; set; }

        public Shell(NetworkStream Stream, StreamReader Reader, uint LocalID)
        {
            this.Stream = Stream;
            this.Reader = Reader;
            this.System = System;
            this.LocalID = LocalID;
            ShellFlow = new ShellFlow(Stream, Reader, LocalID);
        }

        public async Task<string> Excute(string Command)
        {
            return await ShellFlow.ExcuteShellFlow(Command);
        }
        public async Task<XmlDocument> GetUIXml()
        {
            string Raw = await Excute("uiautomator dump /dev/tty");
            List<string> Terminal = Raw.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            string XML = string.Concat(Terminal.Take(Terminal.Count - 2));
            XmlDocument Document = new XmlDocument();
            Document.LoadXml(XML);
            return Document;
        }

    }
}
