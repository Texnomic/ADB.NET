using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texnomic.AdbNet.Protocol
{
    public enum Systems { Bootloader, Device, Host }
    public enum Command { SYNC = 0x434e5953, CNXN = 0x4e584e43, AUTH = 0x48545541, OPEN = 0x4e45504f, OKAY = 0x59414b4f, CLSE = 0x45534c43, WRTE = 0x45545257 };
}
