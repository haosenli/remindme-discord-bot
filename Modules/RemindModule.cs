using Discord;
using Discord.Commands;
using System.Text.Json;
using System.Text;
using Newtonsoft.Json;

public class RemindModule : ModuleBase<SocketCommandContext>
{
    private static readonly HttpClient sharedHttpClient = new()
	{
		BaseAddress = new Uri("http://localhost:5149"),
	};

    public RemindModule()
    {
		Task.Run(() => StartReminders());
    }

	public async Task StartReminders()
    {
        TimeSpan interval = TimeSpan.FromSeconds(1);
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        try
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                await CheckRemindersAsync();
                // Delay for the specified interval
                await Task.Delay(interval, cancellationTokenSource.Token);
            }
        }
        catch (TaskCanceledException)
        {
            // Task was canceled, no further action needed
        }
    }

    [Summary("Runs the reminder function.")]
    private async Task CheckRemindersAsync()
    {
		// Run until all current and past reminders are cleared out.
		HttpResponseMessage response = await sharedHttpClient.GetAsync("peek-reminder");
		var jsonResponse = await response.Content.ReadAsStringAsync();
		var responseBody = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);
		while (responseBody != null && 
			   responseBody.ContainsKey("utcTime") && 
			   DateTime.Parse(responseBody["utcTime"]) <= DateTime.UtcNow) 
		{
			// Process response body
			var msg = $"<@{responseBody["authorId"]}> {responseBody["messageContent"]}";
			var messageGuildId = ulong.Parse(responseBody["messageGuildId"]);
			var messageChannelId = ulong.Parse(responseBody["messageChannelId"]);
			var messageChannel = Context.Client.GetChannel(messageChannelId);
			
			// Send message for reminder
			await Context.Client.GetGuild(messageGuildId).GetTextChannel(messageChannelId).SendMessageAsync(msg);
			
			// Remove and process next reminder
			response = await sharedHttpClient.GetAsync("pop-reminder");
			response = await sharedHttpClient.GetAsync("peek-reminder");
			jsonResponse = await response.Content.ReadAsStringAsync();
			responseBody = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);
		}
    }
    
    [Command("rmd")]
    [Summary("Creates a reminder.")]
    public async Task AddReminder([Remainder] string message)
    {
		using StringContent jsonContent = new(
			System.Text.Json.JsonSerializer.Serialize(new {
				authorId = Context.Message.Author.Id.ToString(),
				messageGuildId = Context.Guild.Id.ToString(),
				messageChannelId = Context.Message.Channel.Id.ToString(),
				messageContent = message
			}),
			Encoding.UTF8,
			"application/json");

		using HttpResponseMessage response = await sharedHttpClient.PostAsync("add-reminder", jsonContent);
    }
}