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
        private readonly ILogger<IRCBot> _logger;
        private readonly BotCommandResolver _resolver;
        public IRCBot(BotCommandResolver resolver, ILogger<IRCBot> logger)
        {
            _resolver = resolver;
            _logger = logger;
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

        public async Task Chat(CancellationToken cancellationToken)
        {
            bool retry = true;
            int retryCount = 0;
            await Task.Delay(10, cancellationToken);
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
                                    await _resolver.ResolveCommand(writer, reply);
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
