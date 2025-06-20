namespace TravelGuiderAPI.Models
{
    public class LocationInfo
    {
        public string Name { get; set; }
        public string Photo { get; set; }
        public string Location { get; set; }
        public string Timing { get; set; }
        public decimal Ticket_Price { get; set; }
        public double Estimated_Visit_Time_Hours { get; set; }
        public List<string> Nearby_Places { get; set; }
    }

    public class Place
    {
        public string Name { get; set; }
        public List<string> Category { get; set; }
        public List<string> Best_Months { get; set; }
        public List<LocationInfo> Locations { get; set; }
        public Dictionary<string, List<string>> Stays { get; set; }
        public List<string> Transportation { get; set; }
        public string Dress_Recommendation { get; set; }
    }

    public class PlaceData
    {
        public List<Place> Places { get; set; }
    }

}
