using TravelGuiderAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddScoped<IPlaceService, PlaceService>();
builder.Services.AddScoped<ITripService, TripService>();


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add config for OpenWeatherMap
builder.Configuration.AddJsonFile("appsettings.json");


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000",
                "https://tripnests.netlify.app")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello from Render!");

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
//app.Urls.Add($"http://0.0.0.0:{port}");

app.UseCors("AllowReactApp");


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
