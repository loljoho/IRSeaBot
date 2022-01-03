using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IRSeaBot.Services
{
    public class BotContainer : BackgroundService
    {
        public IServiceProvider Services;
        public BotContainer(IServiceProvider services)
        {
            Services = services;
        }

        private async Task StartBot(CancellationToken cancellationToken)
        {
            using var scope = Services.CreateScope();
            IRCBot bot = scope.ServiceProvider.GetRequiredService<IRCBot>();
            await bot.Chat(cancellationToken);
        }


        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await StartBot(cancellationToken);
        }
    }
}
