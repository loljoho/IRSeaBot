using IRSeaBot.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace IRSeaBot.Services
{
    public class ForecastService : IBotService
    {
        public async Task<string> Get(string zip, string replyTo)
        {
            using HttpClient http = new HttpClient();
            string query = $"http://api.weatherapi.com/v1/forecast.json?key={Settings.WeatherKey}&q={zip}&days=1";
            HttpResponseMessage response = await http.GetAsync(query);
            if (response.IsSuccessStatusCode)
            {

                string resp = await response.Content.ReadAsStringAsync();
                WeatherReply wr = JsonConvert.DeserializeObject<WeatherReply>(resp);
                Day forecast = wr.forecast.ForecastDay[0].day;
                string r = "Forecast for " + wr.location.name + ", " + wr.location.region + " - " + wr.forecast.ForecastDay[0].date
                    + ": " + forecast.condition.text + ". High temp: " + forecast.maxtemp_f + "F. Low Temp: " + forecast.mintemp_f + "F. Humiditiy: "
                    + forecast.avghumidity + "%. Rain Chance: " + forecast.daily_chance_of_rain + "%.";
                string reply = $"PRIVMSG {replyTo} {r}";
                return reply;
            }
            return "";
        }
    }
}
