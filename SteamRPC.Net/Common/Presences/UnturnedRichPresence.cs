using DiscordRPC;
using Steamworks;

namespace SteamRPC.Net.Presences
{
    internal sealed class UnturnedRichPresence : SteamRichPresence
    {
        private const string STATUS = "status";

        public UnturnedRichPresence(CSteamID steamId, Assets assets) : base(steamId, assets)
        {
            var status = SteamFriends.GetFriendRichPresence(steamId, STATUS);

            Details = !string.IsNullOrWhiteSpace(status) ? status : "In game";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is UnturnedRichPresence other)) return false;
            return Details?.Equals(other.Details) != false;
        }

        public override int GetHashCode()
        {
            return Details.GetHashCode();
        }
    }
}