using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TravelGuiderAPI.Models;

namespace TravelGuiderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public AuthController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            var path = Path.Combine(_env.ContentRootPath, "Data/users.json");
            var users = JsonConvert.DeserializeObject<List<User>>(System.IO.File.ReadAllText(path)) ?? new();

            if (users.Any(u => u.Email == user.Email))
                return BadRequest("Email already registered");

            users.Add(user);
            System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(users, Formatting.Indented));

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            var userPath = Path.Combine(_env.ContentRootPath, "Data/users.json");
            var sessionPath = Path.Combine(_env.ContentRootPath, "Data/sessions.json");

            var users = JsonConvert.DeserializeObject<List<User>>(System.IO.File.ReadAllText(userPath)) ?? new();

            var existingUser = users.FirstOrDefault(u => u.Email == user.Email && u.Password == user.Password);
            if (existingUser == null)
                return Unauthorized("Invalid credentials");

            var sessions = JsonConvert.DeserializeObject<List<Session>>(System.IO.File.ReadAllText(sessionPath)) ?? new();

            var token = Guid.NewGuid().ToString();

            sessions.Add(new Session
            {
                Email = user.Email,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });

            System.IO.File.WriteAllText(sessionPath, JsonConvert.SerializeObject(sessions, Formatting.Indented));

            return Ok(new { Token = token });
        }
    }

}
