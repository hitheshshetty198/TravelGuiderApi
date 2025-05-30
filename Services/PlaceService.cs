using TravelGuiderAPI.Models;
using System.Text.Json;

namespace TravelGuiderAPI.Services
{
    public class PlaceService
    {
        private readonly List<Place> _places;

        public PlaceService()
        {
            var json = File.ReadAllText("Data/places.json");
            _places = JsonSerializer.Deserialize<List<Place>>(json);
        }

        public List<Place> GetAll() => _places;

        public List<Place> Search(string name = null, string month = null, string[] category = null)
        {
            return _places.Where(p =>
                (string.IsNullOrEmpty(name) || p.Name.Contains(name, StringComparison.OrdinalIgnoreCase) || p.Location.Contains(name, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(month) || p.BestMonthToVisit.Contains(month, StringComparison.OrdinalIgnoreCase)) &&
                (category == null || category.Any(c => p.Categories.Contains(c, StringComparer.OrdinalIgnoreCase)))
            ).ToList();
        }

        public Place GetByName(string name) =>
            _places.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
