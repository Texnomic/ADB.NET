using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Texnomic.AdbNet
{
    public class BaseClient
    {
        private const string IP = "127.0.0.1";
        private const int ServerPort = 5037;

        internal async Task<string> ServerQuery(string Query)
        {
            if (Process.GetProcessesByName("adb").Length == 0) throw new AdbServerNotRunningException();

            using (TcpClient Client = new TcpClient())
            {
                await Client.ConnectAsync(IP, ServerPort);

                using (NetworkStream Stream = Client.GetStream())
                {
                    using (StreamWriter Writer = new StreamWriter(Stream))
                    {
                        using (StreamReader Reader = new StreamReader(Stream))
                        {
                            await Writer.WriteAsync($"{Query.Length.ToString("X4")}{Query}");
                            await Writer.FlushAsync();
                            QueryResult Query2Result = await GetQueryResult(Reader);

                            return Query2Result.Message;
                        }
                    }
                }
            }
        }
        internal async Task<string> ClientQuery(int ClientPort, string Query)
        {
            if (Process.GetProcessesByName("adb").Length == 0) throw new AdbServerNotRunningException();

            using (TcpClient Client = new TcpClient())
            {
                await Client.ConnectAsync(IP, ServerPort);

                using (NetworkStream Stream = Client.GetStream())
                {
                    using (StreamWriter Writer = new StreamWriter(Stream))
                    {
                        using (StreamReader Reader = new StreamReader(Stream))
                        {
                            string SwitchQuery = $"host:transport:emulator-{ClientPort}";
                            await Writer.WriteAsync($"{SwitchQuery.Length.ToString("X4")}{SwitchQuery}");
                            await Writer.FlushAsync();
                            string Status = await ReadStatus(Reader);

                            await Writer.WriteAsync($"{Query.Length.ToString("X4")}{Query}");
                            await Writer.FlushAsync();

                            return await Reader.ReadToEndAsync();
                        }
                    }
                }
            }
        }


        private async Task<QueryResult> GetQueryResult(StreamReader Reader)
        {
            return new QueryResult
            {
                Status = await ReadStatus(Reader),
                Message = await ReadMessage(Reader)
            };
        }
        private async Task<string> ReadStatus(StreamReader Reader)
        {
            char[] StatusArray = new char[4];
            await Reader.ReadBlockAsync(StatusArray, 0, 4);
            string Status = string.Concat(StatusArray);
            if (Status == "FAIL") throw new CommandFailedException();
            if (Status != "OKAY") throw new UnexpectedMessageException();
            return Status;
        }
        private async Task<int> ReadLength(StreamReader Reader)
        {
            char[] Length = new char[4];
            await Reader.ReadBlockAsync(Length, 0, 4);
            return Convert.ToInt32(string.Concat(Length), 16);
        }
        private async Task<string> ReadMessage(StreamReader Reader)
        {
            int Length = await ReadLength(Reader);
            char[] Message = new char[Length];
            await Reader.ReadBlockAsync(Message, 0, Length);
            if (Message.Length != Length) throw new WrongMessageLengthException();
            return string.Concat(Message);
        }

    }
}
