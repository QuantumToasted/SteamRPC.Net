using System;
using DiscordRPC;
using Steamworks;

namespace SteamRPC.Net.Presences
{
    internal abstract class SteamRichPresence : RichPresence
    {
        protected SteamRichPresence(CSteamID steamId, Assets assets)
        {
            SteamId = steamId;
            Assets = assets;
        }

        public static SteamRichPresence LastPresence { get; internal set; }
        
        public static DateTime? LastTimestamp { get; protected set; }

        public CSteamID SteamId { get; }

        public abstract override bool Equals(object obj);

        public abstract override int GetHashCode();
    }
}