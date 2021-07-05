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
            using HttpClient http = new HttpClient();
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

        public async Task<string> GetForecast(string zip)
        {
            using HttpClient http = new HttpClient();
            string query = $"http://api.weatherapi.com/v1/forecast.json?key={Settings.WeatherKey}&q={zip}";
            HttpResponseMessage response = await http.GetAsync(query);
            if (response.IsSuccessStatusCode)
            {

                string resp = await response.Content.ReadAsStringAsync();
                WeatherReply wr = JsonConvert.DeserializeObject<WeatherReply>(resp);
                Day forecast = wr.forecast.ForecastDay[0].day;
                string r = "Forecast for " + wr.location.name + ", " + wr.location.region + " - " + wr.forecast.ForecastDay[0].date
                    + ": " + forecast.condition.text + ". High temp: " + forecast.maxtemp_f + "F. Low Temp: " + forecast.mintemp_f + "F. Humiditiy: " 
                    + forecast.avghumidity + "%. Rain Chance: " + forecast.daily_chance_of_rain + "%.";
                return r;
            }
            return "";
        }
    }
}
