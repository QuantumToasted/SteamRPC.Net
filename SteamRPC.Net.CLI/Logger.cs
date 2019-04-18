using System;

namespace SteamRPC.Net.CLI
{
    public static class Logger
    {
        public static void Log(object obj, string source, ConsoleColor color = ConsoleColor.Gray)
        {
            var message = obj?.ToString();
            if (string.IsNullOrWhiteSpace(message)) return;

            foreach (var line in message.Split(new []{"\n"}, StringSplitOptions.RemoveEmptyEntries))
            {
                Console.Write(source.PadRight(10) + " | ");
                Console.ForegroundColor = color;
                Console.WriteLine(line);
                Console.ResetColor();
            }
        }
    }
}