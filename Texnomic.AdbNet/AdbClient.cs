using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using Texnomic.AdbNet.Models;
using System.Management;
using System.Runtime.CompilerServices;

namespace Texnomic.AdbNet
{
    /// <summary>
    ///  The main ADB Client class, it includes both of the Client and Server functionality.
    ///  Note: Only one instance should be used per machine to avoid getting Server Listener conflicts.
    /// </summary>
    public class AdbClient
    {
        private const string Localhost = "127.0.0.1";
        private const int Port = 5037;
        private const uint LocalID = 1234;
        private IPGlobalProperties IPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        private List<Task> BackgroundTasks = new List<Task>();
        private List<Emulator> Emulators = new List<Emulator>();
        private List<Task> MonitoredClients = new List<Task>();

        /// <summary>Intalizing a new instance of the ADB Client Class.</summary>
        /// <remarks>Note: The Server Listener will start automatically as a background Task.</remarks>
        public AdbClient()
        {
            BackgroundTasks.Add(Task.Run(() => MonitorServer()));
        }

        /// <summary>
        /// Refresh the list of Emulators currently running on the local machine.
        /// New Emulators will be added, closed or unresponsive Emulators will be removed.
        /// </summary>
        /// <remarks>Note: New Emulators are added automatically by the background Server Task.</remarks>
        /// <returns>Generic List of Emulator Class.</returns>
        public List<Emulator> GetEmulators()
        {
            Scan();
            return Emulators;
        }

        private void MonitorServer()
        {
            CheckAdb();

            TcpListener Listener = new TcpListener(IPAddress.Any, Port);
            Listener.Start();

            while (true)
            {
                MonitoredClients.Add(HandleClient(Listener.AcceptTcpClient()));
            }
        }
        private void Scan()
        {
            List<IPEndPoint> EndPoints = GetEndPoints();

            for (int i = 0; i < Emulators.Count; i++)
            {
                if (EndPoints.Contains(Emulators[i].EndPoint)) continue;
                Emulators[i].Cleanup();
                Emulators.Remove(Emulators[i]);
            }

            for (int i = 0; i < EndPoints.Count; i++)
            {
                if (Emulators.Exists(Emulator => Emulator.EndPoint.Port == EndPoints[i].Port)) continue;
                Emulators.Add(new Emulator(EndPoints[i], LocalID));
            }
        }
        private List<IPEndPoint> GetEndPoints()
        {
            // 169.254.190.187
            List<IPEndPoint> EndPoints = IPGlobalProperties.GetActiveTcpListeners()
                                     .Where(EP => EP.Port >= 5555)
                                     .Where(EP => EP.Port <= 5585)
                                     .Where(EP => EP.Port % 2 == 1)
                                     .Where(EP => EP.AddressFamily == AddressFamily.InterNetwork)
                                     .ToList();

            return EndPoints;
        }


        private void CheckAdb()
        {
            if (Process.GetProcessesByName("adb").Length > 0)
            {
                KillAdbServer();
            }
        }
        private void KillAdbServer()
        {
            using (TcpClient Client = new TcpClient())
            {
                Client.Connect(Localhost, Port);

                using (StreamWriter Writer = new StreamWriter(Client.GetStream()))
                {
                    using (StreamReader Reader = new StreamReader(Client.GetStream()))
                    {
                        Writer.Write("0009host:kill");
                        Writer.Flush();
                        string Result = Reader.ReadToEnd();
                        Process.GetProcessesByName("adb")[0].WaitForExit();
                        Console.WriteLine("ADB Server Killed.");
                    }
                }
            }
        }

        private Task HandleClient(TcpClient Client)
        {
            using (StreamReader Reader = new StreamReader(Client.GetStream()))
            {
                using (StreamWriter Writer = new StreamWriter(Client.GetStream()))
                {
                    char[] RawSize = new char[4];
                    Reader.Read(RawSize, 0, 4);
                    int Size = Convert.ToInt32(string.Concat(RawSize), 16);

                    char[] RawMessage = new char[Size];
                    Reader.Read(RawMessage, 0, Size);

                    string Message = string.Concat(RawMessage);

                    //Console.WriteLine(Message);

                    if (Message == "host:version")
                    {
                        Writer.Write("OKAY");
                        Writer.Flush();

                        Writer.Write("00040027");
                        Writer.Flush();

                        Client.Close();

                        //MonitoredClients.Remove();
                    }

                    if (Message.StartsWith("host-serial:emulator-"))
                    {
                        Writer.Write("FAIL");
                        Writer.Flush();

                        Writer.Write("0020device 'emulator-5564' not found");

                        Client.Close();
                    }

                    if (Message.StartsWith("host:connect:"))
                    {
                        Writer.Write("OKAY");
                        Writer.Flush();

                        IPAddress IP = IPAddress.Parse(Message.Split(':')[2]);
                        IPEndPoint EndPoint = new IPEndPoint(IP, 5555);

                        if (Emulators.Exists(Emulator => Emulator.EndPoint.GetHashCode() == EndPoint.GetHashCode()) == false)
                        {
                            Emulators.Add(new Emulator(EndPoint, LocalID));
                        }

                        Writer.Write("0009Connected");
                        Writer.Flush();

                        Client.Close();
                    }

                    if (Message.StartsWith("host:disconnect:"))
                    {
                        Writer.Write("OKAY");
                        Writer.Flush();

                        IPAddress IP = IPAddress.Parse(Message.Split(':')[2]);
                        IPEndPoint EndPoint = new IPEndPoint(IP, 5555);

                        for (int i = 0; i < Emulators.Count; i++)
                        {
                            if (Emulators[i].EndPoint.GetHashCode() == EndPoint.GetHashCode())
                            {
                                Emulators[i].Cleanup();
                                Emulators.Remove(Emulators[i]);
                                break;
                            }
                        }

                        Writer.Write("000CDisconnected");
                        Writer.Flush();

                        Client.Close();
                    }

                    if (Message.StartsWith("host:emulator:"))
                    {
                        Client.Close();
                    }

                    if (Message == "host:track-devices")
                    {
                        Writer.Write("OKAY");
                        Writer.Flush();

                        Writer.Write("0000");
                        Writer.Flush();

                        Writer.Write("0016emulator-5564\toffline\n");
                        Writer.Flush();

                        Scan();

                        Writer.Write("0015emulator-5564\tdevice\n");
                        Writer.Flush();

                        Reader.Read();
                    }

                    throw new NotImplementedException(Message);
                }
            }
        }
    }

}
