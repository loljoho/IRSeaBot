using IRSeaBot.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Data.Entities
{
    public class SeenUser : IBotEntity
    {
        public int Id { get; set; }
        public string User { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }

        public string ReplyTo { get; set; } //channel

        public string GetSendMessage()
        {
            return $"PRIVMSG {ReplyTo} {User} was last seen at {Timestamp} saying {Message}";
        }

        public static string GetNotFoundMessage(string replyTo, string user)
        {
            return $"PRIVMSG {replyTo} I have never seen {user} before.";
        }

        public static SeenUser CreateSeen(string[] input, string replyTo)
        {
            try
            {
                SeenUser user = new()
                {
                    User = input[0],
                    Message = input[1],
                    ReplyTo = replyTo,
                    Timestamp = DateTime.Parse(input[2])
                };
                return user;
            }
            catch
            {
                return null;
            }
        }
    }
}
