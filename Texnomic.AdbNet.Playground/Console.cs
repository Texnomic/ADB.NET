using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Texnomic.AdbNet;
using System.Threading;

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
            Stopwatch StopWatch = new Stopwatch();

            List<Emulator> Emulators = Client.GetEmulators();

            Emulators.ForEach(Emulator => Console.WriteLine($"Emulator: {Emulator.EndPoint.Port}\n"));

            while (true)
            {
                Console.Write("> ");
                string Command = Console.ReadLine();
                string Result = "";

                StopWatch.Start();
                try
                {
                    Result = await Emulators[0].ExcuteShell(Command);
                }
                catch(Exception Error)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {Error.Message}");
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                StopWatch.Stop();

                Console.Write(Result);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n\nTime Taken: {StopWatch.ElapsedMilliseconds.ToString("d")} Milliseconds");
                Console.ForegroundColor = ConsoleColor.Green;

                StopWatch.Reset();
                Console.WriteLine("");
            }

        }
    }
}
