using IRSeaBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRSeaBot.Services
{
    public static class CommandFactory
    {
        public static string GetCommands()
        {
            CommandList cl = new CommandList();

            StringBuilder sb = new StringBuilder();
            sb.Append("Commands you can use:  ---------   ");
            foreach (Command c in cl.Commands)
            {
                sb.Append(c.Title + ": " + c.Usage + "  ---  ");
            }
            return sb.ToString();
        }
    }
}
