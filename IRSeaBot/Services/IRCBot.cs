using IRSeaBot.Factories;
using IRSeaBot.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace IRSeaBot.Services
{
    public class IRCBot
    {
        // server to connect to
        private readonly string _server = Settings.server;
        // server port (6667 by default)
        private readonly int _port = Settings.port;
        // user information defined in RFC 2812 (IRC: Client Protocol) is sent to the IRC server 
        private readonly string _user = Settings.user;
        // the bot's nickname
        private readonly string _nick = Settings.nick;
        // channel to join
        private readonly string _channel = Settings.channel;
        private readonly int _maxRetries = Settings.maxRetries;
        private readonly WeatherService _ws;
        private readonly YouTubeService _yt;
        private readonly IFileService<Like> _ls;
        private readonly IFileService<SeenUser> _ss;
        private readonly ILogger<IRCBot> _logger;

        public IRCBot(WeatherService ws, YouTubeService yt, IFileService<Like> ls, IFileService<SeenUser> ss, ILogger<IRCBot> logger)
        {
            _ws = ws;
            _yt = yt;
            _ls = ls;
            _ss = ss;
            _logger = logger;
        }

        private string GetRestOfMessage(string[] msg)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < msg.Length; i++)
            {
                sb.Append(msg[i] + " ");
            }
            return sb.ToString();
        }

        private string GetFirstWordOfMessage(string[] msg)
        {
            string word = String.Empty;
            if(msg.Length >= 1)
            {
                word = msg[1];
            }
            return word;
        }

        private ChatReply ParseReply(string inputLine)
        {
            try
            {
                string[] splitInput = inputLine.Split(new Char[] { ' ' });
                ChatReply reply = new ChatReply
                {
                    User = splitInput[0],
                    Command = splitInput[1],
                };
                if(splitInput.Length > 2)
                {
                    reply.Param = splitInput[2];
                    if(splitInput.Length > 3)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 3; i < splitInput.Length; i++)
                        {
                            sb.Append(splitInput[i] + " ");
                        }
                        reply.Message = sb.ToString();
                    }
                }
                return reply;
            }catch(Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
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
            if(direction == '+')
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
                Like newLike = await _ls.WriteFile(like);
                writer.WriteLine(newLike?.GetSendMessage(replyTo));
                writer.Flush();
            }
        }

        public async Task GetLike(StreamWriter writer, string[] msg, string replyTo)
        {
            string phrase = GetRestOfMessage(msg);
            Like like = await _ls.GetFileItem(phrase.Trim());
            if(like != null)
            {
                writer.WriteLine(like.GetSendMessage(replyTo));
                writer.Flush();
            }
            else
            {
                writer.WriteLine($"PRIVMSG {replyTo} {phrase} has 0 likes");
                writer.Flush();
            }
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
                    await _ss.WriteFile(seenUser);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
        }

        private async Task GetSeen(string[] msg, string replyTo, StreamWriter writer)
        {
            string seenMsg = GetRestOfMessage(msg);
            if (await _ss.GetFileItem(seenMsg) is SeenUser user)
            {
                writer.WriteLine(user.GetSendMessage(replyTo));
                writer.Flush();
            }
            else
            {
                writer.WriteLine("PRIVMSG " + replyTo + " who?");
                writer.Flush();
            }
        }
        
        private string GetReplyTo(ChatReply reply)
        {
            string replyTo;
            if (reply.Param == "LookOfRobot") replyTo = reply.User.Substring(1).Split("!")[0];
            else replyTo = reply.Param;
            return replyTo;
        }

        private bool IsLike(string[] msg)
        {
            if (msg[0].EndsWith("++") || msg[0].EndsWith("--"))
            {
                if(msg[0].Length > 2) return true;
            }
            return false;
        }

        private void SetClientNick(StreamWriter writer)
        {
            writer.WriteLine("NICK " + _nick);
            writer.Flush();
            writer.WriteLine(_user);
            writer.Flush();
        }

        private void SendPongReply(StreamWriter writer, ChatReply reply)
        {
            string pongReply = reply.Command;
            writer.WriteLine("PONG " + pongReply);
            writer.Flush();
        }

        private void JoinChannel(StreamWriter writer)
        {
            writer.WriteLine("JOIN " + _channel);
            writer.Flush();
        }

        public async Task Chat(CancellationToken cancellationToken)
        {
            bool retry = true;
            int retryCount = 0;
            while (!cancellationToken.IsCancellationRequested && retry)
            {
                try
                {
                    //join server and set nick
                    using var irc = new TcpClient(_server, _port);
                    using var stream = irc.GetStream();
                    using var reader = new StreamReader(stream);
                    using var writer = new StreamWriter(stream);
                    SetClientNick(writer);

                    while (true) //start chatting
                    {
                        string inputLine;
                        while ((inputLine = reader.ReadLine()) != null) //someone sent a message
                        {
                            try
                            {
                                ChatReply reply = ParseReply(inputLine);
                                if (reply == null) continue;
                                else if(reply.User == "PING") // reply to server ping
                                {
                                    SendPongReply(writer, reply);
                                }
                                else if(reply.Command == "001") //join a channel
                                {
                                    JoinChannel(writer);
                                }
                                else if (reply.Command == "PRIVMSG" && !String.IsNullOrWhiteSpace(reply.Message))
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
                                                string helpMsg = GetRestOfMessage(msg);
                                                if (String.IsNullOrWhiteSpace(helpMsg))
                                                {
                                                    help = CommandFactory.GetCommands();
                                                }
                                                else
                                                {
                                                    help = CommandFactory.GetCommand(helpMsg);
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
                                                if(!String.IsNullOrWhiteSpace(fightMsg))
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
                                                string r = await _ws.GetWeather(msg2);
                                                writer.WriteLine("PRIVMSG " + replyTo + " " + r);
                                                writer.Flush();
                                                break;
                                            case ":.fc":
                                                string fcMsg = GetRestOfMessage(msg);
                                                string fc = await _ws.GetForecast(fcMsg);
                                                writer.WriteLine("PRIVMSG " + replyTo + " " + fc);
                                                writer.Flush();
                                                break;
                                            case ":.yt":
                                                string ytMsg = GetRestOfMessage(msg);
                                                string ytR = await _yt.GetVideo(ytMsg);
                                                writer.WriteLine("PRIVMSG " + replyTo + " " + ytR);
                                                writer.Flush();
                                                break;
                                            case ":.likes":
                                                await GetLike(writer, msg, replyTo);
                                                break;
                                            case ":.seen":
                                                await GetSeen(msg, replyTo, writer);                                        
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
                                            default:
                                                break;
                                        }
                                    }
                                }
                            }
                            catch(Exception ex)
                            {
                                _logger.LogError(ex.Message);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    // shows the exception, sleeps for a little while and then tries to establish a new connection to the IRC server
                    _logger.LogInformation(e.ToString());
                    await Task.Delay(5000);
                    retryCount++;
                    if (retryCount > _maxRetries) retry = false;
                }
            }
        }
    }
}
