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
        public CancellationTokenSource cts;
        public CancellationToken token;

        public BotContainer(IServiceProvider services)
        {
            Services = services;
        }

        private async Task StartBot(CancellationToken cancellationToken)
        {
            token = cancellationToken;
            using var scope = Services.CreateScope();
            IRCBot bot = scope.ServiceProvider.GetRequiredService<IRCBot>();
            await bot.Chat(cancellationToken);
        }


        public override void Dispose()
        {
            cts?.Dispose();
            base.Dispose();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await StartBot(cancellationToken);
        }

        public async Task Restart()
        {
            await base.StopAsync(token);
            if(cts != null && !cts.Token.IsCancellationRequested) cts.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();
            token = cts.Token;
            await base.StartAsync(token);
        }
    }
}
