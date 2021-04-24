using IRSeaBot.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Services
{
    public class LikesService
    {
        public async Task<Like> GetLikes(string phrase)
        {
            string pathString = GetPath();
            ConcurrentDictionary<string, Like> likesDict = new ConcurrentDictionary<string, Like>();
            string likesString = "";
            using (StreamReader reader = new StreamReader(pathString))
            {
                try
                {
                    likesString = await reader.ReadToEndAsync();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }             
            }
            if (!String.IsNullOrWhiteSpace(likesString))
            {
                string[] parsed = likesString.Split(",");
                Parallel.ForEach(parsed, p =>
                {
                    if (!String.IsNullOrWhiteSpace(p))
                    {
                        Like like = parseLike(p);
                        likesDict.TryAdd(like.Phrase, like);
                    }
                });
            }
            bool found = likesDict.TryGetValue(phrase, out Like like);
            if (found) return like;
            else return null;
        }

        private Like parseLike(string p)
        {
            p = p.Trim();
            string[] split = p.Split("|");
            Like like = new Like
            {
                Phrase = split[0],
                Score = Convert.ToInt32(split[1]),
            };
            return like;
        }

        private string GetPath()
        {
            var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var complete = Path.Combine(systemPath, "IRSeaBot");
            Directory.CreateDirectory(complete);
            var pathString = Path.Combine(complete, "Likes.txt");
            if (!File.Exists(pathString))
            {
                using(FileStream fs = File.Create(pathString))
                {
                    fs.Write(null);
                }
            }
            return pathString;
        }

        public async Task<Like> EditLike(string phrase, string direction)
        {
            string pathString = GetPath();

            ConcurrentDictionary<string, Like> likesDict = new ConcurrentDictionary<string, Like>();
            string likesString = ""; 
            using (StreamReader reader = new StreamReader(pathString))
            {
                try
                {
                    likesString = await reader.ReadToEndAsync();
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
               
            }
            if (!String.IsNullOrWhiteSpace(likesString))
            {
                string[] parsed = likesString.Split(",");
                Parallel.ForEach(parsed, p =>
                {
                    if (!String.IsNullOrWhiteSpace(p))
                    {
                        Like like = parseLike(p);
                        likesDict.TryAdd(like.Phrase, like);
                    }
                });
            }

            if(!likesDict.Any(x => x.Key.Equals(phrase)))
            {

                Like newLike = new Like
                {
                    Phrase = phrase,
                    Score = direction == "+" ? 1 : -1,
                };
                likesDict.TryAdd(newLike.Phrase, newLike);
            }
            else
            {
                if (direction == "+") ++likesDict[phrase].Score;
                else --likesDict[phrase].Score;
            }
            
            var tempFile = Path.GetTempFileName();
            using (StreamWriter output = new StreamWriter(tempFile))
            {
                foreach(KeyValuePair<string, Like> kv in likesDict)
                {
                    await output.WriteLineAsync(kv.Value.Phrase + "|" + kv.Value.Score + ",");
                }
                
            }
            File.Delete(pathString);
            File.Move(tempFile, pathString);
            return likesDict[phrase];
        }
    }
}
