using Microsoft.Extensions.Configuration;

// Discord namespaces
using Discord;
using Discord.WebSocket;
using Discord.Commands;

class Program
{
 
	private DiscordSocketClient? _client;

	public static Task Main(string[] args) => new Program().MainAsync();

	public async Task MainAsync()
	{
		var config = new DiscordSocketConfig() 
		{
			GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
		};

		_client = new DiscordSocketClient(config);

		_client.Log += Log;

		// Retrieve bot token from settings.json
		string settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
		IConfiguration configuration = new ConfigurationBuilder()
			.AddJsonFile(settingsPath, optional: false, reloadOnChange: true)
			.Build();
		var botToken = configuration.GetSection("botCredentials:token").Value;

		// Create instances of CommandService and CommandHandler
		var commands = new CommandService();
		var commandHandler = new CommandHandler(_client, commands);

		// Install and set up command handling
		await commandHandler.InstallCommandsAsync();

		// Start Bot
		await _client.LoginAsync(TokenType.Bot, botToken);
		await _client.StartAsync();

		// Block this task until the program is closed.
		await Task.Delay(-1);
	}

    private Task Log(LogMessage msg)
    {
		Console.WriteLine(msg.ToString());
		return Task.CompletedTask;
    }
}
