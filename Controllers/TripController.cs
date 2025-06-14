using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TravelGuiderAPI.Models;

namespace TravelGuiderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TripController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public TripController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("make-my-trip")]
        public IActionResult MakeMyTrip([FromBody] TripRequest request)
        {
            var filePath = Path.Combine(_env.ContentRootPath, "Data/places.json");
            var json = System.IO.File.ReadAllText(filePath);
            var data = JsonConvert.DeserializeObject<PlaceData>(json);

            var placeInfo = data.Places.FirstOrDefault(p =>
                string.Equals(p.Name, request.Place, StringComparison.OrdinalIgnoreCase));

            if (placeInfo == null) return NotFound("Place not found");

            var totalDays = (request.EndDate - request.StartDate).Days + 1;
            var itinerary = new List<ItineraryItem>();


            for (int i = 0; i < totalDays; i++)
            {
                var day = request.StartDate.AddDays(i);
                var location = placeInfo.Locations[i % placeInfo.Locations.Count];

                itinerary.Add(new ItineraryItem
                {
                    Date = day.ToString("yyyy-MM-dd"),
                    Visit = location.Name,
                    Address = location.Location,
                    Meal = new Meal
                    {
                        Breakfast = "Local Dhaba",
                        Lunch = "Recommended Restaurant",
                        Dinner = "Hotel Restaurant"
                    },
                    Stay = placeInfo.Stays[request.StayType].FirstOrDefault(),
                    Transport = placeInfo.Transportation[i % placeInfo.Transportation.Count],
                    Dress = placeInfo.Dress_Recommendation
                });
            }


            var trip = new TripPlan
            {
                Place = request.Place,
                Days = totalDays,
                Itinerary = itinerary
            };

            return Ok(trip);

        }

        [HttpPost("save-trip")]
        public IActionResult SaveTrip([FromQuery] string token, [FromBody] TripSaveRequest request)
        {
            var sessionPath = Path.Combine(_env.ContentRootPath, "Data/sessions.json");
            var tripPath = Path.Combine(_env.ContentRootPath, "Data/savedTrips.json");

            var sessions = JsonConvert.DeserializeObject<List<Session>>(System.IO.File.ReadAllText(sessionPath)) ?? new();
            var session = sessions.FirstOrDefault(s => s.Token == token && s.ExpiresAt > DateTime.UtcNow);
            if (session == null)
                return Unauthorized("Invalid or expired session");

            var savedTrips = JsonConvert.DeserializeObject<List<SavedTrip>>(System.IO.File.ReadAllText(tripPath)) ?? new();

            savedTrips.Add(new SavedTrip
            {
                Email = session.Email,
                Trip = request.Trip
            });

            System.IO.File.WriteAllText(tripPath, JsonConvert.SerializeObject(savedTrips, Formatting.Indented));

            return Ok("Trip saved successfully");
        }


        [HttpGet("get-saved-trips")]
        public IActionResult GetSavedTrips([FromQuery] string token)
        {
            var sessionPath = Path.Combine(_env.ContentRootPath, "Data/sessions.json");
            var tripPath = Path.Combine(_env.ContentRootPath, "Data/savedTrips.json");

            var sessions = JsonConvert.DeserializeObject<List<Session>>(System.IO.File.ReadAllText(sessionPath)) ?? new();

            var session = sessions.FirstOrDefault(s => s.Token == token && s.ExpiresAt > DateTime.UtcNow);
            if (session == null)
                return Unauthorized("Invalid or expired session");

            var savedTrips = JsonConvert.DeserializeObject<List<SavedTrip>>(System.IO.File.ReadAllText(tripPath)) ?? new();

            var userTrips = savedTrips.Where(t => t.Email == session.Email).ToList();

            return Ok(userTrips);
        }


    }

}
