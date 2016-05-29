using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texnomic.AdbNet.Models
{
    internal class Terminal
    {
        public uint RemoteID { get; set; }
        public List<string> Lines = new List<string>();
    }
}
