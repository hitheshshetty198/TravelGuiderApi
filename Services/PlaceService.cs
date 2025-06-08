using TravelGuiderAPI.Models;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq; // for JObject


namespace TravelGuiderAPI.Services
{
    public class PlaceService : IPlaceService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public PlaceService(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;
        }

        public async Task<Place?> GetPlaceByNameAsync(string name)
        {
            var path = Path.Combine(_env.ContentRootPath, "placesData.json");
            var json = await File.ReadAllTextAsync(path);
            var data = JsonConvert.DeserializeObject<PlaceData>(json);
            return data.Places.FirstOrDefault(p =>
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<WeatherResponse?> GetWeatherAsync(string city)
        {
            var apiKey = _config["OpenWeatherMap:ApiKey"];
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

            using var client = new HttpClient();
            var res = await client.GetAsync(url);
            if (!res.IsSuccessStatusCode) return null;

            var content = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<WeatherResponse>(content);
        }
        public async Task<WeatherResponse?> GetWeatherByPlaceNameAsync(string placeName)
        {
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "Data/places.json");
            var json = await File.ReadAllTextAsync(jsonPath);
            var root = JsonDocument.Parse(json).RootElement;

            var places = root.GetProperty("places");

            foreach (var state in places.EnumerateArray())
            {
                string stateName = state.GetProperty("name").GetString();

                foreach (var location in state.GetProperty("locations").EnumerateArray())
                {
                    string locationName = location.GetProperty("name").GetString();

                    if (string.Equals(locationName, placeName, StringComparison.OrdinalIgnoreCase))
                    {
                        // 🎯 Found the match, now get weather for its parent state (city)
                        string apiKey = _config["OpenWeatherMap:ApiKey"];
                        string url = $"https://api.openweathermap.org/data/2.5/weather?q={stateName}&appid={apiKey}&units=metric";

                        using var client = new HttpClient();
                        var response = await client.GetAsync(url);

                        if (!response.IsSuccessStatusCode) return null;

                        var content = await response.Content.ReadAsStringAsync();
                        var weatherData = JObject.Parse(content);



                        float temp = weatherData["main"]?["temp"]?.Value<float>() ?? 0;
                        string condition = weatherData["weather"]?[0]?["description"]?.Value<string>() ?? "Not available";


                        return new WeatherResponse
                        {
                            Temperature = temp,
                            Condition = condition,
                            Suggestion = temp switch
                            {
                                < 10 => "Too cold to travel",
                                < 25 => "Ideal for travel",
                                _ => "It may be too hot, carry water and light clothing"
                            }
                        };

                    }
                }
            }

            return null; // Not found
        }

    }

}
