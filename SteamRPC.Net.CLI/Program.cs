using System;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using CommandLine;
using DiscordRPC;
using Newtonsoft.Json;

namespace SteamRPC.Net.CLI
{
    public class Program
    {
        private readonly RichPresenceConverter _converter;
        private readonly Timer _timer;
        private RichPresence _currentPresence;

        public Program(string[] args)
        {
            var appId = 440;
            Parser.Default.ParseArguments<RuntimeConfig>(args)
                .WithParsed(x => appId = x.AppId)
                .WithNotParsed(x => { }); // TODO: might not be necessary

            var config = new Config(appId);
            _converter = new RichPresenceConverter(config.SelectedClient.ClientId, config.SteamId, 
                (SteamAppId) appId, imageKey: config.SelectedClient.ImageKey, 
                imageText: config.SelectedClient.ImageText);

            _timer = new Timer
            {
                AutoReset = true,
                Enabled = false,
                Interval = 1000
            };
            _timer.Elapsed += (_, __) => UpdateConsole();
        }

        public async Task InitializeAsync()
        {
            _converter.Log += LogMessage;
            _converter.Ready += HandleReady;
            _converter.PresenceUpdated += HandlePresenceUpdate;

            _timer.Start();
            _converter.Initialize();

            await Task.Delay(-1);
        }

        private void HandlePresenceUpdate(PresenceUpdateEventArgs args)
        {
            _currentPresence = args.Presence;
        }

        private void HandleReady(ReadyEventArgs args)
        {
            Logger.Log($"Client ready for user {args.Message.User}", "RPC", ConsoleColor.Green);
            Logger.Log("Console will be cleared in 10 seconds to start displaying rich presence data.", "RPC", ConsoleColor.Yellow);
            _timer.Start();
        }

        private void LogMessage(LogEventArgs args)
        {
            ConsoleColor color;
            switch (args.Level)
            {
                case LogLevel.Debug:
                    color = ConsoleColor.DarkGray;
                    break;
                case LogLevel.Info:
                    color = ConsoleColor.Green;
                    break;
                case LogLevel.Warning:
                    color = ConsoleColor.Magenta;
                    break;
                case LogLevel.Error:
                    color = ConsoleColor.Yellow;
                    break;
                case LogLevel.Critical:
                    color = ConsoleColor.Red;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Logger.Log(args.Message, args.Source, color);
        }

        private void UpdateConsole()
        {
            if (_currentPresence is null) return;
            var builder = new StringBuilder()
                .AppendLine(_currentPresence.Details);
            if (!string.IsNullOrWhiteSpace(_currentPresence.State))
                builder.AppendLine(
                    $"{_currentPresence.State} ({_currentPresence.Party.Size} of {_currentPresence.Party.Max})");

            if (_currentPresence.Timestamps.Start.HasValue)
            {
                var elapsed = DateTime.UtcNow - _currentPresence.Timestamps.Start.Value;
                if (elapsed.Hours > 0)
                    builder.Append(elapsed.Hours.ToString("0#:"));
                builder.Append(elapsed.Minutes.ToString("0#:"))
                    .Append(elapsed.Seconds.ToString("0#"))
                    .AppendLine(" elapsed")
                    .AppendLine();
            }

            builder.AppendLine("Current presence:")
                .AppendLine(JsonConvert.SerializeObject(_currentPresence, Formatting.Indented));

            Console.Clear();
            Console.WriteLine(builder.ToString());
        }

        public static void Main(string[] args)
            => new Program(args).InitializeAsync().GetAwaiter().GetResult();
    }
}
