using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Models
{
    public class SeenUser : IFileItem
    {
        public string Key { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }

        public static string FileFolder => Startup.Configuration["SeenFilePath"];

        public string GetSendMessage(string replyTo)
        {
            return $"PRIVMSG {replyTo} {Key} was last seen at {Timestamp} saying {Message}";
        }
    }
}
