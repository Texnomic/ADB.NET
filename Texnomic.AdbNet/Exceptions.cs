using System;

namespace Texnomic.AdbNet
{
    public class ConnectionFailedException : Exception
    {
        public ConnectionFailedException() : base("Connection Failed To ADB Server.") { }
    }
    public class UnexpectedMessageException : Exception
    {
        public UnexpectedMessageException() : base("Unexpected Message Received.") { }
    }
    public class WrongMessageLengthException : Exception
    {
        public WrongMessageLengthException() : base("Wrong Message Length.") { }
    }
    public class IncompleteMessageException : Exception
    {
        public IncompleteMessageException() : base("Incomplete Message Received.") { }
    }
    public class AdbServerNotRunningException : Exception
    {
        public AdbServerNotRunningException() : base("ADB Server Not Running.") { }
    }
    public class CommandFailedException : Exception
    {
        public CommandFailedException() : base("Command Failed.") { }
    }
        public class SdkNotFoundException : Exception
    {
        public SdkNotFoundException() : base("Android SDK Not Installed or ANDROID_HOME Environment Variable Not Set.") { }
    }
    public class AdbNotFoundException : Exception
    {
        public AdbNotFoundException() : base("ADB.exe Not Found In Android SDK.") { }
    }
    public class UnableToStartAdbServerException : Exception
    {
        public UnableToStartAdbServerException() : base("Unable To Start ADB Server.") { }
    }
}
