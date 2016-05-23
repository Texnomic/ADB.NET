using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Texnomic.AdbNet;

namespace Texnomic.AdbNet.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Texnomic ADB.Net Playground";
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("=================================");
            Console.WriteLine("== Texnomic ADB.Net Playground ==");
            Console.WriteLine("=================================");
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Green;
            MainAsync().Wait();
        }

        async static Task MainAsync()
        {
            AdbClient Client = new AdbClient();
            AdbServer Server = new AdbServer();
            Stopwatch StopWatch = new Stopwatch();

            await Server.Start();
            //await Server.Stop();

            string Devices = await Server.GetDevices();
            Console.Write(Devices);
            Console.WriteLine("");

            while (true)
            {
                Console.Write("> ");
                string Command = Console.ReadLine();
                string Result = "";

                StopWatch.Start();
                try
                {
                    Result = await Client.ExcuteShell(5564, Command);
                }
                catch(Exception Error)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Error.Message);
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                StopWatch.Stop();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Time Taken: {StopWatch.ElapsedMilliseconds.ToString("d")} Milliseconds");
                Console.ForegroundColor = ConsoleColor.Green;

                StopWatch.Reset();
                Console.Write(Result);
                Console.WriteLine("");
            }

        }
    }
}
