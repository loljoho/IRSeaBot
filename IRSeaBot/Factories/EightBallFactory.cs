using IRSeaBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Factories
{
    public class EightBallFactory
    {
        public static string GetRandomReply()
        {
            Random rand = new Random();
            return replies[rand.Next(0, 24)];
        }

        private static readonly List<string> replies = new List<string>
        {
            "As I see it, yes",
            "It is certain",
            "It is decidedly so",
            "Most likely",
            "Outlook good",
            "Indubitably",
            "Signs point to yes",
            "One would be wise to think so",
            "Without a doubt",
            "Yes",
            "Yes - definitely",
            "Yes - and play the lotto too",
            "You may rely on it",
            "Reply hazy, try again",
            "Ask again later",
            "Better not tell you now",
            "Cannot predict now",
            "Concentrate and ask again",
            "Don't count on it",
            "My reply is no",
            "My sources say no",
            "Outlook not so good",
            "Very doubtful",
            "Better luck next time",
            "In your dreams"
        };
    }
}
