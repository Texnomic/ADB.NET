using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Texnomic.AdbNet.Protocol;

namespace Texnomic.AdbNet.Models
{
    class SyncFlow
    {
        private Flows Flows;
        private IPEndPoint EndPoint;
        private uint LocalID = (uint)(new Random().Next(0, 9999));
        private uint RemoteID;


        public SyncFlow(IPEndPoint EndPoint)
        {
            this.EndPoint = EndPoint;
        }

        private async Task<WriteMessage> Connect()
        {
            Flows = new Flows(EndPoint);
            return await Flows.ConnectFlow<WriteMessage>(Systems.Host);
        }

        private async Task<OkayMessage> Open()
        {
            OkayMessage OpenOkay = await Flows.OpenFlow<OkayMessage>(LocalID, $"sync:\0");
            RemoteID = OpenOkay.Argument1;
            return OpenOkay;
        }

        private async Task<OkayMessage> SendStat(string FilePath)
        {
            StatSyncMessage Stat_Request = new StatSyncMessage(LocalID, RemoteID, FilePath);
            return await Flows.WriteFlow<OkayMessage>(Stat_Request.Message);
        }

        private async Task<StatSyncMessage> RecieveStat()
        {
            WriteMessage WriteMessage = await Flows.RecievePayloadFlow<WriteMessage>(LocalID, RemoteID);
            return new StatSyncMessage(WriteMessage);
        }

        private async Task<OkayMessage> SendRecv(string FilePath)
        {
            RecvSyncMessage RecvSyncMessage = new RecvSyncMessage(LocalID, RemoteID, FilePath);
            return await Flows.WriteFlow<OkayMessage>(RecvSyncMessage.Message);
        }

        private async Task<DataSyncMessage> RecieveData()
        {
            WriteMessage WriteMessage = await Flows.RecieveStreamingPayloadFlow<WriteMessage>(LocalID, RemoteID);
            return new DataSyncMessage(WriteMessage);
        }

        public async Task Pull(string FilePath)
        {
            WriteMessage Connect_WriteMessage = await Connect();
            OkayMessage Open_OkayMessage = await Open();
            OkayMessage SendStat_OkayMessage = await SendStat(FilePath);
            StatSyncMessage RecieveStat_StatSyncMessage = await RecieveStat();
            OkayMessage SendRecv_OkayMessage = await SendRecv(FilePath);

            FileStream FileStream = File.OpenWrite(FilePath.Split('/').Last());

            DataSyncMessage DataSyncMessage = await RecieveData();

            //Files Smaller Than 64K Fits inside the first DataSyncMessage Packet.

            if (DataSyncMessage.EndOfFile == false)
            {
                WriteMessage WriteMessage;
                string EOF;

                while (true)
                {
                    WriteMessage = await Flows.RecieveStreamingPayloadFlow<WriteMessage>(LocalID, RemoteID);

                    await FileStream.WriteAsync(WriteMessage.Payload, 0, WriteMessage.Payload.Length);

                    EOF = Encoding.UTF8.GetString(WriteMessage.Payload.Skip(WriteMessage.Payload.Length - 8).Take(4).ToArray());

                    if (EOF == "DONE") break;
                }
            }
            else
            {
                await FileStream.WriteAsync(DataSyncMessage.Payload, 0, DataSyncMessage.Payload.Length);
            }
            
            await FileStream.FlushAsync();

            FileStream.Close();

            OkayMessage QuiteOkay = await Flows.WriteFlow<OkayMessage>(LocalID, RemoteID, "QUIT\0\0\0\0");

            CloseMessage Close = await Flows.RecieveCloseFlow<CloseMessage>(LocalID, RemoteID);
        }
    }
}
