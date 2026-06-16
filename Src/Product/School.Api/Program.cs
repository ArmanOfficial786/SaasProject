using Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add Shared.Application services (AutoMapper, Configuration, etc.)
//builder.Services.AddSharedApplication(builder.Configuration);

// Add Shared.Infrastructure services (UnitOfWork, Repository pattern)
builder.Services.AddSharedInfrastructure();

// Add School DbContext with connection string
var schoolConnectionString = builder.Configuration.GetConnectionString("SchoolConnection")
    ?? throw new InvalidOperationException("Connection string 'SchoolConnection' not found.");
builder.Services.AddSchoolDbContext(schoolConnectionString);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
