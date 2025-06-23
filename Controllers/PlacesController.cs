using Microsoft.AspNetCore.Mvc;
using TravelGuiderAPI.Models;
using TravelGuiderAPI.Services;
using Newtonsoft.Json;
using System.Net.Http;

namespace TravelGuiderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlacesController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly IPlaceService _placeService;


        public PlacesController(IWebHostEnvironment env, IConfiguration config, IPlaceService placeService)
        {
            _env = env;
            _config = config;
            _placeService = placeService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string name)
        {
            var path = Path.Combine(_env.ContentRootPath, "Data/places.json");
            var json = System.IO.File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<PlaceData>(json);

            // 1. Try to find by place/state name
            var place = data.Places.FirstOrDefault(p =>
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (place != null)
            {
                // Get weather using place name
                string apiKey = _config["OpenWeatherMap:ApiKey"];
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={place.Name}&appid={apiKey}&units=metric";

                using var client = new HttpClient();
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                float temperature = 0;
                string condition = "N/A";
                string suggestion = "No weather suggestion";

                if (response.IsSuccessStatusCode)
                {
                    var weather = JsonConvert.DeserializeObject<WeatherResponse>(content);
                    temperature = weather.Main.Temp;
                    condition = weather.Weather.FirstOrDefault()?.Description ?? "Unknown";
                    suggestion = temperature > 35 ? "Too hot to travel" :
                                 temperature < 10 ? "Too cold, pack warm clothes" :
                                 "Great time to visit";
                }

                return Ok(new
                {
                    name = place.Name,
                    category = place.Category,
                    best_Months = place.Best_Months,
                    locations = place.Locations.Select(loc => new
                    {
                        name = loc.Name,
                        photo = loc.Photo,
                        location = loc.Location,
                        timing = loc.Timing,
                        ticket_Price = loc.Ticket_Price,
                        estimated_Visit_Time_Hours = loc.Estimated_Visit_Time_Hours,
                        nearby_Places = loc.Nearby_Places
                    }).ToList(),
                    stays = place.Stays,
                    transportation = place.Transportation,
                    dress_Recommendation = place.Dress_Recommendation,
                    weather = new
                    {
                        temperature,
                        condition,
                        suggestion
                    }
                });
            }

            // 2. Try to find by a location inside a place
            place = data.Places.FirstOrDefault(p =>
                p.Locations.Any(loc => loc.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));

            if (place != null)
            {
                var location = place.Locations.First(l =>
                    l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                string city = place.Name;

                string apiKey = _config["OpenWeatherMap:ApiKey"];
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

                using var client = new HttpClient();
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                float temperature = 0;
                string condition = "N/A";
                string suggestion = "No weather suggestion";

                if (response.IsSuccessStatusCode)
                {
                    var weather = JsonConvert.DeserializeObject<WeatherResponse>(content);
                    temperature = weather.Main.Temp;
                    condition = weather.Weather.FirstOrDefault()?.Description ?? "Unknown";
                    suggestion = temperature > 35 ? "Too hot to travel" :
                                 temperature < 10 ? "Too cold, pack warm clothes" :
                                 "Great time to visit";
                }

                return Ok(new
                {
                    name = location.Name,
                    photo = location.Photo,
                    location = location.Location,
                    timing = location.Timing,
                    ticket_Price = location.Ticket_Price,
                    estimated_Visit_Time_Hours = location.Estimated_Visit_Time_Hours,
                    nearby_Places = location.Nearby_Places,
                    best_Months = place.Best_Months,
                    stays = place.Stays,
                    transportation = place.Transportation,
                    dress_Recommendation = place.Dress_Recommendation,
                    weather = new
                    {
                        temperature,
                        condition,
                        suggestion
                    }
                });
            }

            return NotFound("Place or location not found");
        }

        [HttpGet("suggest")]
        public IActionResult Suggest(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Search query is empty");

            var path = Path.Combine(_env.ContentRootPath, "Data/places.json");
            var json = System.IO.File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<PlaceData>(json);

            query = query.Trim().ToLower();
            var matchedNames = data.Places
                .Select(p => p.Name)
                .Concat(data.Places.SelectMany(p => p.Locations.Select(loc => loc.Name)))
                .Where(name => name != null && name.ToLower().Contains(query))
                .Distinct()
                .ToList();

            if (!matchedNames.Any())
                return NotFound("No matching places found");

            return Ok(matchedNames);
        }


        [HttpGet("weather")]
        public async Task<IActionResult> GetWeather(string city)
        {
            string apiKey = _config["OpenWeatherMap:ApiKey"];
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var weather = JsonConvert.DeserializeObject<WeatherResponse>(content);

            return Ok(new
            {
                Temperature = weather.Main.Temp,
                Condition = weather.Weather.First().Description,
                Suggestion = weather.Main.Temp > 35 ? "Too hot to travel" :
                             weather.Main.Temp < 10 ? "Too cold, pack warm clothes" :
                             "Great time to visit"
            });
        }

        [HttpGet("weather-by-place")]
        public async Task<IActionResult> GetWeatherByPlace([FromQuery] string placeName)
        {
            var result = await _placeService.GetWeatherByPlaceNameAsync(placeName);
            if (result == null)
                return NotFound("Place not found or weather info not available");

            return Ok(result);
        }

        


    }
}
