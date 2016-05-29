using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texnomic.AdbNet.Protocol
{
    internal enum Commands { SYNC = 0x434e5953, Connect = 0x4e584e43, AUTH = 0x48545541, Open = 0x4e45504f, Okay = 0x59414b4f, Close = 0x45534c43, Write = 0x45545257 };
}
