using IRSeaBot.Models;
using System;
using System.IO;
using IRSeaBot.Utils;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using IRSeaBot.Factories;

namespace IRSeaBot.Services
{
    public class BotCommandResolver
    {
        public IServiceProvider Services;

        public BotCommandResolver(IServiceProvider services)
        {
            Services = services;
        }

        private string GetReplyTo(ChatReply reply)
        {
            string replyTo;
            if (reply.Param == "LookOfRobot") replyTo = reply.User.Substring(1).Split("!")[0];
            else replyTo = reply.Param;
            return replyTo;
        }

        private async Task LogUserAsync(StreamWriter writer, ChatReply reply, string replyTo)
        {
            if (replyTo.Equals(Settings.channel))
            {
                if (reply.Message.Contains(".seen")) return;
                try
                {
                    string username = reply.User.Substring(1).Split("!")[0];
                    if (username == Settings.nick) return; //don't log the bot
                    string[] input = { username, reply.Message, DateTime.Now.ToString() };
                    SeenUser seenUser = FileItemFactory.CreateFile(input, FileTypes.Seen) as SeenUser;
                    using var scope = Services.CreateScope();
                    IFileService<SeenUser> _s = scope.ServiceProvider.GetRequiredService<IFileService<SeenUser>>();
                    await _s.WriteFile(seenUser);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private bool IsLike(string[] msg)
        {
            if (msg[0].EndsWith("++") || msg[0].EndsWith("--"))
            {
                if (msg[0].Length > 2) return true;
            }
            return false;
        }

        public async Task SendLike(StreamWriter writer, string[] msg, string replyTo)
        {
            if (msg[0].EndsWith("++"))
            {
                await SendLike(writer, msg, replyTo, '+');
            }
            else if (msg[0].EndsWith("--"))
            {
                await SendLike(writer, msg, replyTo, '-');
            }
        }

        public async Task SendLike(StreamWriter writer, string[] msg, string replyTo, char direction)
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
            if (FileItemFactory.CreateFile(input, FileTypes.Likes) is Like like)
            {
                using var scope = Services.CreateScope();
                IFileService<Like> _s = scope.ServiceProvider.GetRequiredService<IFileService<Like>>();
                Like newLike = await _s.WriteFile(like);
                writer.WriteLine(newLike?.GetSendMessage(replyTo));
                writer.Flush();
            }
        }

        public async Task ResolveCommand(StreamWriter writer, ChatReply reply)
        {
            string replyTo = GetReplyTo(reply);
            await LogUserAsync(writer, reply, replyTo);
            string[] msg = reply.Message.Split(" ");

            if (IsLike(msg))
            {
                await SendLike(writer, msg, replyTo);
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
                        string helpMsg = Util.GetRestOfMessage(msg);
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
                        string fightMsg = Util.GetFirstWordOfMessage(msg);
                        if (!String.IsNullOrWhiteSpace(fightMsg))
                        {
                            string fight = FightFactory.GetFight(fightMsg);
                            writer.WriteLine("PRIVMSG " + replyTo + " " + fight);
                            writer.Flush();
                        }
                        break;
                    case ":.hi5":
                        string hi5Msg = Util.GetFirstWordOfMessage(msg);
                        if (!String.IsNullOrWhiteSpace(hi5Msg))
                        {
                            string sender = reply.User.Substring(1).Split("!")[0];
                            string hi5 = HighFiveFactory.GetHighFive(sender, hi5Msg);
                            writer.WriteLine("PRIVMSG " + replyTo + " " + hi5);
                            writer.Flush();
                        }
                        break;
                    case ":.we":
                        string msg2 = Util.GetRestOfMessage(msg);
                        using (var scope = Services.CreateScope())
                        {

                            WeatherService _s = scope.ServiceProvider.GetRequiredService<WeatherService>();
                            string r = await _s.Get(msg2, replyTo);
                            writer.WriteLine(r);
                            writer.Flush();
                        }
                        break;
                    case ":.fc":
                        string fcMsg = Util.GetRestOfMessage(msg);
                        using (var scope = Services.CreateScope())
                        {

                            ForecastService _s = scope.ServiceProvider.GetRequiredService<ForecastService>();
                            string r = await _s.Get(fcMsg, replyTo);
                            writer.WriteLine(r);
                            writer.Flush();
                        }
                        break;
                    case ":.yt":
                        string ytMsg = Util.GetRestOfMessage(msg);
                        using (var scope = Services.CreateScope())
                        {

                            ForecastService _s = scope.ServiceProvider.GetRequiredService<ForecastService>();
                            string r = await _s.Get(ytMsg, replyTo);
                            writer.WriteLine(r);
                            writer.Flush();
                        }
                        break;
                    case ":.likes":
                        string phrase = Util.GetRestOfMessage(msg).Trim();
                        if(phrase.Length > 0)
                        {
                            using (var scope = Services.CreateScope())
                            {
                                IFileService<Like> _s = scope.ServiceProvider.GetRequiredService<IFileService<Like>>();
                                string r = await _s.Get(phrase, replyTo);
                                writer.WriteLine(r);
                                writer.Flush();
                            }
                        }
                        break;
                    case ":.seen":
                        string seenMsg = Util.GetRestOfMessage(msg);
                        using (var scope = Services.CreateScope())
                        {
                            IFileService<SeenUser> _s = scope.ServiceProvider.GetRequiredService<IFileService<SeenUser>>();
                            string r = await _s.Get(seenMsg, replyTo);
                            writer.WriteLine(r);
                            writer.Flush();
                        }
                        break;
                    case ":.8ball":
                        string eballQ = Util.GetRestOfMessage(msg).Trim();
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
                    default:
                        break;
                }
            }
        }
    }
}
