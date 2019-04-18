using System;
using DiscordRPC.Message;

namespace SteamRPC.Net
{
    public sealed class ReadyEventArgs : EventArgs
    {
        public ReadyEventArgs(ReadyMessage message)
        {
            Message = message;
        }

        public ReadyMessage Message { get; }
    }
}