using GraniteStateUsersGroups.RetCons;
using GraniteStateUsersGroups.RetCons.Web;
using Microsoft.AspNetCore.Mvc;
using WeatherForecast.Abstractions.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add RetCon configuration logic
builder.AddRetConTargetServices(RetConDiscoveryLevel.RequireSignedAssemblies);

var app = builder.Build();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use RetCon post-build configuration logic
app.UseRetConTargetServices();

app.MapGet("/weatherforecast", ([FromServices] IWeatherForecaster forecaster) => forecaster.Forecast())
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();
