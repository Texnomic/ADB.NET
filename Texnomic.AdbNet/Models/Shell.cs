using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Texnomic.AdbNet.Protocol;

namespace Texnomic.AdbNet.Models
{
    public class Shell
    {
        private ShellFlow ShellFlow;

        public Shell(IPEndPoint EndPoint)
        {
            ShellFlow = new ShellFlow(EndPoint);
        }

        public async Task<List<string>> Excute(string Command)
        {
            return await ShellFlow.ExcuteShellFlow(Command);
        }
        public async Task<XmlDocument> GetUIXml()
        {
            List<string> Raw = await Excute("uiautomator dump /dev/tty");
            Raw[0] = Raw[0].Replace("UI hierchary dumped to: /dev/tty", "");
            XmlDocument Document = new XmlDocument();
            Document.LoadXml(Raw[0]);
            return Document;
        }

    }
}
