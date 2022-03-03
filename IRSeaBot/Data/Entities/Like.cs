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

        public string GetSendMessage(string replyTo)
        {
            return $"PRIVMSG {replyTo} {Key} has {Score} likes";
        }

        public static Like CreateLike(string[] input)
        {
            try
            {
                Like like = new()
                {
                    Key = input[0],
                    Score = int.Parse(input[1]),
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
