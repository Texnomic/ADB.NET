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
    public class Install
    {
        private Flows Flows;
        private IPEndPoint EndPoint;
        private bool IsRoot;
        private uint LocalID;
        private uint RemoteID;

        public Install(IPEndPoint EndPoint)
        {
            this.EndPoint = EndPoint;
            IsRoot = false;
            LocalID = (uint)(new Random().Next(0, 9999));
        }

        public async Task<string> Apk(string ApkPath)
        {
            Flows = new Flows(EndPoint);

            await Flows.ConnectFlow<WriteMessage>(Systems.Host);

            FileInfo Info = new FileInfo(ApkPath);

            OkayMessage OpenOkay = await Flows.OpenFlow<OkayMessage>(LocalID, $"exec:cmd package 'install' -S {Info.Length}\0");

            RemoteID = OpenOkay.Argument1;

            byte[] Data = File.ReadAllBytes(Info.FullName);

            decimal FileLength = Info.Length;
            decimal MaxData = Constants.MaxData;

            decimal Rounds = Math.Ceiling(FileLength / MaxData);

            for (int i = 0; i < Rounds + 1; i++)
            {
                OkayMessage WriteOkay = await Flows.WriteFlow<OkayMessage>(LocalID, RemoteID, Data.Skip((int)(Constants.MaxData * i)).Take((int)Constants.MaxData).ToArray());
            }

            WriteMessage InstallResult = await Flows.RecievePayloadFlow<WriteMessage>(LocalID, RemoteID);

            CloseMessage CloseResult = await Flows.SendCloseFlow<CloseMessage>(LocalID, RemoteID);

            return Encoding.UTF8.GetString(InstallResult.Payload);
        }
    }
}
