using GraniteStateUsersGroups.RetCons;
using GraniteStateUsersGroups.RetCons.Web;
using Microsoft.AspNetCore.Mvc;
using WeatherForecast.Abstractions.Interfaces;
using WeatherForecast.Classic;
using WeatherForecast.Enhanced;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddRetConTargetServices(RetConDiscoveryLevel.RequireSignedAssemblies);

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRetConTargetServices();

app.MapGet("/weatherforecast", ([FromServices] IWeatherForecaster forecaster) => forecaster.Forecast())
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/weatherforecast/{zipcode}", ([FromServices] IWeatherForecaster forecaster, string zipcode) => forecaster.ForecastForZipCode(zipcode));

app.Run();
