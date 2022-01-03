using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Models
{
    public class Like : IFileItem
    {
        public string Key { get; set; }
        public int Score { get; set; }

        public static string FileFolder => Startup.Configuration["LikesFilePath"];


        public string GetSendMessage(string replyTo)
        {
            return $"PRIVMSG {replyTo} {Key} has {Score} likes";
        }
    }
}
