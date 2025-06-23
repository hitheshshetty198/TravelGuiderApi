namespace TravelGuiderAPI.Models
{
    public class WeatherResponse
    {
        public MainWeather Main { get; set; }
        public List<Weather> Weather { get; set; }
        public float Temperature { get; set; }
        public string Condition { get; set; }
        public string Suggestion { get; set; }
    }

    public class MainWeather
    {
        public float Temp { get; set; }
    }

    public class Weather
    {
        public string Description { get; set; }
    }

}
