using TravelGuiderAPI.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IPlaceService, PlaceService>();
builder.Services.AddScoped<ITripService, TripService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello from Render!");

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";

app.UseCors("AllowReactApp");


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
