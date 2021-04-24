using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Models
{
    public class SeenUser
    {
        public string Username { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
