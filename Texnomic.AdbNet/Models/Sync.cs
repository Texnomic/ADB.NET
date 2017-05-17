using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Texnomic.AdbNet.Protocol;

namespace Texnomic.AdbNet.Models
{
    public class Sync
    {
        private SyncFlow SyncFlow;

        public Sync(IPEndPoint EndPoint)
        {
            SyncFlow = new SyncFlow(EndPoint);
        }

        public async Task Pull(string FilePath)
        {
            await SyncFlow.Pull(FilePath);
        }


    }
}
