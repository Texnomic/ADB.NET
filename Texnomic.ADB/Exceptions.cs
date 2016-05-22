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
}
