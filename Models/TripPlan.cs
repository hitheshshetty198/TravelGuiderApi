namespace TravelGuiderAPI.Models
{
    public class TripPlan
    {
        public string Place { get; set; }
        public int Days { get; set; }
        public List<ItineraryItem> Itinerary { get; set; }
    }

    public class ItineraryItem
    {
        public string Date { get; set; }
        public string Visit { get; set; }
        public string Address { get; set; }
        public Meal Meal { get; set; }
        public string Stay { get; set; }
        public string Transport { get; set; }
        public string Dress { get; set; }
    }

    public class Meal
    {
        public string Breakfast { get; set; }
        public string Lunch { get; set; }
        public string Dinner { get; set; }
    }
}
