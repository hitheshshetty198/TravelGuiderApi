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
            if (request == null)
                return BadRequest("Request cannot be null");

            if (string.IsNullOrWhiteSpace(request.Place))
                return BadRequest("Place name is required");

            if (request.StartDate > request.EndDate)
                return BadRequest("Start date must be before end date");

            if (request.Budget <= 0)
                return BadRequest("Budget must be greater than 0");

            var filePath = Path.Combine(_env.ContentRootPath, "Data/places.json");
            var json = System.IO.File.ReadAllText(filePath);
            var data = JsonConvert.DeserializeObject<PlaceData>(json);

            var placeInfo = data.Places.FirstOrDefault(p =>
                string.Equals(p.Name, request.Place, StringComparison.OrdinalIgnoreCase));

            if (placeInfo == null)
                return NotFound("Place not found");

            if (!placeInfo.Stays.ContainsKey(request.StayType))
                return BadRequest($"Stay type '{request.StayType}' not available for this place");

            var totalDays = (request.EndDate - request.StartDate).Days + 1;
            var totalLocations = placeInfo.Locations.Count;

            var itinerary = new List<object>();

            for (int i = 0; i < totalDays; i++)
            {
                var day = request.StartDate.AddDays(i);
                string visit;
                string address;
                string photo;

                if (i < totalLocations)
                {
                    var location = placeInfo.Locations[i];
                    visit = location.Name;
                    address = location.Location;
                    photo = location.Photo;
                }
                else
                {
                    visit = "Revisit previous places or leisure day";
                    address = "Free day for local exploration";
                    photo = "https://res.cloudinary.com/diutdhsh3/image/upload/v1749920097/freeday_w24k3l.png";
                    if(day.Date < request.EndDate.Date) {
                        itinerary.Add(new
                        {
                            Date = day.ToString("yyyy-MM-dd") + " To " + request.EndDate.ToString("yyyy-MM-dd"),
                            Visit = visit,
                            Address = address,
                            Photo = photo,
                            Meal = new { Breakfast = "Local Dhaba", Lunch = "Recommended Restaurant", Dinner = "Hotel Restaurant" },
                            Stay = placeInfo.Stays[request.StayType].FirstOrDefault(),
                            Transport = placeInfo.Transportation[i % placeInfo.Transportation.Count],
                            Dress = placeInfo.Dress_Recommendation
                        });
                        break;
                    }
                    else
                    {
                        itinerary.Add(new
                        {
                            Date = day.ToString("yyyy-MM-dd"),
                            Visit = visit,
                            Address = address,
                            Photo = photo,
                            Meal = new { Breakfast = "Local Dhaba", Lunch = "Recommended Restaurant", Dinner = "Hotel Restaurant" },
                            Stay = placeInfo.Stays[request.StayType].FirstOrDefault(),
                            Transport = placeInfo.Transportation[i % placeInfo.Transportation.Count],
                            Dress = placeInfo.Dress_Recommendation
                        });
                        break;
                    }
                    
                }

                itinerary.Add(new
                {
                    Date = day.ToString("yyyy-MM-dd"),
                    Visit = visit,
                    Address = address,
                    Photo = photo,
                    Meal = new { Breakfast = "Local Dhaba", Lunch = "Recommended Restaurant", Dinner = "Hotel Restaurant" },
                    Stay = placeInfo.Stays[request.StayType].FirstOrDefault(),
                    Transport = placeInfo.Transportation[i % placeInfo.Transportation.Count],
                    Dress = placeInfo.Dress_Recommendation
                });
            }

            return Ok(new
            {
                Place = request.Place,
                Days = totalDays,
                Itinerary = itinerary
            });
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
                Id = Guid.NewGuid().ToString(),  
                Email = session.Email,
                Trip = request.Trip
            });

            System.IO.File.WriteAllText(tripPath, JsonConvert.SerializeObject(savedTrips, Formatting.Indented));

            return Ok("Trip saved successfully");
        }


        [HttpGet("get-saved-trips")]
        public IActionResult GetSavedTrips([FromQuery] string token, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var sessionPath = Path.Combine(_env.ContentRootPath, "Data/sessions.json");
            var tripPath = Path.Combine(_env.ContentRootPath, "Data/savedTrips.json");

            var sessions = JsonConvert.DeserializeObject<List<Session>>(System.IO.File.ReadAllText(sessionPath)) ?? new();

            var session = sessions.FirstOrDefault(s => s.Token == token && s.ExpiresAt > DateTime.UtcNow);
            if (session == null)
                return Unauthorized("Invalid or expired session");

            var savedTrips = JsonConvert.DeserializeObject<List<SavedTrip>>(System.IO.File.ReadAllText(tripPath)) ?? new();

            var userTrips = savedTrips
                .Where(t => t.Email == session.Email)
                .OrderByDescending(t => t.Id) // sorting latest first based on ID (since ID is Guid)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                Page = page,
                PageSize = pageSize,
                TotalTrips = savedTrips.Count(t => t.Email == session.Email),
                Trips = userTrips
            });
        }


        [HttpDelete("delete-saved-trip")]
        public IActionResult DeleteSavedTrip([FromQuery] string token, [FromQuery] string tripId)
        {
            var sessionPath = Path.Combine(_env.ContentRootPath, "Data/sessions.json");
            var tripPath = Path.Combine(_env.ContentRootPath, "Data/savedTrips.json");

            var sessions = JsonConvert.DeserializeObject<List<Session>>(System.IO.File.ReadAllText(sessionPath)) ?? new();
            var session = sessions.FirstOrDefault(s => s.Token == token && s.ExpiresAt > DateTime.UtcNow);
            if (session == null)
                return Unauthorized("Invalid or expired session");

            var savedTrips = JsonConvert.DeserializeObject<List<SavedTrip>>(System.IO.File.ReadAllText(tripPath)) ?? new();

            var tripToDelete = savedTrips.FirstOrDefault(t => t.Id == tripId && t.Email == session.Email);
            if (tripToDelete == null)
                return NotFound("Trip not found");

            savedTrips.Remove(tripToDelete);
            System.IO.File.WriteAllText(tripPath, JsonConvert.SerializeObject(savedTrips, Formatting.Indented));

            return Ok("Trip deleted successfully");
        }

        [HttpPut("update-saved-trip")]
        public IActionResult UpdateSavedTrip([FromQuery] string token, [FromQuery] string tripId, [FromBody] UpdateTripRequest request)
        {
            var sessionPath = Path.Combine(_env.ContentRootPath, "Data/sessions.json");
            var tripPath = Path.Combine(_env.ContentRootPath, "Data/savedTrips.json");

            var sessions = JsonConvert.DeserializeObject<List<Session>>(System.IO.File.ReadAllText(sessionPath)) ?? new();
            var session = sessions.FirstOrDefault(s => s.Token == token && s.ExpiresAt > DateTime.UtcNow);
            if (session == null)
                return Unauthorized("Invalid or expired session");

            var savedTrips = JsonConvert.DeserializeObject<List<SavedTrip>>(System.IO.File.ReadAllText(tripPath)) ?? new();

            var tripToUpdate = savedTrips.FirstOrDefault(t => t.Id == tripId && t.Email == session.Email);
            if (tripToUpdate == null)
                return NotFound("Trip not found");

            // Regenerate trip based on new input
            var generatedTrip = GenerateTripFromRequest(request);
            tripToUpdate.Trip = generatedTrip;

            System.IO.File.WriteAllText(tripPath, JsonConvert.SerializeObject(savedTrips, Formatting.Indented));

            return Ok("Trip updated successfully");
        }

        private TripPlan GenerateTripFromRequest(UpdateTripRequest request)
        {
            var filePath = Path.Combine(_env.ContentRootPath, "Data/places.json");
            var json = System.IO.File.ReadAllText(filePath);
            var data = JsonConvert.DeserializeObject<PlaceData>(json);

            var placeInfo = data.Places.FirstOrDefault(p =>
                string.Equals(p.Name, request.Place, StringComparison.OrdinalIgnoreCase));

            if (placeInfo == null)
                throw new Exception("Place not found");

            if (!placeInfo.Stays.ContainsKey(request.StayType))
                throw new Exception($"Stay type '{request.StayType}' not available");

            var totalDays = (request.EndDate - request.StartDate).Days + 1;
            var totalLocations = placeInfo.Locations.Count;

            var itinerary = new List<ItineraryItem>();

            for (int i = 0; i < totalDays; i++)
            {
                var day = request.StartDate.AddDays(i);
                string visit;
                string address;
                string photo;

                if (i < totalLocations)
                {
                    var location = placeInfo.Locations[i];
                    visit = location.Name;
                    address = location.Location;
                    photo = location.Photo;
                }
                else
                {
                    visit = "Revisit previous places or leisure day";
                    address = "Free day for local exploration";
                    photo = "https://res.cloudinary.com/diutdhsh3/image/upload/v1749920097/freeday_w24k3l.png";
                    if (day.Date < request.EndDate.Date)
                    {
                        itinerary.Add(new
                        {
                            Date = day.ToString("yyyy-MM-dd") + " To " + request.EndDate.ToString("yyyy-MM-dd"),
                            Visit = visit,
                            Address = address,
                            Photo = photo,
                            Meal = new { Breakfast = "Local Dhaba", Lunch = "Recommended Restaurant", Dinner = "Hotel Restaurant" },
                            Stay = placeInfo.Stays[request.StayType].FirstOrDefault(),
                            Transport = placeInfo.Transportation[i % placeInfo.Transportation.Count],
                            Dress = placeInfo.Dress_Recommendation
                        });
                        break;
                    }
                    else
                    {
                        itinerary.Add(new
                        {
                            Date = day.ToString("yyyy-MM-dd"),
                            Visit = visit,
                            Address = address,
                            Photo = photo,
                            Meal = new { Breakfast = "Local Dhaba", Lunch = "Recommended Restaurant", Dinner = "Hotel Restaurant" },
                            Stay = placeInfo.Stays[request.StayType].FirstOrDefault(),
                            Transport = placeInfo.Transportation[i % placeInfo.Transportation.Count],
                            Dress = placeInfo.Dress_Recommendation
                        });
                        break;
                    }

                }

                itinerary.Add(new
                {
                    Date = day.ToString("yyyy-MM-dd"),
                    Visit = visit,
                    Address = address,
                    Photo = photo,
                    Meal = new { Breakfast = "Local Dhaba", Lunch = "Recommended Restaurant", Dinner = "Hotel Restaurant" },
                    Stay = placeInfo.Stays[request.StayType].FirstOrDefault(),
                    Transport = placeInfo.Transportation[i % placeInfo.Transportation.Count],
                    Dress = placeInfo.Dress_Recommendation
                });
            }

            return Ok(new
            {
                Place = request.Place,
                Days = totalDays,
                Itinerary = itinerary
            });
        }




    }

}
