namespace TravelGuiderAPI.Models
{
    public class User
    {
        public string Email { get; set; }
        public string Password { get; set; } // Optional: hash later
    }

    public class Session
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class SavedTrip
    {
        public string Email { get; set; }
        public object Trip { get; set; } // use the object to store the itinerary response
    }

    public class TripSaveRequest
    {
        public object Trip { get; set; }
    }
}
