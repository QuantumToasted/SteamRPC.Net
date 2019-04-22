using System;
using DiscordRPC;
using DiscordRPC.Message;
using SteamRPC.Net.Presences;

namespace SteamRPC.Net
{
    public sealed partial class RichPresenceConverter
    {
        public delegate void ReadyEvent(ReadyMessage message);
        public event ReadyEvent Ready;
        
        public delegate void DetailsUpdateEvent(string details);
        public event DetailsUpdateEvent DetailsUpdated;

        public delegate void StateUpdateEvent(string state);
        public event StateUpdateEvent StateUpdated;

        public delegate void PartyUpdateEvent(Party party);
        public event PartyUpdateEvent PartyUpdated;

        public delegate void AssetsUpdateEvent(Assets assets);
        public event AssetsUpdateEvent AssetsUpdated;

        public delegate void TimestampsUpdateEvent(Timestamps timestamps);
        public event TimestampsUpdateEvent TimestampsUpdated;

        public delegate void LogEvent(string value, string source, LogLevel level);
        public event LogEvent Log;
    }
}
