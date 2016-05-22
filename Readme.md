# ADB.NET

### Description
ADB.NET Library is A Client-Side C# Implementation Of The Android Debug Bridge for .NET Platform(s). Currently, It is A Port For The ADB *Client --> Server* Protocol.

### Targets
1. Providing A Direct Programmable Access To ADB's Cliend-Side Features without using "ADB.exe".
2. Working-Around Google's Bad ADB Protocol Documentation.
3. Providing A Clear & Understandble APIs.

### Functions
1. AdbClient: Querying ADB Server for its version.
2. AdbClient: Querying ADB Server for The Connected Clients. (USB or Emulator)
3. AdbClient: Executing Shell Commands On The Connected Clients
4. AdbServer: Start & Stop ADB Server Using The Offical "ADB.exe". (Will Be Replaced In A Future Implementation.)

### Usage
```csharp
using Texnomic.AdbNet;

AdbClient Client = new AdbClient();

string Version = Client.GetHostVersion();
string Devices = Client.GetDevices();
string Result = Client.ExcuteShell(5564, "ls");
```

```csharp
using Texnomic.AdbNet;

AdbServer Server = new AdbServer();

Server.Start();
await Server.Stop();
```