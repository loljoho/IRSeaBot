﻿using IRSeaBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace IRSeaBot.Services
{
    public class YouTubeService
    {
        public async Task<string> GetVideo(string searchKey)
        {
            using(HttpClient http = new HttpClient())
            {
                string query = $"https://youtube.googleapis.com/youtube/v3/search?q={searchKey}&key={Settings.YoutubeKey}&maxResults=1&type=video";
                HttpResponseMessage response = await http.GetAsync(query);
                if (response.IsSuccessStatusCode)
                {
                    string resp = await response.Content.ReadAsStringAsync();
                    YouTubeReply yr = JsonConvert.DeserializeObject<YouTubeReply>(resp);
                    string r = $"https://www.youtube.com/watch?v={yr.Items[0].id.videoId}";
                    return r;
                }
                return "";
            }
        }
    }
}