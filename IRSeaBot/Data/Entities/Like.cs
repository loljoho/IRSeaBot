using IRSeaBot.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Data.Entities
{
    public class Like : IBotEntity
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public int Score { get; set; }

        public string ReplyTo { get; set; } //channel

        public string GetSendMessage()
        {
            return $"PRIVMSG {ReplyTo} {Key} has {Score} likes";
        }

        public static string GetNotFoundMessage(string replyTo, string key)
        {
            return $"PRIVMSG {replyTo} {key} has 0 likes";
        }

        public static Like CreateLike(string[] input, string replyTo)
        {
            try
            {
                Like like = new()
                {
                    Key = input[0],
                    Score = int.Parse(input[1]),
                    ReplyTo = replyTo,
                };
                return like;
            }
            catch
            {
                return null;
            }
        }
    }
}
