namespace TravelGuiderAPI.Models
{
    public class User
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class Session
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class SavedTrip
    {
        public string Id { get; set; }  
        public string Email { get; set; }
        public TripPlan Trip { get; set; }
        public decimal Budget { get; set; }
        public string StayType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }


    public class TripSaveRequest
    {
        public TripPlan Trip { get; set; }
        
    }
}
