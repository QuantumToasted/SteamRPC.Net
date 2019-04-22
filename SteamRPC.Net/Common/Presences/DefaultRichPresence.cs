using System;
using Steamworks;

namespace SteamRPC.Net.Presences
{
    internal sealed class DefaultRichPresence : SteamRichPresence
    {
        public DefaultRichPresence() 
            : base(CSteamID.Nil, null)
        {
            Details = "In game";
            LastTimestamp = LastTimestamp ?? DateTime.UtcNow;
        }

        public override bool Equals(object obj)
            => obj is DefaultRichPresence;

        public override int GetHashCode()
            => int.MinValue;
    }
}