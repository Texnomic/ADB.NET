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
        public bool Start()
        {
            Process[] Processes = Process.GetProcessesByName("adb");
            if (Processes.Length > 0) return true;

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
        }
        public async Task Stop()
        {
            string Result = await ExcuteCommand("host:kill");
        }

        #region Internal Methods
        internal async Task<string> ExcuteCommand(string Command)
        {
            await Connect(5037);
            await WriteCommand(Command);
            string Result = await ReadStatus();
            Disconnect();
            return Result;
        }
        internal async Task WriteCommand(string Command)
        {
            await Writer.WriteLineAsync(Encode(Command));
            await Writer.FlushAsync();
        }
        internal async Task<string> ReadCommand()
        {
            string Status = await ProcessStatus();

            string Hex = await Read(4);
            int Length = Convert.ToInt32(Hex, 16);

            string Message = await Read(Length);

            return Message;
        }
        internal async Task<string> ProcessStatus()
        {
            string Status = await ReadStatus();
            if (Status == "FAIL") throw new CommandFailedException();
            if (Status != "OKAY") throw new UnexpectedMessageException();
            return Status;
        }
        internal async Task<string> ReadStatus()
        {
            return await Read(4);
        }
        internal async Task<string> Read(int Length = 1)
        {
            char[] Data = new char[Length];
            int Index = 0;

            while (true)
            {
                await Reader.ReadBlockAsync(Data, Index, Length);
                Index = Array.IndexOf<char>(Data, '\0');
                if (Index == -1) break;
                Length = Length - Index;
                Thread.Sleep(100);
            }

            return string.Concat(Data);
        }
        internal string Encode(string Command)
        {
            return $"{CalculateLength(Command)}{Command}";
        }
        internal string CalculateLength(string Message)
        {
            return Message.Length.ToString("X4");
        }
        #endregion
    }
}
