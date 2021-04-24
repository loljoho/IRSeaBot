using IRSeaBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Services
{
    public static class DQFactory
    {
        public static string GetRandomQuote()
        {
            List<string> dqList = new DQQuotes().Quotes;
            Random rand = new Random();
            return dqList[rand.Next(0, 31)];
        }
    }
}
