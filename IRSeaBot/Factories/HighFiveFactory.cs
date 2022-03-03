using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Factories
{
    public class HighFiveFactory
    {
        public static string GetHighFive(string nick, string user)
        {
            Random rand = new Random();
            string hi5 = string.Empty;
            switch (rand.Next(13))
            {
                case 0:
                    hi5 = $"{nick} offers a fist and {user} pounds it";
                    break;
                case 1:
                    hi5 = $"{nick} tries to give {user} a five up high but misses. that was awkward";
                    break;
                case 2:
                    hi5 = $"{nick} gives {user} a killer high-five";
                    break;
                case 3:
                    hi5 = $"{nick} smashes {user} up high";
                    break;
                case 4:
                    hi5 = $"{nick} slaps skin with {user}";
                    break;
                case 5:
                    hi5 = $"{nick} {user} winds up for a killer five but misses and falls flat on their face";
                    break;
                case 6:
                    hi5 = "{nick} halfheartedly high-fives {user}";
                    break;
                case 7:
                    hi5 = $"{nick} gives {user} a smooth five down low";
                    break;
                case 8:
                    hi5 = $"{nick} gives {user} a friendly high five";
                    break;
                case 9:
                    hi5 = $"{nick} starts to give {user} a high five, but leaves them hanging";
                    break;
                case 10:
                    hi5 = $"{nick} performs an incomprehensible handshake with {user} that identifies them as the very best of friends";
                    break;
                case 11:
                    hi5 = $"{nick} makes as if to high five {user} but pulls their hand away at the last second";
                    break;
                case 12:
                    hi5 = $"{nick} leaves {user} hanging";
                    break;
            };
            return hi5;
        }
    }
}
