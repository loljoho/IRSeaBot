using System;
using System.Threading;

namespace IRSeaBot.Models
{
    public class BotConfiguration
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Server { get; set; }
        public int Port { get; set; } = 6667;
        public string Username { get; set; }
        public string User
        {
            get
            {
                return $"USER {Username} 0 * : IRSeaBot";
            }
        }

        public string Nick { get; set; }

        public string Channel { get; set; }

        public CancellationTokenSource cancellationTokenSource { get; set; } = new CancellationTokenSource();
    }
}
