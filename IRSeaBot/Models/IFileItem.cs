using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Models
{
    public interface IFileItem
    {
        public string Key { get; set; }
        public string GetSendMessage(string replyTo);
        public static string FileFolder {get;}
    }

    public enum FileTypes
    {
        Seen,
        Likes,
        Reminders
    }
}
