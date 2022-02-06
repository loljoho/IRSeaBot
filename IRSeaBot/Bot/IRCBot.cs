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
using System.Timers;

namespace IRSeaBot.Services
{
    public class IRCBot
    {
        // server to connect to
        private string _server; // Settings.server;
        // server port (6667 by default)
        private int _port; // Settings.port;
        // user information defined in RFC 2812 (IRC: Client Protocol) is sent to the IRC server 
        private string _user; //Settings.user;
        // the bot's nickname
        private string _nick;/* = Settings.nick;*/
        // channel to join
        private string _channel;// = Settings.channel;
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

        public BotConfiguration getConfig()
        {
            return _config;
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

        private async Task CheckReminders(StreamWriter writer)
        {
            await _reminderContainer.CheckReminders(writer);
        }

        public async Task Chat(CancellationToken cancellationToken, BotConfiguration config, IServiceProvider services)
        {
            _config = config;
            _server = config.Server;
            _port = config.Port;
            _user = config.User;
            _nick = config.Nick; 
            _channel = config.Channel;
            bool retry = true;
            int retryCount = 0;
            await _reminderContainer.LoadReminders();
            reminderTimer = new System.Timers.Timer(10000);
            //await Task.Delay(10, cancellationToken);
            try
            {
                while (!cancellationToken.IsCancellationRequested && retry)
                {
                    try
                    {
                        //join server and set nick
                        using var irc = new TcpClient(_server, _port);
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
                            //cancellationToken.ThrowIfCancellationRequested();
                        });
                        SetClientNick(writer);
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
                                    JoinChannel(writer);
                                }
                                else if (reply.Command == "PRIVMSG" && !String.IsNullOrWhiteSpace(reply.Message))
                                {
                                    await _resolver.ResolveCommand(writer, reply, services);
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
                        await Task.Delay(5000);
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
