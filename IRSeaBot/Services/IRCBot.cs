﻿using IRSeaBot.Models;
using System;
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
        public IRCBot(WeatherService ws, YouTubeService yt, LikesService ls)
        {
            _ws = ws;
            _yt = yt;
            _ls = ls;
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
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public void SendLike(StreamWriter writer, Like like, string replyTo)
        {
            if (like != null)
            {
                writer.WriteLine("PRIVMSG " + replyTo + " " + like.Phrase + " has " + like.Score + " points.");
                writer.Flush();
            }
        }

        public async Task Chat(CancellationToken cancellationToken)
        {
            bool retry = true;
            int retryCount = 0;
            while (!cancellationToken.IsCancellationRequested && retry)
            {
                try
                {
                    using (var irc = new TcpClient(_server, _port))
                    using (var stream = irc.GetStream())
                    using (var reader = new StreamReader(stream))
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.WriteLine("NICK " + _nick);
                        writer.Flush();
                        writer.WriteLine(_user);
                        writer.Flush();

                        while (true)
                        {
                            string inputLine;
                            while ((inputLine = reader.ReadLine()) != null)
                            {
                                Console.WriteLine("<- " + inputLine);
                                ChatReply reply = ParseReply(inputLine);
                                if (reply == null) continue;
                                if (reply.User == "PING") //splitInput[0]
                                {
                                    Console.WriteLine("PING ->");
                                    string PongReply = reply.Command; // splitInput[1];
                                    Console.WriteLine("->PONG " + PongReply);
                                    writer.WriteLine("PONG " + PongReply);
                                    writer.Flush();
                                    //continue;
                                }

                                switch (reply.Command) //splitInput[1]
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
                                    string replyTo = "";
                                    if (reply.Param == "LookOfRobot")
                                    {
                                        replyTo = reply.User.Substring(1).Split("!")[0];
                                    }
                                    else
                                    {
                                        replyTo = reply.Param;
                                    }

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
                                        switch (msg[0]) //splitInput[3]
                                        {
                                            case ":.test":
                                                writer.WriteLine("PRIVMSG " + replyTo + " :asbestos in obstetrics");
                                                writer.Flush();
                                                break;
                                            case ":.help":
                                                string help = CommandFactory.GetCommands();
                                                writer.WriteLine("PRIVMSG " + replyTo + " " + help);
                                                writer.Flush();
                                                break;
                                            case ":.quayle":
                                                string quote = DQFactory.GetRandomQuote();
                                                writer.WriteLine("PRIVMSG " + replyTo + " " + quote);
                                                writer.Flush();
                                                break;
                                            case ":.we":
                                                string msg2 = GetRestOfMessage(msg);
                                                string r = await _ws.GetWeather(msg2);
                                                writer.WriteLine("PRIVMSG " + replyTo + " " + r);
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
                                            default:
                                                break;
                                        }
                                    }

 
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    // shows the exception, sleeps for a little while and then tries to establish a new connection to the IRC server
                    Console.WriteLine(e.ToString());
                    await Task.Delay(5000);
                    retryCount++;
                    if (retryCount > _maxRetries) retry = false;
                }
            }
        }
    }
}