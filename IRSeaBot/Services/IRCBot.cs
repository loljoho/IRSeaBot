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
        private readonly LikesService _ls;
        private readonly SeenService _ss;
        private DateTime lastSeenWrite = DateTime.Now;
        private Dictionary<string, SeenUser> seenUsers;
        private System.Timers.Timer seenWriteTimer;
        private readonly object seenDictLock = new();
        private SemaphoreSlim seenWriteSemaphore;
        private readonly ILogger<IRCBot> _logger;

        public IRCBot(WeatherService ws, YouTubeService yt, LikesService ls, SeenService ss, ILogger<IRCBot> logger)
        {
            _ws = ws;
            _yt = yt;
            _ls = ls;
            _ss = ss;
            seenWriteSemaphore = new SemaphoreSlim(1, 1);
            seenUsers = new Dictionary<string, SeenUser>();
            seenWriteTimer = new System.Timers.Timer(30000);
            seenWriteTimer.AutoReset = true;
            seenWriteTimer.Elapsed += async (sender, e) => await WriteSeens();
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

        public void SendLike(StreamWriter writer, Like like, string replyTo)
        {
            if (like != null)
            {
                writer.WriteLine("PRIVMSG " + replyTo + " " + like.Phrase + " has " + like.Score + " likes.");
                writer.Flush();
            }
        }

        private void SendUser(StreamWriter writer, SeenUser user, string replyTo)
        {
            if(user != null)
            {
                writer.WriteLine("PRIVMSG " + replyTo + " " + user.Username + " was last seen at " + user.Timestamp.ToString() + " saying " + user.Message);
                writer.Flush();
            }
            else
            {
                writer.WriteLine("PRIVMSG " + replyTo + " who?");
                writer.Flush();
            }
        }

        private void LogUser(ChatReply reply)
        {
            if (reply.Message.Contains(".seen")) return;
            try
            {
                string username = reply.User.Substring(1).Split("!")[0];
                if (username == Settings.nick) return; //don't log the bot
                SeenUser seenUser = new SeenUser
                {
                    Username = username,
                    Message = reply.Message.Replace('|', ' ').Replace('~', ' '), //remove delimiters
                    Timestamp = DateTime.Now
                };
                lock (seenDictLock)
                {
                    seenUsers[seenUser.Username] = seenUser;
                }
            }catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private async Task WriteSeens()
        {
            if(seenUsers.Count > 0)
            {
                Dictionary<string, SeenUser> writes;
                lock (seenDictLock)
                {
                    writes = new Dictionary<string, SeenUser>(seenUsers);
                    seenUsers.Clear();
                }
                await seenWriteSemaphore.WaitAsync();
                try
                {
                    await _ss.WriteSeens(writes);
                }catch(Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                finally
                {
                    seenWriteSemaphore.Release();
                }                         
            }      
        }

        public async Task Chat(CancellationToken cancellationToken)
        {
            bool retry = true;
            int retryCount = 0;
            seenWriteTimer.Start();
            await Task.Delay(25, cancellationToken);
            while (!cancellationToken.IsCancellationRequested && retry)
            {
                try
                {
                    using var irc = new TcpClient(_server, _port);
                    using var stream = irc.GetStream();
                    using var reader = new StreamReader(stream);
                    using var writer = new StreamWriter(stream);
                    writer.WriteLine("NICK " + _nick);
                    writer.Flush();
                    writer.WriteLine(_user);
                    writer.Flush();

                    while (true)
                    {
                        string inputLine;
                        while ((inputLine = reader.ReadLine()) != null)
                        {
                            try
                            {
                                _logger.LogInformation("<- " + inputLine);
                                ChatReply reply = ParseReply(inputLine);
                                if (reply == null) continue;
                                if (reply.User == "PING")
                                {
                                    _logger.LogInformation("PING ->");
                                    string PongReply = reply.Command;
                                    _logger.LogInformation("->PONG " + PongReply);
                                    writer.WriteLine("PONG " + PongReply);
                                    writer.Flush();
                                }

                                switch (reply.Command)
                                {
                                    case "001":
                                        writer.WriteLine("JOIN " + _channel);
                                        writer.Flush();
                                        break;
                                    default:
                                        break;
                                }

                                if (reply.Command == "PRIVMSG" && !String.IsNullOrWhiteSpace(reply.Message))
                                {
                                    if (reply.Param.Equals(Settings.channel)) LogUser(reply);

                                    string replyTo = "";
                                    if (reply.Param == "LookOfRobot") replyTo = reply.User.Substring(1).Split("!")[0];
                                    else replyTo = reply.Param;
                                    string[] msg = reply.Message.Split(" ");

                                    if (msg[0].EndsWith("++"))
                                    {
                                        string phrase = msg[0].Split(":")[1];
                                        phrase = phrase.Split("+")[0];
                                        Like like = await _ls.EditLike(phrase, "+");
                                        SendLike(writer, like, replyTo);
                                    }
                                    else if (msg[0].EndsWith("--"))
                                    {
                                        string phrase = msg[0].Split(":")[1];
                                        phrase = phrase.Split("-")[0];
                                        Like like = await _ls.EditLike(phrase, "-");
                                        SendLike(writer, like, replyTo);
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
                                                string phrase = GetRestOfMessage(msg);
                                                Like like = await _ls.GetLikes(phrase.Trim());
                                                SendLike(writer, like, replyTo);
                                                break;
                                            case ":.seen":
                                                string seenMsg = GetRestOfMessage(msg);
                                                await seenWriteSemaphore.WaitAsync();
                                                try
                                                {
                                                    SeenUser user = await _ss.GetSeen(seenMsg);
                                                    SendUser(writer, user, replyTo);
                                                }
                                                catch(Exception ex)
                                                {
                                                    _logger.LogError(ex.Message);
                                                }
                                                finally
                                                {
                                                    seenWriteSemaphore.Release();
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
