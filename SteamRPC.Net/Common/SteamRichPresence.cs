using System;
using DiscordRPC;
using Steamworks;

namespace SteamRPC.Net
{
    internal sealed class SteamRichPresence
    {
        private const string CONNECT = "connect";
        private const string STATE = "state";
        private const string STEAM_DISPLAY = "steam_display";
        private const string STATUS = "status";
        private const string MATCH_GROUP_LOCATION = "matchgrouploc";
        private const string STEAM_PLAYER_GROUP = "steam_player_group";
        private const string STEAM_PLAYER_GROUP_SIZE = "steam_player_group_size";
        private const string CURRENT_MAP = "currentmap";

        private static DateTime? _timestamp;
        private readonly CSteamID _steamId;
        private readonly string _imageKey;
        private readonly string _imageText;

        internal SteamRichPresence(CSteamID steamId, string imageKey = null, string imageText = null)
        {
            _steamId = steamId;
            _imageKey = imageKey;
            _imageText = imageText;

            var connect = SteamFriends.GetFriendRichPresence(steamId, CONNECT);
            ConnectCommand = !string.IsNullOrWhiteSpace(connect)
                ? connect
                : default;

            var state = SteamFriends.GetFriendRichPresence(steamId, STATE);
            State = Enum.TryParse(state, true, out RichPresenceState rpState)
                ? rpState
                : default;

            var steamDisplay = SteamFriends.GetFriendRichPresence(steamId, STEAM_DISPLAY);
            SteamDisplay = !string.IsNullOrWhiteSpace(steamDisplay)
                ? steamDisplay
                : default;

            var status = SteamFriends.GetFriendRichPresence(steamId, STATUS);
            Status = !string.IsNullOrWhiteSpace(status)
                ? status
                : default;

            var location = SteamFriends.GetFriendRichPresence(steamId, MATCH_GROUP_LOCATION);
            Location = Enum.TryParse(location, true, out MatchGroupLocation rpLocation)
                ? rpLocation
                : default;

            var group = SteamFriends.GetFriendRichPresence(steamId, STEAM_PLAYER_GROUP);
            Group = !string.IsNullOrWhiteSpace(group)
                ? group
                : default;

            var groupSize = SteamFriends.GetFriendRichPresence(steamId, STEAM_PLAYER_GROUP_SIZE);
            GroupSize = int.TryParse(groupSize, out var size)
                ? size
                : default;

            var currentMap = SteamFriends.GetFriendRichPresence(steamId, CURRENT_MAP);
            CurrentMap = !string.IsNullOrWhiteSpace(currentMap)
                ? currentMap
                : default;
        }

        internal string ConnectCommand { get; }

        internal RichPresenceState State { get; }

        internal string SteamDisplay { get; }

        internal string Status { get; }

        internal MatchGroupLocation Location { get; }

        internal string Group { get; }

        internal int? GroupSize { get; }

        internal string CurrentMap { get; }

        public override bool Equals(object obj)
        {
            if (!(obj is SteamRichPresence other)) return false;
            return _steamId == other._steamId &&
                   State.Equals(other.State) &&
                   Status?.Equals(other.Status) != false &&
                   Location.Equals(other.Location) &&
                   Group?.Equals(other.Group) != false &&
                   CurrentMap?.Equals(other.CurrentMap) != false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)State;
                hashCode = (hashCode * 397) ^ (Status != null ? Status.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)Location;
                hashCode = (hashCode * 397) ^ (Group != null ? Group.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CurrentMap != null ? CurrentMap.GetHashCode() : 0);
                return hashCode;
            }
        }

        private string FormatDetails(string mapName)
        {
            string details;
            var map = !string.IsNullOrWhiteSpace(mapName)
                ? $"({mapName})"
                : string.Empty;

            switch (State)
            {
                case RichPresenceState.None:
                    return "In game";
                case RichPresenceState.PlayingCommunity:
                    details = $"In a community server {map}";
                    break;
                case RichPresenceState.LoadingCommunity:
                    details = "Loading into a community server";
                    break;
                case RichPresenceState.MainMenu:
                    details = "At the main menu";
                    break;
                case RichPresenceState.SearchingMatchGroup:
                    details = $"Searching for a{FormatLocation()} match";
                    break;
                case RichPresenceState.LoadingMatchGroup:
                    details = $"Loading into a{FormatLocation()} match";
                    break;
                case RichPresenceState.PlayingMatchGroup:
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
                    case MatchGroupLocation.None:
                        return string.Empty;
                    case MatchGroupLocation.Casual:
                        return " Casual 12v12";
                    case MatchGroupLocation.Competitive6v6:
                        return " Competitive 6v6";
                    case MatchGroupLocation.BootCamp:
                        return "n MvM Boot Camp";
                    case MatchGroupLocation.MannUp:
                        return "n MvM Mann Up";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static SteamRichPresence LastPresence { get; private set; }

        public static implicit operator RichPresence(SteamRichPresence steamPresence)
        {
            var presence = new RichPresence();

            if (!string.IsNullOrWhiteSpace(steamPresence._imageKey) &&
                !string.IsNullOrWhiteSpace(steamPresence._imageText))
            {
                presence.Assets = new Assets
                {
                    LargeImageKey = steamPresence._imageKey,
                    LargeImageText = steamPresence._imageText
                };
            }

            if (!_timestamp.HasValue || LastPresence?.Equals(steamPresence) != true)
            {
                _timestamp = DateTime.UtcNow;
            }

            presence.Timestamps = new Timestamps(_timestamp.Value);
            presence.Details = steamPresence.FormatDetails(steamPresence.CurrentMap);

            if (steamPresence.GroupSize.HasValue &&
                (steamPresence.State == RichPresenceState.PlayingMatchGroup ||
                 steamPresence.State == RichPresenceState.SearchingMatchGroup ||
                 steamPresence.State == RichPresenceState.LoadingMatchGroup))
            {
                presence.State = steamPresence.GroupSize.Value == 1 ? "By themselves" : "In a party";

                presence.Party = new Party
                {
                    ID = steamPresence.Group,
                    Max = 6,
                    Size = steamPresence.GroupSize.Value
                };
            }

            LastPresence = steamPresence;
            return presence;
        }
    }
}