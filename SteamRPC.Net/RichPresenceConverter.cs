using System;
using System.IO;
using System.Timers;
using DiscordRPC;
using DiscordRPC.Message;
using SteamRPC.Net.Presences;
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
        private int _elapsedCount;

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
                Interval = 1000
            };

            _imageKey = imageKey;
            _imageText = imageText;
        }

        public DiscordRpcClient Client { get; }

        public User RichPresenceUser { get; private set; }

        public void Initialize()
        {
            SubscribeToEvents();

            if (!Client.Initialize())
                Log?.Invoke($"Could not initialize a Discord IPC connection with application ID {Client.ApplicationID}.",
                    "RPC", LogLevel.Critical);

            Log?.Invoke("Initialized.", "RPC", LogLevel.Info);

            try
            {
                File.WriteAllText("steam_appid.txt", _appId.ToString("D"));
            }
            catch (Exception ex)
            {
                Log?.Invoke($"Could not write out game app ID to file: {ex.Message}", "Steamworks", LogLevel.Critical);
            }

            if (!SteamAPI.Init())
                Log?.Invoke($"Could not initialize a Steamworks connection for app ID {_appId:D}.", "Steamworks",
                    LogLevel.Critical);

            Log?.Invoke("Initialized.", "Steamworks", LogLevel.Info);
        }

        public void Start()
        {
            _timer.Start();

            SteamRichPresence presence;
            switch (_appId)
            {
                case SteamAppId.TF2:
                    presence = new TF2RichPresence(_steamId, new Assets
                    {
                        LargeImageKey = _imageKey,
                        LargeImageText = _imageText
                    });
                    break;
                default:
                    presence = new DefaultRichPresence();
                    break;
            }

            Client.SetPresence(presence);
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Dispose()
        {
            File.Delete("steam_appid.txt");
            Client?.Dispose();
            _timer?.Dispose();
        }

        private void SubscribeToEvents()
        {
            Client.OnReady += HandleClientReady;
            _timer.Elapsed += (_, __) => UpdatePresence();
        }

        private void UpdatePresence()
        {
            if (_elapsedCount <= 10)
            {
                _elapsedCount++;
                return;
            }

            SteamRichPresence presence;
            switch (_appId)
            {
                case SteamAppId.TF2:
                    presence = new TF2RichPresence(_steamId, new Assets
                    {
                        LargeImageKey = _imageKey,
                        LargeImageText = _imageText
                    });
                    break;
                default:
                    presence = new DefaultRichPresence();
                    break;
            }

            var updated = false;
            if (/*presence.State != default && */SteamRichPresence.LastPresence?.State != presence.State)
            {
                StateUpdated?.Invoke(presence.State);
                updated = true;
            }

            if (/*presence.Details != default && */SteamRichPresence.LastPresence?.Details != presence.Details)
            {
                DetailsUpdated?.Invoke(presence.Details);
                updated = true;
            }

            if (/*presence.Party != default && */presence.Party?.Size != SteamRichPresence.LastPresence?.Party?.Size)
            {
                PartyUpdated?.Invoke(presence.Party);
                updated = true;
            }

            if (/*presence.Timestamps?.Start != default && */
                presence.Timestamps?.Start != SteamRichPresence.LastPresence?.Timestamps?.Start)
            {
                TimestampsUpdated?.Invoke(presence.Timestamps);
                updated = true;
            }

            if (updated)
            {
                SteamRichPresence.LastPresence = presence;
            }
        }

        private void HandleClientReady(object sender, ReadyMessage message)
        {
            RichPresenceUser = message.User;
            Log?.Invoke("Ready", "RPC", LogLevel.Info);
            Ready?.Invoke(message);

            if (_autoStart)
                Start();
        }
    }
}
