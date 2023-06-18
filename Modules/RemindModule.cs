using Discord.Commands;
using ReminderUtilities;

public class RemindModule : ModuleBase<SocketCommandContext>
{
    private ReminderQueue reminders;

    public RemindModule()
    {
        reminders = new ReminderQueue();
    }

	[Command("rmdstart", RunMode = RunMode.Async)]
    [Summary("Starts the reminder function.")]
	public void StartReminders()
	{
		// Timer object to periodically call CheckRemindersAsync
		System.Timers.Timer reminderTimer = new System.Timers.Timer(5000);
        reminderTimer.Elapsed += async (sender, e) => await CheckRemindersAsync();
        reminderTimer.Start();
	}

	
	[Command("checkrmd")]
    [Summary("Runs the reminder function.")]
    private async Task CheckRemindersAsync()
    {
		// Run until all current and past reminders are cleared out.
		Reminder? reminder = reminders.peekReminder();

		Console.WriteLine(reminder);
		while (reminder != null && reminder.utcTime <= DateTime.UtcNow) {
			Console.WriteLine("here2");
			// Send reminder
			string msg = $"<@{reminder.authorId}> {reminder.messageContent}";
			await reminder.messageChannel.SendMessageAsync(msg);
			// Get new reminder
			reminder = reminders.peekReminder();
		}
    }
    
    [Command("rmd")]
    [Summary("Creates a reminder.")]
    public async Task AddReminder([Remainder] string message)
    {
		Console.WriteLine("new reminder added");
        reminders.addReminder(Context.Message);
		Console.WriteLine(reminders.peekReminder());
    }
}