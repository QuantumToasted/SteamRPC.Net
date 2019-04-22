using System;
using DiscordRPC;
using Steamworks;

namespace SteamRPC.Net.Presences
{
    internal sealed class TF2RichPresence : SteamRichPresence
    {
        private const string CONNECT = "connect";
        private const string STATE = "state";
        private const string STEAM_DISPLAY = "steam_display";
        private const string STATUS = "status";
        private const string MATCH_GROUP_LOCATION = "matchgrouploc";
        private const string STEAM_PLAYER_GROUP = "steam_player_group";
        private const string STEAM_PLAYER_GROUP_SIZE = "steam_player_group_size";
        private const string CURRENT_MAP = "currentmap";

        public TF2RichPresence(CSteamID steamId, Assets assets) 
            : base(steamId, assets)
        {
            var currentMap = SteamFriends.GetFriendRichPresence(steamId, CURRENT_MAP);
            CurrentMap = !string.IsNullOrWhiteSpace(currentMap)
                ? currentMap
                : default;

            var presenceState = SteamFriends.GetFriendRichPresence(steamId, STATE);
            PresenceState = Enum.TryParse(presenceState, true, out TF2PresenceState state)
                ? state
                : default;

            var group = SteamFriends.GetFriendRichPresence(steamId, STEAM_PLAYER_GROUP);
            group = !string.IsNullOrWhiteSpace(group)
                ? group
                : $"tf2_{steamId.m_SteamID}";

            var location = SteamFriends.GetFriendRichPresence(steamId, MATCH_GROUP_LOCATION);
            Location = Enum.TryParse(location, true, out TF2MatchGroupLocation groupLocation)
                ? groupLocation
                : default;

            var groupSize = SteamFriends.GetFriendRichPresence(steamId, STEAM_PLAYER_GROUP_SIZE);
            if (int.TryParse(groupSize, out var size) && 
                (PresenceState == TF2PresenceState.SearchingMatchGroup ||
                 PresenceState == TF2PresenceState.LoadingMatchGroup ||
                 PresenceState == TF2PresenceState.PlayingMatchGroup ||
                 PresenceState == TF2PresenceState.MainMenu))
            {
                State = "In a party";

                Party = new Party
                {
                    ID = group,
                    Size = size,
                    Max = 6
                };
            }

            Details = FormatDetails();

            if (!LastTimestamp.HasValue || LastPresence?.Equals(this) != true)
            {
                LastTimestamp = DateTime.UtcNow;
            }
            
            Timestamps = new Timestamps(LastTimestamp.Value);
        }

        public TF2PresenceState PresenceState { get; }

        public TF2MatchGroupLocation Location { get; }

        public string CurrentMap { get; }

        public override bool Equals(object obj)
        {
            if (!(obj is TF2RichPresence other)) return false;
            return PresenceState.Equals(other.PresenceState) &&
                   Location.Equals(other.Location) &&
                   CurrentMap?.Equals(other.CurrentMap) != false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) PresenceState;
                hashCode = (hashCode * 397) ^ (int) Location;
                hashCode = (hashCode * 397) ^ (CurrentMap != null ? CurrentMap.GetHashCode() : 0);
                return hashCode;
            }
        }

        private string FormatDetails()
        {
            string details;
            var map = !string.IsNullOrWhiteSpace(CurrentMap)
                ? $"\n({CurrentMap})"
                : string.Empty;

            switch (PresenceState)
            {
                case TF2PresenceState.None:
                    return "In game";
                case TF2PresenceState.PlayingCommunity:
                    details = $"In a community server {map}";
                    break;
                case TF2PresenceState.LoadingCommunity:
                    details = "Loading into a community server";
                    break;
                case TF2PresenceState.MainMenu:
                    details = "At the main menu";
                    break;
                case TF2PresenceState.SearchingMatchGroup:
                    details = $"Searching for a{FormatLocation()} match";
                    break;
                case TF2PresenceState.LoadingMatchGroup:
                    details = $"Loading into a{FormatLocation()} match";
                    break;
                case TF2PresenceState.PlayingMatchGroup:
                    details = $"In a{FormatLocation()} match {map}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return details.Trim();

            string FormatLocation()
            {
                switch (Location)
                {
                    case TF2MatchGroupLocation.None:
                        return string.Empty;
                    case TF2MatchGroupLocation.Casual:
                        return " Casual 12v12";
                    case TF2MatchGroupLocation.Competitive6v6:
                        return " Competitive 6v6";
                    case TF2MatchGroupLocation.BootCamp:
                        return "n MvM Boot Camp";
                    case TF2MatchGroupLocation.MannUp:
                        return "n MvM Mann Up";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}