using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Texnomic.AdbNet
{
    public class AdbClient : BaseClient
    {
        #region Public Methods
        public async Task<string> ExcuteShell(int Port, string Query)
        {
            Query = $"shell: {Query}";
            return await ClientQuery(Port, Query);
        }
        #endregion

    }
}
