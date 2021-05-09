using IRSeaBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRSeaBot.Services
{
    public class CommandFactory
    {
        public static string GetCommands()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Commands you can use:  ---------   ");
            foreach (Command c in commands)
            {
                sb.Append($"{c.Cmd}: {c.Title}  | | ");
            }
            return sb.ToString();
        }

        public static string GetCommand(string cmd)
        {
            Command command = commands.FirstOrDefault(x => x.Cmd.Equals(cmd.Trim().ToLower()));
            if (command != null){
                return $"{command.Title}: {command.Description} - Usage: {command.Usage}";
            }
            else
            {
                return "Unknown command";
            }       
        }

        private static readonly List<Command> commands = new List<Command>
        {
            new Command
            {
                Title = "Dan Quayle Quotes",
                Cmd = ".quayle",
                Usage = ".quayle",
                Description = "Famous quotes from 44th Vice President Dan Quayle",
            },
            new Command
            {
                Title = "Weather Forecast",
                Cmd = ".we",
                Usage = ".we <location>",
                Description = "Gets the current weather for your location."
            },
            new Command
            {
                Title = "YouTube Videos",
                Cmd = ".yt",
                Usage = ".yt <search term>",
                Description = "Gets the first youtube video returned by your search key."
            },
            new Command
            {
                Title = "Likes",
                Cmd = "++ / --",
                Usage = ".likes <search term>",
                Description = "Gets a phrases likes value.",
            },
            new Command
            {
                Title = "Seen",
                Cmd = ".seen",
                Usage = ".seen <user>",
                Description = "Gets the last time I have seen a user.",
            },
            new Command
            {
                Title = "8 Ball",
                Cmd = ".8ball",
                Usage = ".8ball <yes or no question>",
                Description = "Predicts the future.",
            },
        };
    }
}
