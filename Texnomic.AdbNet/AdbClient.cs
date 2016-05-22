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
        public async Task<string> GetHostVersion()
        {
           return await ExcuteCommand("host:version");
        }
        public async Task<string> GetDevices()
        {
           return await ExcuteCommand("host:devices");
        }
        public async Task<string> ExcuteShell(int Port, string Command)
        {
            await Connect(5037);
            await SwitchTransport(Port);
            await WriteShell(Command);
            string Result = await ReadShell();
            Disconnect();
            return Result;
        }
        #endregion

        #region Internal Methods
        internal async Task<string> ExcuteCommand(string Command)
        {
            await Connect(5037);
            await WriteCommand(Command);
            string Result = await ReadCommand();
            Disconnect();
            return Result;
        }
        internal async Task Register(int Port)
        {
            await Connect(5037);
            await WriteCommand($"host:emulator:{Port}");
            Disconnect();
        }
        internal async Task SwitchTransport(int Port)
        {
            await WriteCommand($"host:transport:emulator-{Port}");
            string Status = await ProcessStatus();
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
        internal async Task<string> ReadShell()
        {
            await ProcessStatus();
            return await Reader.ReadToEndAsync();
        }
        internal async Task WriteShell(string Command)
        {
            await WriteCommand($"shell:{Command}");
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
