using Discord;
using Discord.Commands;
using RunMode = Discord.Commands.RunMode;

public class ExampleCommand : ModuleBase<SocketCommandContext>
{
    public CommandService? CommandService { get; set; }

    [Command("hello", RunMode = RunMode.Async)]
    public async Task Hello()
    {
        // await Context.Message.Channel.SendMessageAsync
        await Context.Message.ReplyAsync($"Hello {Context.User.Username}. Nice to meet you!");
    }
}