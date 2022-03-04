using IRSeaBot.Dtos;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using IRSeaBot.Data.Entities;
using IRSeaBot.Data.Repositories;
using IRSeaBot.Factories;

namespace IRSeaBot.Services
{
    public class BotCommandResolver
    {
        private static string GetRestOfMessage(string[] msg)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < msg.Length; i++)
            {
                sb.Append(msg[i] + " ");
            }
            return sb.ToString();
        }

        private static string GetFirstWordOfMessage(string[] msg)
        {
            string word = String.Empty;
            if (msg.Length >= 1)
            {
                word = msg[1];
            }
            return word;
        }
        private static string GetReplyTo(ChatReply reply, string botNick)
        {
            string replyTo;
            if (reply.Param == botNick) replyTo = reply.User.Substring(1).Split("!")[0];
            else replyTo = reply.Param;
            return replyTo;
        }

        private async Task LogUserAsync(StreamWriter writer, ChatReply reply, string replyTo, IServiceProvider services)
        {
            if (reply.Message.Contains(".seen")) return;
            try
            {
                string username = reply.User.Substring(1).Split("!")[0];
                string[] input = { username, reply.Message, DateTime.Now.ToString() };
                SeenUser seenUser = SeenUser.CreateSeen(input, replyTo);
                using var scope = services.CreateScope();
                SeenUserRepository repository = scope.ServiceProvider.GetRequiredService<SeenUserRepository>();
                await repository.UpsertSeenUser(seenUser);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private async Task WriteReminder(StreamWriter writer, string message, ChatReply chatReply, string replyTo, IServiceProvider services)
        {
            string username = chatReply.User.Substring(1).Split("!")[0];
            string[] input = message.Split("-");
            Reminder reminder = Reminder.CreateReminder(input, username, replyTo);
            if(reminder != null)
            {
                using var scope = services.CreateScope();
                ReminderRepository repository = scope.ServiceProvider.GetRequiredService<ReminderRepository>();
                Reminder newReminder = await repository.Insert(reminder);
                writer.WriteLine(newReminder?.GetSendMessage());
                writer.Flush();
                await ReminderContainer.AddReminder(reminder);
            }
            else
            {
                writer.WriteLine($"PRIVMSG {replyTo} reminder format is #y#mo#w#d#m#h#s - reminder message");
                writer.Flush();
            }

        }

        private static bool IsLike(string[] msg)
        {
            if (msg[0].EndsWith("++") || msg[0].EndsWith("--"))
            {
                if (msg[0].Length > 2) return true;
            }
            return false;
        }

        public async Task SendLike(StreamWriter writer, string[] msg, string replyTo, IServiceProvider services)
        {
            if (msg[0].EndsWith("++"))
            {
                await SendLike(writer, msg, replyTo, '+', services);
            }
            else if (msg[0].EndsWith("--"))
            {
                await SendLike(writer, msg, replyTo, '-', services);
            }
        }

        public async Task SendLike(StreamWriter writer, string[] msg, string replyTo, char direction, IServiceProvider services)
        {
            string phrase = msg[0].Split(":")[1];
            string[] input = new string[2];
            if (direction == '+')
            {
                phrase = phrase.Split("+")[0];
                input[0] = phrase;
                input[1] = "1";
            }
            else
            {
                phrase = phrase.Split("-")[0];
                input[0] = phrase;
                input[1] = "-1";
            }

            if (Like.CreateLike(input, replyTo) is Like like)
            {
                using var scope = services.CreateScope();
                LikeRepository repository = scope.ServiceProvider.GetRequiredService<LikeRepository>();
                Like newLike = await repository.UpsertLike(like);
                writer.WriteLine(newLike?.GetSendMessage());
                writer.Flush();
            }
        }

        public async Task ResolveCommand(StreamWriter writer, ChatReply reply, IServiceProvider services, string botNick)
        {
            string replyTo = GetReplyTo(reply, botNick);
            await LogUserAsync(writer, reply, replyTo, services);
            string[] msg = reply.Message.Split(" ");

            if (IsLike(msg))
            {
                await SendLike(writer, msg, replyTo, services);
            }
            else
            {
                switch (msg[0])
                {
                    case ":.test":
                        writer.WriteLine("PRIVMSG " + replyTo + " :asbestos in obstetrics");
                        writer.Flush();
                        break;
                    case ":.help":
                        string help;
                        string helpMsg = GetRestOfMessage(msg);
                        if (String.IsNullOrWhiteSpace(helpMsg))
                        {
                            help = CommandRepository.GetCommands();
                        }
                        else
                        {
                            help = CommandRepository.GetCommandString(helpMsg);
                        }

                        writer.WriteLine("PRIVMSG " + replyTo + " " + help);
                        writer.Flush();
                        break;
                    case ":.quayle":
                        string quote = DQFactory.GetRandomQuote();
                        writer.WriteLine("PRIVMSG " + replyTo + " " + quote);
                        writer.Flush();
                        break;
                    case ":.fight":
                        string fightMsg = GetFirstWordOfMessage(msg);
                        if (!String.IsNullOrWhiteSpace(fightMsg))
                        {
                            string fight = FightFactory.GetFight(fightMsg);
                            writer.WriteLine("PRIVMSG " + replyTo + " " + fight);
                            writer.Flush();
                        }
                        break;
                    case ":.hi5":
                        string hi5Msg = GetFirstWordOfMessage(msg);
                        if (!String.IsNullOrWhiteSpace(hi5Msg))
                        {
                            string sender = reply.User.Substring(1).Split("!")[0];
                            string hi5 = HighFiveFactory.GetHighFive(sender, hi5Msg);
                            writer.WriteLine("PRIVMSG " + replyTo + " " + hi5);
                            writer.Flush();
                        }
                        break;
                    case ":.we":
                        string msg2 = GetRestOfMessage(msg);
                        using (var scope = services.CreateScope())
                        {

                            WeatherService _s = scope.ServiceProvider.GetRequiredService<WeatherService>();
                            string r = await _s.Get(msg2, replyTo);
                            writer.WriteLine(r);
                            writer.Flush();
                        }
                        break;
                    case ":.fc":
                        string fcMsg = GetRestOfMessage(msg);
                        using (var scope = services.CreateScope())
                        {

                            ForecastService _s = scope.ServiceProvider.GetRequiredService<ForecastService>();
                            string r = await _s.Get(fcMsg, replyTo);
                            writer.WriteLine(r);
                            writer.Flush();
                        }
                        break;
                    case ":.yt":
                        string ytMsg = GetRestOfMessage(msg);
                        using (var scope = services.CreateScope())
                        {

                            YouTubeService _s = scope.ServiceProvider.GetRequiredService<YouTubeService>();
                            string r = await _s.Get(ytMsg, replyTo);
                            writer.WriteLine(r);
                            writer.Flush();
                        }
                        break;
                    case ":.likes":
                        string phrase = GetRestOfMessage(msg).Trim();
                        if(phrase.Length > 0)
                        {
                            using var scope = services.CreateScope();
                            LikeRepository repository = scope.ServiceProvider.GetRequiredService<LikeRepository>();
                            Like like = await repository.GeyByKeyAndChannel(phrase, replyTo);
                            string reponse = like?.GetSendMessage() ?? Like.GetNotFoundMessage(replyTo, phrase);
                            writer.WriteLine(reponse);
                            writer.Flush();
                        }
                        break;
                    case ":.seen":
                        string seenMsg = GetRestOfMessage(msg);
                        using (var scope = services.CreateScope())
                        {
                            SeenUserRepository repository = scope.ServiceProvider.GetRequiredService<SeenUserRepository>();
                            SeenUser user = await repository.GetByKeyAndChannel(seenMsg, replyTo);
                            string response = user?.GetSendMessage() ?? SeenUser.GetNotFoundMessage(replyTo, seenMsg);
                            writer.WriteLine(response);
                            writer.Flush();
                        }
                        break;
                    case ":.8ball":
                        string eballQ = GetRestOfMessage(msg).Trim();
                        string eballReply;
                        if (!eballQ.EndsWith("?"))
                        {
                            eballReply = "I can only answer a question";
                        }
                        else
                        {
                            eballReply = EightBallFactory.GetRandomReply();
                        }
                        writer.WriteLine("PRIVMSG " + replyTo + " " + eballReply);
                        writer.Flush();
                        break;
                    case ":.remind":
                        string reminderMessage = GetRestOfMessage(msg).Trim();
                        await WriteReminder(writer, reminderMessage, reply, replyTo, services);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
