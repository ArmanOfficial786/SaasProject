using Hrm.Api.Extensions;
using Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

// Add HRM DbContext with connection string
var hrmConnectionString = builder.Configuration.GetConnectionString("HrmConnection")
    ?? throw new InvalidOperationException("Connection string 'HrmConnection' not found.");
builder.Services.AddHrmDbContext(hrmConnectionString);


// Add Shared.Infrastructure services (UnitOfWork, Repository pattern)
builder.Services.AddSharedInfrastructure();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

// Seed the database with initial data - handle errors gracefully
try
{
    await app.InitialiseDatabaseAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning(ex, "Database seeding encountered an error, but the application will continue running.");
}


app.Run();



