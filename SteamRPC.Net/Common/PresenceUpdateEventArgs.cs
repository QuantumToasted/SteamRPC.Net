using System;
using DiscordRPC;

namespace SteamRPC.Net
{
    public sealed class PresenceUpdateEventArgs : EventArgs
    {
        public PresenceUpdateEventArgs(RichPresence presence)
        {
            Presence = presence;
        }

        public RichPresence Presence { get; }
    }
}