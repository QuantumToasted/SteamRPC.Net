using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SteamRPC.Net.CLI
{
    public sealed class Config
    {
        [JsonProperty("ClientMetadata")]
        private readonly IReadOnlyDictionary<string, RpcClientMetadata> _clientMetadata;

        public Config(int appId)
        {
            try
            {
                JsonConvert.PopulateObject(File.ReadAllText("config.json"), this);

                if (_clientMetadata is null || _clientMetadata.Count == 0)
                {
                    throw new ArgumentException("No RPC client metadata could be found.");
                }

                if (!_clientMetadata.TryGetValue(appId.ToString(), out var client))
                {
                    Logger.Log("No Steam app ID was provided. Defaulting to the first available RPC client.", "Config",
                        ConsoleColor.Yellow);
                }
                else SelectedClient = client;

                if (SteamId == default)
                {
                    throw new ArgumentException("A Steam user ID must be specified.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "Config", ConsoleColor.Red);
                Console.ReadKey();
                Environment.Exit(-1);
            }

            Logger.Log("Loaded configuration file.", "Config", ConsoleColor.Green);
        }

        [JsonIgnore]
        public RpcClientMetadata SelectedClient { get; }

        [JsonProperty("SteamId")]
        public ulong SteamId { get; private set; }
    }

    public sealed class RpcClientMetadata
    {
        [JsonProperty("ClientId")]
        public ulong ClientId { get; private set; }

        [JsonProperty("ImageKey")]
        public string ImageKey { get; private set; }

        [JsonProperty("ImageText")]
        public string ImageText { get; private set; }
    }
}