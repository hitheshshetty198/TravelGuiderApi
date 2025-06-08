using TravelGuiderAPI.Models;

namespace TravelGuiderAPI.Services
{
    public interface ITripService
    {
        Task<object> GenerateTripPlanAsync(TripRequest request);
    }

}
