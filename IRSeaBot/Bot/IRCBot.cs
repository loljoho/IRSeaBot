using IRSeaBot.Dtos;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IRSeaBot.Services
{
    public class IRCBot
    {
        private readonly int _maxRetries = Settings.maxRetries;
        private readonly ILogger<IRCBot> _logger;
        private readonly BotCommandResolver _resolver;
        private readonly ReminderContainer _reminderContainer;
        private System.Timers.Timer reminderTimer;
        private BotConfiguration _config; 

        public IRCBot(BotCommandResolver resolver, ILogger<IRCBot> logger, ReminderContainer reminderContainer)
        {
            _resolver = resolver;
            _reminderContainer = reminderContainer;
            _logger = logger;
        }

        public BotConfiguration GetConfig()
        {
            return _config;
        }

        private ChatReply ParseReply(string inputLine)
        {
            try
            {
                string[] splitInput = inputLine.Split(new Char[] { ' ' });
                ChatReply reply = new()
                {
                    User = splitInput[0],
                    Command = splitInput[1],
                };
                if(splitInput.Length > 2)
                {
                    reply.Param = splitInput[2];
                    if(splitInput.Length > 3)
                    {
                        StringBuilder sb = new();
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

        private static void SetClientNick(StreamWriter writer, string user, string nick)
        {
            writer.WriteLine($"NICK {nick}");
            writer.Flush();
            writer.WriteLine($"USER {user} 0 * : {user}");
            writer.Flush();
        }

        private static void SendPongReply(StreamWriter writer, ChatReply reply)
        {
            string pongReply = reply.Command;
            writer.WriteLine("PONG " + pongReply);
            writer.Flush();
        }

        private static void JoinChannel(StreamWriter writer, string channel)
        {
            writer.WriteLine("JOIN " + channel);
            writer.Flush();
        }

        private async Task CheckReminders(StreamWriter writer)
        {
            await _reminderContainer.CheckReminders(writer, _config.Channel);
        }

        public async Task Chat(BotConfiguration config, IServiceProvider services, CancellationToken cancellationToken)
        {
            _config = config;
            bool retry = true;
            int retryCount = 0;
            await _reminderContainer.LoadReminders();
            reminderTimer = new System.Timers.Timer(7000);
            try
            {
                while (!cancellationToken.IsCancellationRequested && retry)
                {
                    try
                    {
                        //join server and set nick
                        using var irc = new TcpClient(config.Server, config.Port);
                        using var stream = irc.GetStream();
                        using var reader = new StreamReader(stream);
                        using var writer = new StreamWriter(stream);
                        using CancellationTokenRegistration ctr = cancellationToken.Register(() => {
                            retryCount = _maxRetries;
                            retry = false;
                            writer.Close();
                            reader.Close();
                            stream.Close();
                            irc.Close();
                        });
                        SetClientNick(writer, config.Username, config.Nick);
                        reminderTimer.Elapsed += async (sender, args) => await CheckReminders(writer);
                        reminderTimer.AutoReset = true;
                        reminderTimer.Start();
                        string inputLine;
                        while (!cancellationToken.IsCancellationRequested && (inputLine = reader.ReadLine()) != null) //someone sent a message
                        {

                            try
                            {
                                ChatReply reply = ParseReply(inputLine);
                                if (reply == null) continue;
                                else if (reply.User == "PING") // reply to server ping
                                {
                                    SendPongReply(writer, reply);
                                }
                                else if (reply.Command == "001") //join a channel
                                {
                                    JoinChannel(writer, config.Channel);
                                }
                                else if (reply.Command == "PRIVMSG" && !string.IsNullOrWhiteSpace(reply.Message))
                                {
                                    await _resolver.ResolveCommand(writer, reply, services, config.Nick);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex.Message);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // shows the exception, sleeps for a little while and then tries to establish a new connection to the IRC server
                        _logger.LogInformation(e.ToString());
                        reminderTimer.Stop();
                        await Task.Delay(5000, cancellationToken);
                        retryCount++;
                        if (retryCount > _maxRetries) retry = false;
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (reminderTimer != null)
                {
                    reminderTimer.Dispose();
                }
            }
        }
    }
}
