using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Models
{
    public class ChatReply
    {
        public string User { get; set; }
        public string Command { get; set; }
        public string Param { get; set; }
        public string Message { get; set; }
    }
}
