using IRSeaBot.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Services
{
    public class SeenService
    {
        public async Task<SeenUser> GetSeen(string username)
        {
            string pathString = GetPath();
            ConcurrentDictionary<string, SeenUser> seenDict = new ConcurrentDictionary<string, SeenUser>();
            string seenString = "";
            using (StreamReader reader = new StreamReader(pathString))
            {
                try
                {
                    seenString = await reader.ReadToEndAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            if (!String.IsNullOrWhiteSpace(seenString))
            {
                string[] parsed = seenString.Split(",");
                Parallel.ForEach(parsed, p =>
                {
                    if (!String.IsNullOrWhiteSpace(p))
                    {
                        SeenUser user = parseSeenUser(p);
                        seenDict.TryAdd(user.Username, user);
                    }
                });
            }
            bool found = seenDict.TryGetValue(username, out SeenUser user);
            if (found) return user;
            else return null;
        }

        private SeenUser parseSeenUser(string p)
        {
            p = p.Trim();
            string[] split = p.Split("|");
            SeenUser user = new SeenUser
            {
                Username = split[0],
                Message = split[1],
                Timestamp = DateTime.Parse(split[2])
            };
            return user;
        }

        private string GetPath()
        {
            var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var complete = Path.Combine(systemPath, "IRSeaBot");
            Directory.CreateDirectory(complete);
            var pathString = Path.Combine(complete, "Seen.txt");
            if (!File.Exists(pathString))
            {
                using FileStream fs = File.Create(pathString);
                fs.Write(null);
            }
            return pathString;
        }

        public async Task<DateTime> WriteSeens(IDictionary<string, SeenUser> users)
        {
            string pathString = GetPath();

            ConcurrentDictionary<string, SeenUser> seenDict = new ConcurrentDictionary<string, SeenUser>();
            string seenStiring = "";

            using (StreamReader reader = new StreamReader(pathString))
            {
                try
                {
                    seenStiring = await reader.ReadToEndAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            }
            if (!String.IsNullOrWhiteSpace(seenStiring))
            {
                string[] parsed = seenStiring.Split(",");
                Parallel.ForEach(parsed, p =>
                {
                    if (!String.IsNullOrWhiteSpace(p))
                    {
                        SeenUser user = parseSeenUser(p);
                        seenDict.TryAdd(user.Username, user);
                    }
                });
            }

            Parallel.ForEach(users, (kv) =>
            {
                seenDict.AddOrUpdate(kv.Key, kv.Value, (name, user) =>
                {
                    user = kv.Value;
                    return user;
                });
            });

            var tempFile = Path.GetTempFileName();
            using (StreamWriter output = new StreamWriter(tempFile))
            {
                foreach (KeyValuePair<string, SeenUser> kv in seenDict)
                {
                    await output.WriteLineAsync(kv.Value.Username + "|" + kv.Value.Message + "|" + kv.Value.Timestamp + ",");
                }
            }
            File.Delete(pathString);
            File.Move(tempFile, pathString);
            return DateTime.Now;
        }
    }
}
