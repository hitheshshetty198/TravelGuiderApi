using Microsoft.AspNetCore.Mvc;
using TravelGuiderAPI.Models;

namespace TravelGuiderAPI.Services
{
    public interface IPlaceService
    {
        Task<Place?> GetPlaceByNameAsync(string name);
        Task<WeatherResponse?> GetWeatherAsync(string city);
        Task<WeatherResponse?> GetWeatherByPlaceNameAsync(string placeName);

    }

}
