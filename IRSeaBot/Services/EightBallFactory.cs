using IRSeaBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Services
{
    public class EightBallFactory
    {
        public static string GetRandomReply()
        {
            List<string> ball = new EightBall().Replies;
            Random rand = new Random();
            return ball[rand.Next(0, 22)];
        }
    }
}
