// See https://aka.ms/new-console-template for more information

using Discord;
using Discord.Net;
using Discord.WebSocket;
using DiscordBot;
using Newtonsoft.Json;

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
        client.Ready += ClientReady;
        client.SlashCommandExecuted += SlashCommandHandler;


        // Block this task until the program is closed.
        await Task.Delay(-1);
    }
    
    private static Task Log(LogMessage msg) {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    public static async Task ClientReady() {
        var guild = client.GetGuild(ulong.Parse(Environment.GetEnvironmentVariable("DEV_GUILD")?? throw new Exception("Cannot find guild id in ENV")));
        
        var pingCommand = new SlashCommandBuilder();
        pingCommand.WithName("ping");
        pingCommand.WithDescription("This pings the bot and returns a pong message!");

        var listRolesCommand = new SlashCommandBuilder()
                .WithName("list-roles")
                .WithDescription("List all the roles of a given user")
                .AddOption("user", ApplicationCommandOptionType.User, "The users you want to list the roles of", isRequired: true)
        ;

        try {
            await client.CreateGlobalApplicationCommandAsync(pingCommand.Build());
            await client.Rest.CreateGlobalCommand(listRolesCommand.Build());
            await guild.CreateApplicationCommandAsync(listRolesCommand.Build());
        }
        catch (HttpException exception) {
            // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

            // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
            Console.WriteLine(json);
        }

    }

    public static async Task SlashCommandHandler(SocketSlashCommand command) {
        switch (command.CommandName) {
            case "ping":
                await command.RespondAsync("Pong! :ping_pong:");
                break;
            case "list-roles":
                await HandleListRoleCommand(command);
                break;
            default:
                await command.RespondAsync($"You executed {command.Data.Name}");
                break;
        }
    }

    public static async Task HandleListRoleCommand(SocketSlashCommand command) {
        var guildUser = (SocketGuildUser)command.Data.Options.First().Value;
        
        var roleList = string.Join(",\n", guildUser.Roles.Where(x => !x.IsEveryone).Select(x => x.Mention));

        var embedBuiler = new EmbedBuilder()
            .WithAuthor(guildUser.DisplayName, guildUser.GetAvatarUrl() ?? guildUser.GetDefaultAvatarUrl())
            .WithTitle("Roles")
            .WithDescription(roleList)
            .WithColor(new Color(0, 255, 188))
            .WithCurrentTimestamp()
        ;
        await command.RespondAsync(embed: embedBuiler.Build());
    }

}