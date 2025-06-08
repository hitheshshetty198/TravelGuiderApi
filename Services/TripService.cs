
using Newtonsoft.Json;
using TravelGuiderAPI.Models;
namespace TravelGuiderAPI.Services
{

    public class TripService : ITripService
    {
        private readonly IWebHostEnvironment _env;

        public TripService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<object> GenerateTripPlanAsync(TripRequest request)
        {
            var path = Path.Combine(_env.ContentRootPath, "placesData.json");
            var json = await File.ReadAllTextAsync(path);
            var data = JsonConvert.DeserializeObject<PlaceData>(json);
            var place = data.Places.FirstOrDefault(p => p.Name.Equals(request.Place, StringComparison.OrdinalIgnoreCase));

            if (place == null) return "Place not found";

            int totalDays = (request.EndDate - request.StartDate).Days + 1;
            var itinerary = new List<object>();

            for (int i = 0; i < totalDays; i++)
            {
                var day = request.StartDate.AddDays(i);
                var loc = place.Locations[i % place.Locations.Count];
                itinerary.Add(new
                {
                    Date = day.ToString("yyyy-MM-dd"),
                    Visit = loc.Name,
                    Meal = new { Breakfast = "Local Café", Lunch = "Top-rated Restaurant", Dinner = "Hotel Buffet" },
                    Stay = place.Stays[request.StayType].FirstOrDefault(),
                    Transport = place.Transportation[i % place.Transportation.Count],
                    Dress = place.Dress_Recommendation
                });
            }

            return new
            {
                Place = place.Name,
                TotalDays = totalDays,
                Itinerary = itinerary
            };
        }
    }

}
