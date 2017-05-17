# ADB.NET
##Note: *Work-In-Progress*

### Description
ADB.NET Library is A Complete C# Implementation Of The Android Debug Bridge Client & Server Protocol(s) for .NET Platform(s).

### Targets
1. Providing A Direct Programmable Access To ADB's Client & Server Features without using "ADB.exe".
2. Workaround Google's Bad ADB Protocols Implementation & Documentation.
3. Providing A Clear & Understandble Set Of APIs.
4. Re-working The Entire Protocol Stack for More Optimizations & Efficiency.

### Usage
```csharp
using Texnomic.AdbNet;

AdbClient Client = new AdbClient();

List<Emulator> Emulators = Client.GetEmulators();

await Emulators[0].Root.Enable();

await Emulators[0].Install.Apk("ApkPath");

string Result = await Emulators[0].Shell.Excute("ls");

XmlDocument Document = await Emulators[0].Shell.GetUIXml();

await Emulators[0].Sync.Pull("FilePath");

```