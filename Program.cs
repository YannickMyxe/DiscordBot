// See https://aka.ms/new-console-template for more information

using Discord;
using Discord.WebSocket;
using DiscordBot;

class Program {
    
    private static DiscordSocketClient client;
    
    public static async Task Main() {
        client = new DiscordSocketClient();
        client.Log += Log;
        
        var path = "../../../.env";
        EnvReader.Load(path);
        
        var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        if (token is null) throw new Exception("Token is missing");
        
        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        // Block this task until the program is closed.
        await Task.Delay(-1);
    }
    
    private static Task Log(LogMessage msg) {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

}