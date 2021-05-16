using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Services
{
    public class FightFactory
    {
        public static string GetFight(string user)
        {
            Random rand = new Random();
            int templateNum = rand.Next(14);
            string template = String.Empty;
            switch (templateNum)
            {
                case 0:
                    template = $"ties {user} to a pole and whips them with a {items[rand.Next(items.Count)]}.";
                    break;
                case 1:
                    template = $"{hits[rand.Next(hits.Count)]} {user} with a {items[rand.Next(items.Count)]}.";
                    break;
                case 2:
                    template = $"{hits[rand.Next(hits.Count)]} {user} around a bit with a {items[rand.Next(items.Count)]}.";
                    break;
                case 3:
                    template = $"{throws[rand.Next(throws.Count)]} a {items[rand.Next(items.Count)]} at {user}.";
                    break;
                case 4:
                    template = $"{throws[rand.Next(throws.Count)]} a few {pluralItems[rand.Next(pluralItems.Count)]} at {user}.";
                    break;
                case 5:
                    template = $"grabs a {items[rand.Next(items.Count)]} and {throws[rand.Next(throws.Count)]} it in {user}'s face.";
                    break;
                case 6:
                    template = $"launches a {items[rand.Next(items.Count)]} in {user}'s general direction.";
                    break;
                case 7:
                    template = $"sits on {user}'s face while slamming a {items[rand.Next(items.Count)]} into their crotch.";
                    break;
                case 8:
                    template = $"starts slapping {user} silly with a {items[rand.Next(items.Count)]}.";
                    break;
                case 9:
                    template = $"holds {user} down and repeatedly {hits[rand.Next(hits.Count)]} them with a {items[rand.Next(items.Count)]}.";
                    break;
                case 10:
                    template = $"prods {user} with a {items[rand.Next(items.Count)]}.";
                    break;
                case 11:
                    template = $"picks up a {items[rand.Next(items.Count)]} and {hits[rand.Next(hits.Count)]} {user} with it.";
                    break;
                case 12:
                    template = $"ties {user} to a chair and {throws[rand.Next(throws.Count)]} a {items[rand.Next(items.Count)]} at them.";
                    break;
                case 13:
                    template = $"{hits[rand.Next(hits.Count)]} {user} {where[rand.Next(where.Count)]} with a {items[rand.Next(items.Count)]}.";
                    break;          
            }
            return template;
        }

        private static List<string> items = new List<string>
        {
            "cast iron skillet",
            "large trout",
            "baseball bat",
            "cricket bat",
            "wooden cane",
            "nail",
            "printer",
            "shovel",
            "pair of trousers",
            "CRT monitor",
            "diamond sword",
            "baguette",
            "physics textbook",
            "toaster",
            "portrait of Richard Stallman",
            "television",
            "mau5head",
            "five ton truck",
            "roll of duct tape",
            "book",
            "laptop",
            "old television",
            "sack of rocks",
            "rainbow trout",
            "cobblestone block",
            "lava bucket",
            "rubber chicken",
            "spiked bat",
            "gold block",
            "fire extinguisher",
            "heavy rock",
            "chunk of dirt"
        };

        private static List<string> pluralItems = new List<string>
        {
            "cast iron skillets",
            "large trouts",
            "baseball bats",
            "wooden canes",
            "nails",
            "printers",
            "shovels",
            "pairs of trousers",
            "CRT monitors",
            "diamond swords",
            "baguettes",
            "physics textbooks",
            "toasters",
            "portraits of Richard Stallman",
            "televisions",
            "mau5heads",
            "five ton trucks",
            "rolls of duct tape",
            "books",
            "laptops",
            "old televisions",
            "sacks of rocks",
            "rainbow trouts",
            "cobblestone blocks",
            "lava buckets",
            "rubber chickens",
            "spiked bats",
            "gold blocks",
            "fire extinguishers",
            "heavy rocks",
            "chunks of dirt"
        };

        private static List<string> throws = new List<string>
        {
            "throws",
            "flings",
            "chucks"
        };

        private static List<string> hits = new List<string>
        {
             "hits",
            "whacks",
            "slaps",
            "smacks"
        };

        private static List<string> where = new List<string>
        {
            "in the chest",
            "on the head",
            "on the bum"
        };
    }
}
