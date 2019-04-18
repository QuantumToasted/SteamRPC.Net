using System;
using System.IO;
using System.Timers;
using DiscordRPC;
using Steamworks;

namespace SteamRPC.Net
{
    public sealed partial class RichPresenceConverter : IDisposable
    {
        private readonly CSteamID _steamId;
        private readonly SteamAppId _appId;
        private readonly bool _autoStart;
        private readonly Timer _timer;
        private readonly string _imageKey;
        private readonly string _imageText;

        public RichPresenceConverter(ulong rpcClientId, ulong steamUserId, 
            SteamAppId appId = SteamAppId.TF2, bool autoStart = true,
            string imageKey = null, string imageText = null)
        {
            Client = new DiscordRpcClient(rpcClientId.ToString(), autoEvents: true);

            _steamId = new CSteamID(steamUserId);

            _appId = appId;
            _autoStart = autoStart;

            _timer = new Timer
            {
                AutoReset = true,
                Enabled = false,
                Interval = 10000
            };

            _imageKey = imageKey;
            _imageText = imageText;
        }

        public DiscordRpcClient Client { get; }

        public User RichPresenceUser { get; private set; }

        public RichPresence CurrentRichPresence { get; private set; }

        public void Initialize()
        {
            SubscribeToEvents();

            if (!Client.Initialize())
                LogMessage(
                    $"Could not initialize a Discord IPC connection with application ID {Client.ApplicationID}.",
                    "RPC", LogLevel.Critical);

            LogMessage("Initialized.", "RPC", LogLevel.Info);

            try
            {
                File.WriteAllText("steam_appid.txt", _appId.ToString("D"));
            }
            catch (Exception ex)
            {
                LogMessage($"Could not write out game app ID to file: {ex.Message}", "Steamworks", LogLevel.Critical);
            }

            if (!SteamAPI.Init())
                LogMessage($"Could not initialize a Steamworks connection for app ID {_appId:D}.", "Steamworks",
                    LogLevel.Critical);

            LogMessage("Initialized.", "Steamworks", LogLevel.Info);
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void UpdatePresence()
        {
            Client.SetPresence(new SteamRichPresence(_steamId, _imageKey, _imageText));
        }

        public void Dispose()
        {
            File.Delete("steam_appid.txt");
            Client?.Dispose();
            _timer?.Dispose();
        }
    }
}
