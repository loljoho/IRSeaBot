﻿using IRSeaBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Services
{
    public class DQFactory
    {
        public static string GetRandomQuote()
        {
            Random rand = new();
            return qualeQuotes[rand.Next(0, 31)];
        }

        private static readonly List<string> qualeQuotes = new List<string>
        {
            "Hawaii has always been a very pivotal role in the Pacific. It is in the Pacific. It is a part of the United States that is an island that is right here.",
            "I was recently on a tour of Latin America, and the only regret I have was that I didn't study Latin harder in school so I could converse with those people.",
            "If we don't succeed, we run the risk of failure.",
            "Republicans understand the importance of bondage between a mother and child.",
            "Welcome to President Bush, Mrs. Bush, and my fellow astronauts.",
            "Mars is essentially in the same orbit... Mars is somewhat the same distance from the Sun, which is very important. We have seen pictures where there are canals, we believe, and water. If there is water, that means there is oxygen. If oxygen, that means we can breathe.",
            "What a waste it is to lose one's mind. Or not to have a mind is being very wasteful. How true that is.",
            "The Holocaust was an obscene period in our nation's history. I mean in this century's history. But we all lived in this century. I didn't live in this century.",
            "I believe we are on an irreversible trend toward more freedom and democracy - but that could change.",
            "One word sums up probably the responsibility of any vice president, and that one word is 'to be prepared'.",
            "May our nation continue to be the beakon of hope to the world.",
            "Verbosity leads to unclear, inarticulate things.",
            "We don't want to go back to tomorrow, we want to go forward.",
            "I have made good judgements in the Past. I have made good judgements in the Future.",
            "The future will be better tomorrow.",
            "We're going to have the best-educated American people in the world.",
            "People that are really very weird can get into sensitive positions and have a tremendous impact on history.",
            "I stand by all the misstatements that I've made.",
            "We have a firm commitment to NATO, we are a *part* of NATO. We have a firm commitment to Europe. We are a *part* of Europe.",
            "Public speaking is very easy.",
            "I am not part of the problem. I am a Republican.",
            "I love California, I practically grew up in Phoenix.",
            "A low voter turnout is an indication of fewer people going to the polls.",
            "When I have been asked during these last weeks who caused the riots and the killing in L.A., my answer has been direct and simple: Who is to blame for the riots? The rioters are to blame. Who is to blame for the killings? The killers are to blame.",
            "Illegitimacy is something we should talk about in terms of not having it.",
            "We are ready for any unforeseen event that may or may not occur.",
            "For NASA, space is still a high priority.",
            "Quite frankly, teachers are the only profession that teach our children.",
            "The American people would not want to know of any misquotes that Dan Quayle may or may not make.",
            "We're all capable of mistakes, but I do not care to enlighten you on the mistakes we may or may not have made.",
            "It isn't pollution that's harming the environment. It's the impurities in our air and water that are doing it.",
            "[It's] time for the human race to enter the solar system."
        };
    }
}
