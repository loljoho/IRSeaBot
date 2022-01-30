using System;

namespace IRSeaBot.Models
{
    public class Reminder : IFileItem
    {
        public string Key { get; set; } = Guid.NewGuid().ToString();
        public string Message { get; set; }
        public string Username { get; set; }
        public DateTime RemindAt { get; set; }

        public DateTime Timesamp { get; set; }

        public string ReplyTo { get; set; }

        public static string FileFolder => Startup.Configuration["RemindersFilePath"];

        public string GetSendMessage(string replyTo)
        {
            return $"PRIVMSG {replyTo} Ok I will remind you to do that at {RemindAt}";
        }

        public string GetReminderMessage()
        {
            return $"PRIVMSG {ReplyTo} Hey {Username}, it is time to {Message}";
        }
    }
}
