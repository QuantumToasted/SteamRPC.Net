using CommandLine;

namespace SteamRPC.Net.CLI
{
    public sealed class RuntimeConfig
    {
        [Option('g', "game", Default = 440, HelpText = "Steam app ID for the game you wish to track rich presence for.")]
        public int AppId { get; set; }
    }
}