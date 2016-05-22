# ADB.NET

### Description
ADB.NET library is An Android Debug Bridge Client written for .NET Platform(s). It is a C# port for Google's ADB Client<-->Server Protocol. The library provides the following features:

1. Connecting to Google's Offical ADB.exe Running As A Server.
2. Querying ADB Server for its version.
3. Querying ADB Server for The Connected Clients via USB or Emulators.
4. Executing Shell Commands On The Connected Clients

### Usage
```csharp
using Texnomic.AdbNet;

AdbClient Client = new AdbClient();

string Version = Client.GetHostVersion();
string Devices = Client.GetDevices();
string Result = Client.ExcuteShell(5564, "ls");
```