using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Dtos
{
    public class WeatherReply
    {
        public Location location { get; set; }
        public Current current { get; set; }

        public Forecast forecast { get; set; }
    }

    public class Location
    {
        public string name { get; set; }
        public string region { get; set; }
        public string country { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public string tz_id { get; set; }

        public int localtime_epoc { get; set; }
        public DateTime localtime { get; set; }
    }

    public class Current
    {
        public int last_updated_epoch { get; set; }
        public DateTime last_updated { get; set; }
        public double temp_c { get; set; }
        public double temp_f { get; set; }
        public int is_day { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public string tz_id { get; set; }
        public Condition condition { get; set; }
        public double wind_mph { get; set; }
        public double feelslike_f { get; set; }

        public int humidity { get; set; }
    }


    public class Condition
    {
        public string text { get; set; }
        public string icon { get; set; }
        public int code { get; set; }
    }

    public class Forecast
    {
        public List<ForecastDay> ForecastDay { get; set; }
    }

    //public class ForecastDay
    //{
    //    public List<ForecastEntity> ForecastEntities { get; set; }
    //}

    public class ForecastDay
    {
        public DateTime date { get; set; }
        public Day day { get; set; }

    }

    public class Day
    {
        public double maxtemp_f { get; set; }
        public double mintemp_f { get; set; }
        public double avgtemp_f { get; set; }
        public double totalprecip_in { get; set; }
        public int daily_chance_of_rain { get; set; }

        public int daily_chance_of_snow { get; set; }
        public double avghumidity { get; set; }
        public Condition condition { get; set; }
    }
}
