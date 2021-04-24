using IRSeaBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace IRSeaBot.Services
{
    public class WeatherService
    {
        public async Task<string> GetWeather(string zip)
        {
            using(HttpClient http = new HttpClient())
            {
                string query = $"http://api.weatherapi.com/v1/current.json?key={Settings.WeatherKey}&q={zip}";
                HttpResponseMessage response = await http.GetAsync(query);
                if (response.IsSuccessStatusCode)
                {
                    string resp = await response.Content.ReadAsStringAsync();
                    WeatherReply wr = JsonConvert.DeserializeObject<WeatherReply>(resp);
                    string r = "Weather in " + wr.location.name + ", " + wr.location.region + " is " + wr.current.condition.text + " and " + wr.current.temp_f + ", it feels like " + wr.current.feelslike_f + ".";
                    return r;
                }
                return "";
            }

        }
    }
}
