using System.IO;
using Microsoft.Extensions.Configuration;

// Discord namespaces
using Discord;
using Discord.WebSocket;

class Program
{
 
	private DiscordSocketClient? _client;

	public static Task Main(string[] args) => new Program().MainAsync();

	public async Task MainAsync()
	{
		_client = new DiscordSocketClient();

		_client.Log += Log;

		// Retrieve bot token from settings.json
		string settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
		IConfiguration configuration = new ConfigurationBuilder()
			.AddJsonFile(settingsPath, optional: false, reloadOnChange: true)
			.Build();
		var botToken = configuration.GetSection("botCredentials:token").Value;

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
