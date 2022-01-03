using IRSeaBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRSeaBot.Services
{
    public class CommandRepository
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

        public static string GetCommandString(string cmd)
        {
            Command command = GetCommand(cmd);
            if (command != null) return command.CommandString;
            else return "Unknown command";
        }

        public static Command GetCommand(string cmd)
        {
            return commands.FirstOrDefault(x => x.Cmd == cmd.Trim().ToLower());
        }

        private static readonly List<Command> commands = new List<Command>
        {
            new Command
            {
                Title = "Test",
                Cmd = ".test",
                Usage = ".test",
                Description = "Tests the bot",
            },
            new Command
            {
                Title = "Help",
                Cmd = ".help",
                Usage = ".help",
                Description = "Gets help commands",
            },
            new Command
            {
                Title = "Dan Quayle Quotes",
                Cmd = ".quayle",
                Usage = ".quayle",
                Description = "Famous quotes from 44th Vice President Dan Quayle",
            },
            new Command
            {
                Title = "Fight",
                Cmd = ".fight",
                Usage = ".fight <user>",
                Description = "Fights somebody",
            },
            new Command
            {
                Title = "Hi Five",
                Cmd = ".hi5",
                Usage = ".hi5 <user>",
                Description = "Hi fives somebody"
            },
            new Command
            {
                Title = "Weather Report",
                Cmd = ".we",
                Usage = ".we <search term>",
                Description = "Gets the current weather."
            },
            new Command
            {
                Title = "Weather Forecast",
                Cmd = ".fc",
                Usage = ".fc <search term>",
                Description = "Gets the future weather."
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
                Title = "Likes++",
                Cmd = "++",
                Usage = "<term>++",
                Description = "Increments a noun's like value",
            },
            new Command
            {
                Title = "Likes--",
                Cmd = "--",
                Usage = "<term>--",
                Description = "decrements a noun's like value",
            },
            new Command
            {
                Title = "Likes",
                Cmd = ".likes",
                Usage = ".likes <term>",
                Description = "gets a noun's likes",
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
