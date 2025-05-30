namespace TravelGuiderAPI.Models
{
    public class Place
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string ImageUrl { get; set; }
        public string EntryTime { get; set; }
        public string ExitTime { get; set; }
        public string EntryFee { get; set; }
        public string EstimatedVisitTime { get; set; }
        public string BestMonthToVisit { get; set; }
        public string[] Categories { get; set; }
        public string[] NearbyPlaces { get; set; }
        public string[] Stays { get; set; }
    }
}
