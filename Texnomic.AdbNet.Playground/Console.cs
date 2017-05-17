using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml;
using Texnomic.AdbNet.Models;
using System.Net;
using System.Security.Cryptography;

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

            List<Emulator> Emulators = new List<Emulator>();
            List<UIObject> Objects = new List<UIObject>();

            while (true)
            {
                Emulators = Client.GetEmulators();
                Emulators.ForEach(Emulator => Console.WriteLine($"Emulator:{Emulator.EndPoint.Address}:{Emulator.EndPoint.Port}\n"));

                await Task.Delay(1000);

                Console.Write("> ");
                string Command = Console.ReadLine();
                List<string> Result;
                XmlDocument UIXmlDocument;
                Console.WriteLine("");

                try
                {
                    if (Command == "scan")
                    {
                        Emulators = Client.GetEmulators();
                        Emulators.ForEach(Emulator => Console.WriteLine($"Emulator:{Emulator.EndPoint.Address}:{Emulator.EndPoint.Port}\n"));
                    }
                    else if (Command.StartsWith("select"))
                    {
                        throw new NotImplementedException();
                    }
                    else if (Command.StartsWith("ui"))
                    {
                        StopWatch.Start();
                        UIXmlDocument = await Emulators[0].Shell.GetUIXml();
                        StopWatch.Stop();

                        Objects = UIParser(UIXmlDocument);
                        DrawUITree(Objects);
                    }
                    else if (Command.StartsWith("pull"))
                    {
                        StopWatch.Start();
                        await Emulators[0].Sync.Pull(Command.Split('"')[1]);
                        StopWatch.Stop();
                        Console.WriteLine("[Task Completed]");
                    }
                    else if (Command.StartsWith("install"))
                    {
                        StopWatch.Start();
                        Console.WriteLine(await Emulators[0].Install.Apk(Command.Split('"')[1]));
                        StopWatch.Stop();
                    }
                    else if (Command.StartsWith("root"))
                    {
                        StopWatch.Start();
                        Console.WriteLine(await Emulators[0].Root.Enable());
                        StopWatch.Stop();
                    }
                    else if (Command.StartsWith("mini"))
                    {
                        StopWatch.Start();
                        UIXmlDocument = await Emulators[0].Shell.GetUIXml();
                        StopWatch.Stop();

                        if (Objects.Count == 0) Objects = UIParser(UIXmlDocument);
                        string Shell = GenerateShell(Objects, Command);
                        await Emulators[0].Shell.Excute(Shell);
                    }
                    else
                    {
                        StopWatch.Start();
                        Result = await Emulators[0].Shell.Excute(Command);
                        StopWatch.Stop();
                        Result.ForEach(Line => Console.WriteLine(Line));
                    }
                }
                catch (Exception Error)
                {
                    StopWatch.Stop();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {Error.Message}");
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nTime Taken: {StopWatch.ElapsedMilliseconds.ToString("d")} Milliseconds");
                Console.ForegroundColor = ConsoleColor.Green;


                StopWatch.Reset();
                Console.WriteLine("");
            }
        }

        static string GenerateShell(List<UIObject> Objects, string Mini)
        {
            string[] Commands = Mini.Split(' ');

            string Shell = "";

            foreach (string Input in Commands)
            {
                switch (Input[0])
                {
                    case 'T':
                    case 't':
                        {
                            UIObject Object = Objects.Where(Obj => Obj.GUID == Commands[2]).First();
                            Shell += $"input tap {Object.Bounds[0]} {Object.Bounds[1]};";
                            break;
                        }
                    case 'I':
                    case 'i':
                        {
                            Shell += $"input text {Commands[2].Replace(" ", "%s")};";
                            break;
                        }
                    default: break;
                }

            }

            return Shell;
        }

        static void DrawUITree(List<UIObject> Objects)
        {
            foreach (UIObject Object in Objects)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                string[] Line =
                {
                    "  X: ",
                    Object.Bounds[0].ToString().PadLeft(3, '0'),
                    "  Y: ",
                    Object.Bounds[1].ToString().PadLeft(3, '0')
                };
                Console.Write(string.Concat(Line));

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"  GUID: {Object.GUID}");

                Console.ForegroundColor = ConsoleColor.Blue;
                string Text = Object.Text;
                if (Text == "") Text = "[Empty]";
                Console.Write($"   Name: {Text}".PadRight(20, ' '));

                string ResourceID = Object.ResourceID;
                if (ResourceID == "") ResourceID = "[Empty]";
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"   RID: {ResourceID}");

                Console.ForegroundColor = ConsoleColor.Green;
            }
        }
        static List<UIObject> UIParser(XmlDocument Document)
        {
            List<UIObject> UIObjects = new List<UIObject>();

            XmlNodeList Nodes = Document.SelectNodes("//*");


            using (MD5 md5Hash = MD5.Create())
            {
                foreach (XmlNode Node in Nodes)
                {
                    if (Node.Name != "node") continue;

                    try
                    {
                        UIObject ASO = new UIObject()
                        {
                            GUID = GetMd5Hash(md5Hash, Node.OuterXml, 2),
                            Index = int.Parse(Node.Attributes["index"].Value),
                            Text = Node.Attributes["text"].Value,
                            ResourceID = Node.Attributes["resource-id"].Value,
                            Class = Node.Attributes["class"].Value,
                            Package = Node.Attributes["package"].Value,
                            ContentDesc = Node.Attributes["content-desc"].Value,
                            Checkable = bool.Parse(Node.Attributes["checkable"].Value),
                            Checked = bool.Parse(Node.Attributes["checked"].Value),
                            Clickable = bool.Parse(Node.Attributes["clickable"].Value),
                            Enabled = bool.Parse(Node.Attributes["enabled"].Value),
                            Focusable = bool.Parse(Node.Attributes["focusable"].Value),
                            Focused = bool.Parse(Node.Attributes["focused"].Value),
                            Scrollable = bool.Parse(Node.Attributes["scrollable"].Value),
                            LongClickable = bool.Parse(Node.Attributes["long-clickable"].Value),
                            Password = bool.Parse(Node.Attributes["password"].Value),
                            Selected = bool.Parse(Node.Attributes["selected"].Value),
                            Bounds = Node.Attributes["bounds"]
                                         .Value
                                         .Split(new char[] { ',', '[', ']' }, StringSplitOptions.RemoveEmptyEntries)
                                         .ToList()
                                         .Select(Index => int.Parse(Index))
                                         .ToArray()
                        };
                        UIObjects.Add(ASO);
                    }
                    catch(Exception Error)
                    {
                        Console.Write($"Node: {Node.InnerXml}\nError: {Error.Message}\n");
                    }
                }
            }

            return UIObjects;
        }
        static string GetMd5Hash(MD5 md5Hash, string Input, int Length)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(Input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder Builder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < Length; i++)
            {
                Builder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return Builder.ToString();
        }

        public class UIObject
        {
            public string GUID { get; set; }
            public int Index { get; set; }
            public string Text { get; set; }
            public string ResourceID { get; set; }
            public string Class { get; set; }
            public string Package { get; set; }
            public string ContentDesc { get; set; }
            public bool Checkable { get; set; }
            public bool Checked { get; set; }
            public bool Clickable { get; set; }
            public bool Enabled { get; set; }
            public bool Focusable { get; set; }
            public bool Focused { get; set; }
            public bool Scrollable { get; set; }
            public bool LongClickable { get; set; }
            public bool Password { get; set; }
            public bool Selected { get; set; }
            public int[] Bounds { get; set; }
        }
    }
}
