using System;

namespace SteamRPC.Net
{
    public sealed class LogEventArgs : EventArgs
    {
        public LogEventArgs(string message, string source, LogLevel level)
        {
            Message = message;
            Source = source;
            Level = level;
        }

        public string Message { get; }

        public string Source { get; }

        public LogLevel Level { get; }
    }
}