using Microsoft.AspNetCore.Mvc;
using TravelGuiderAPI.Services;

namespace TravelGuiderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlacesController : ControllerBase
    {
        private readonly PlaceService _placeService;

        public PlacesController(PlaceService placeService)
        {
            _placeService = placeService;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_placeService.GetAll());

        [HttpGet("search")]
        public IActionResult Search(string? name, string? month, [FromQuery] string[]? category)
        {
            var result = _placeService.Search(name, month, category);
            return Ok(result);
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetByName(string name, [FromServices] WeatherService weatherService)
        {
            var place = _placeService.GetByName(name);
            if (place == null) return NotFound();

            var weatherAdvice = await weatherService.GetWeatherAdvice(place.Location);

            return Ok(new
            {
                Place = place,
                WeatherAdvice = weatherAdvice
            });
        }

        [HttpGet("{name}/nearby")]
        public IActionResult GetNearby(string name)
        {
            var place = _placeService.GetByName(name);
            if (place == null) return NotFound();

            var nearby = _placeService.GetAll().Where(p => place.NearbyPlaces.Contains(p.Name)).ToList();
            return Ok(nearby);
        }
    }
}
