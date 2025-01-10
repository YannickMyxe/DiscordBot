using Discord.Commands;
using Discord.WebSocket;

namespace DiscordBot.Commands;

public class CommandHandler {
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;

    public CommandHandler(DiscordSocketClient client, CommandService commands) {
        _client = client;
        _commands = commands;
    }

    public async Task InstallCommandsAsync() {
        _client.MessageReceived += HandleCommandAsync;
    }

    public async Task HandleCommandAsync(SocketMessage messageParam) {
        // Don't process the command if it was a system message
        var message = messageParam as SocketUserMessage;
        if (message == null) return;

        int argPos = 0;

        if (
            !(message.HasCharPrefix('!', ref argPos) || 
              message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            || message.Author.IsBot)
            return;
        
        var context = new SocketCommandContext(_client, message);

        await _commands.ExecuteAsync(
            context: context,
            argPos: argPos,
            services: null
        );
    }
}