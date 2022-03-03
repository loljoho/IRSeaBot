using IRSeaBot.Models;
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
    public class BotContainer
    {
        public IServiceProvider Services;
        private ConcurrentDictionary<Guid, IRCBot> botDictionary;

        public BotContainer(IServiceProvider services)
        {
            Services = services;
            botDictionary = new ConcurrentDictionary<Guid,IRCBot>();
        }

        public List<IRCBot> GetBotList()
        {
            return botDictionary.Values.ToList();
        }

        public void StartBot(BotConfiguration config)
        {
            CancellationToken token = config.cancellationTokenSource.Token;
            using var scope = Services.CreateScope();
            IRCBot bot = scope.ServiceProvider.GetRequiredService<IRCBot>();
            bool added = botDictionary.TryAdd(config.Id, bot);
            if (added)
            {
                _ = bot.Chat(token, config, Services);
            }
        }

        public void StopBot(Guid guid)
        {
            bool found = botDictionary.TryGetValue(guid, out var bot);
            if (found)
            {
                BotConfiguration config = bot.getConfig();
                config.cancellationTokenSource.Cancel();
                bool removed = botDictionary.TryRemove(guid, out var removedBot);
                if (removed)
                {
                    Console.WriteLine($"bot {removedBot.getConfig().Id}");
                }
            }
        }
    }
}
