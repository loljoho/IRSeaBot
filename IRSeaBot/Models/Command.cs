using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Models
{
    public class Command
    {
        public string Title { get; set; }
        public string Cmd { get; set; }
        public string Usage { get; set; }
        public string Description { get; set; }
    }
}
