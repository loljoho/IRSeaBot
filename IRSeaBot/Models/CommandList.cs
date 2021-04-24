using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRSeaBot.Models
{
    public class CommandList
    {
        public List<Command> Commands { get;}
        public CommandList()
        {
            Commands = new List<Command>();
            var quayle = new Command
            {
                Title = "Dan Quayle Quotes",
                Cmd = ".quayle",
                Usage = ".quayle",
                Description = "Famous quotes from 44th Vice President Dan Quayle",
            };
            var we = new Command
            {
                Title = "Weather Forecast",
                Cmd = ".we",
                Usage = ".we <location>",
                Description = "Gets the current weather for your location."
            };
            var yt = new Command
            {
                Title = "YouTube Videos",
                Cmd = ".yt",
                Usage = ".yt <search term>",
                Description = "Gets the first youtube video returned by your search key."
            };
            var likes = new Command
            {
                Title = "Likes",
                Cmd = "++ / --",
                Usage = ".likes <search term>",
                Description = "Gets a phrases likes value.",
            };
            Commands.Add(yt); Commands.Add(we); Commands.Add(quayle); Commands.Add(likes);
        }
    }

    public class Command
    {
        public string Title { get; set; }
        public string Cmd { get; set; }
        public string Usage { get; set; }
        public string Description { get; set; }
    }


}
