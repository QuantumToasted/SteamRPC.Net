using DiscordRPC.Message;

namespace SteamRPC.Net
{
    public sealed partial class RichPresenceConverter
    {
        public delegate void ReadyEvent(ReadyEventArgs args);
        public event ReadyEvent Ready;
        private void HandleReady(object sender, ReadyMessage message)
        {
            RichPresenceUser = message.User;
            LogMessage("Ready", "RPC", LogLevel.Info);
            Ready?.Invoke(new ReadyEventArgs(message));

            if (_autoStart) 
                Start();
        }

        public delegate void PresenceUpdateEvent(PresenceUpdateEventArgs args);
        public event PresenceUpdateEvent PresenceUpdated;
        private void HandlePresenceUpdate(object sender, PresenceMessage message)
        {
            if (message.Presence is null) return;
            CurrentRichPresence = message.Presence;
            PresenceUpdated?.Invoke(new PresenceUpdateEventArgs(message.Presence));
        }

        public delegate void LogEvent(LogEventArgs args);
        public event LogEvent Log;
        private void LogMessage(string value, string source, LogLevel level)
        {
            Log?.Invoke(new LogEventArgs(value, source, level));
        }

        private void SubscribeToEvents()
        {
            Client.OnReady += HandleReady;
            Client.OnPresenceUpdate += HandlePresenceUpdate;
            _timer.Elapsed += (_, __) => UpdatePresence();
        }
    }
}
