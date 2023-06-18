using System.Text.RegularExpressions;
using Discord.WebSocket;

namespace ReminderUtilities
{
    /// <summary>
    /// Parses and encapsulates data for reminders.
    /// <summary>
    public class Reminder
    {
        public readonly ulong authorId;
        public readonly ISocketMessageChannel messageChannel;
        public readonly string messageContent;
        public readonly DateTime utcTime;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Reminder"/> class from the SocketUserMessage.
        /// If the time units in the message is unsupported, the messageContent will be overwritten by an
        /// Exception message, and the utcTime will be set to the current time. This is done so that the
        /// Reminder will be sent to the Discord user immediately for them to correct their reminder.
        /// </summary>
        /// <param name="message">The Discord SocketUserMessage.</param>
        public Reminder(SocketUserMessage message)
        {
            // Resource on mentioning users:
            // https://stackoverflow.com/questions/48855693/discord-net-how-to-make-bot-ping-users-with
            authorId = message.Author.Id;
            messageChannel = message.Channel;
            try
            {
                (utcTime, messageContent) = parseMessage(message.Content);
            }
            catch (NotSupportedException ex)
            {
                utcTime = DateTime.UtcNow;
                messageContent = ex.Message;
            }
        }

        /// <summary>
        /// Parses a SocketUserMessage from Discord.
        /// </summary>
        /// <param name="messageContent"> A Discord SocketUserMessage.</param>
        /// <returns>A DateTime and String parsed from the message.</returns>
        /// <exception cref="NotSupportedException">Thrown when a time unit is unsupported.</exception>
        private (DateTime, String) parseMessage(string messageContent)
        {
            string[] messageParts = messageContent.Split(' ');

            // Tokenize the raw time input
            string rawTime = messageParts[0];
            string[] timeValues = SplitTime(rawTime);
            int timeOffset = 0;

            // Calculate total time offset
            foreach (string timeValue in timeValues)
            {
                int splitIndex = timeValue.Length - 1;
                int timeAmount = int.Parse(timeValue[0 .. splitIndex]);
                char timeUnit = char.ToLower(timeValue[splitIndex]);

                switch (timeUnit)
                {
                    case 'w':
                        timeOffset += timeAmount * 604800;
                        break;

                    case 'd':
                        timeOffset += timeAmount * 86400;
                        break;
                    
                    case 'h':
                        timeOffset += timeAmount * 3600;
                        break;

                    case 'm':
                        timeOffset += timeAmount * 60;
                        break;
                    
                    case 's':
                        timeOffset += timeAmount;
                        break;

                    default:
                        throw new NotSupportedException("Time unit '' not supported. Only w (weeks), d (days), h (hours), m (minutes), s (seconds) are allowed.");
                }
            }

            // Calculate time in UTC
            DateTime utcTime = DateTime.UtcNow.AddSeconds(timeOffset);

            // Parse reminder message
            string content = String.Join(" ", messageParts[1 ..]);
            return (utcTime, content);
        }

        /// <summary>
        /// Splits a string time input into an array of time.
        /// </summary>
        /// <param name="input">The string input representing a time.</param>
        /// <returns>An array of string times.</returns>
        static string[] SplitTime(string input)
        {
            string pattern = @"\d+[wdhmsWDHMS]";
            return Regex.Matches(input, pattern)
                        .Cast<Match>()
                        .Select(match => match.Value)
                        .ToArray();
        }
    }
}
