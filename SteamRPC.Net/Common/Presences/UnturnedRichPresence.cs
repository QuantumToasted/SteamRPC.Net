using DiscordRPC;
using Steamworks;
using System;

namespace SteamRPC.Net.Presences
{
    internal sealed class UnturnedRichPresence : SteamRichPresence
    {
        private const string STATUS = "status";

        public UnturnedRichPresence(CSteamID steamId, Assets assets) 
            : base(steamId, assets)
        {
            var status = SteamFriends.GetFriendRichPresence(steamId, STATUS);
            Details = !string.IsNullOrWhiteSpace(status) ? status : "In game";

            if (!LastTimestamp.HasValue || LastPresence?.Equals(this) != true)
            {
                LastTimestamp = DateTime.UtcNow;
            }

            Timestamps = new Timestamps(LastTimestamp.Value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is UnturnedRichPresence other)) return false;
            return Details.Equals(other.Details);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Timestamps.Start.GetValueOrDefault().GetHashCode() * 397) ^ Details.GetHashCode();
            }
        }
    }
}