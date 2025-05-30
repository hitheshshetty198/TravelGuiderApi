using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TravelGuiderAPI.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "a0f6a3aab2fc7bfdcfe204ae567ac9d7"; 

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetWeatherAdvice(string city)
        {
            try
            {
                var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}&units=metric";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode) return "Weather info not available.";

                var content = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(content);
                var root = jsonDoc.RootElement;

                var temp = root.GetProperty("main").GetProperty("temp").GetDecimal();
                var weather = root.GetProperty("weather")[0].GetProperty("main").GetString();

                if (weather.Contains("Rain") || weather.Contains("Thunderstorm"))
                    return $"⛈️ It’s currently raining in {city}. Not a good time to visit.";

                if (weather.Contains("Snow"))
                    return $"❄️ Snowy in {city}. Travel may be difficult.";

                if (temp > 40)
                    return $"🔥 Very hot ({temp}°C). Not ideal for outdoor activities.";

                return $"✅ Weather in {city} is good ({weather}, {temp}°C). Perfect to visit!";
            }
            catch
            {
                return "Weather info not available.";
            }
        }
    }
}
