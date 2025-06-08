namespace TravelGuiderAPI.Models
{
    public class TripRequest
    {
        public string Place { get; set; }
        public List<string> Categories { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StayType { get; set; }
        public int Budget { get; set; }
    }

}
