using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Texnomic.AdbNet
{
    public class AdbServer : BaseClient
    {
        public async Task<bool> Start()
        {
            return await Task.Run(() =>
            {
                if (Process.GetProcessesByName("adb").Length > 0) return true;

                string SdkDirectory = Environment.GetEnvironmentVariable("ANDROID_HOME", EnvironmentVariableTarget.Machine);
                if (SdkDirectory == null) SdkDirectory = Environment.GetEnvironmentVariable("ANDROID_HOME", EnvironmentVariableTarget.User);
                if (SdkDirectory == null) throw new SdkNotFoundException();
                if (!Directory.Exists(SdkDirectory)) throw new SdkNotFoundException();

                string AdbPath = Path.Combine(SdkDirectory, "platform-tools", "adb.exe");
                if (!File.Exists(AdbPath)) throw new AdbNotFoundException();

                Process AdbServer = new Process();
                AdbServer.StartInfo.FileName = AdbPath;
                AdbServer.StartInfo.Arguments = "start-server";
                AdbServer.StartInfo.UseShellExecute = false;
                AdbServer.StartInfo.CreateNoWindow = true;
                AdbServer.StartInfo.RedirectStandardOutput = true;
                AdbServer.StartInfo.RedirectStandardInput = true;
                AdbServer.StartInfo.RedirectStandardError = true;
                AdbServer.Start();

                Thread.Sleep(1000);

                if (AdbServer.HasExited) throw new UnableToStartAdbServerException();

                return true;
            });
        }
        public async Task<string> Stop()
        {
            return await ServerQuery("host:kill");
        }
        public async Task<string> GetHostVersion()
        {
            return await ServerQuery("host:version");
        }
        public async Task<string> GetDevices()
        {
            return await ServerQuery("host:devices");
        }
    }
}
