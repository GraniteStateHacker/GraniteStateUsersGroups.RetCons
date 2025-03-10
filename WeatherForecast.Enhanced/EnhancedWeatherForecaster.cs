using Microsoft.Extensions.Caching.Distributed;
using WeatherForecast.Abstractions.Interfaces;
using WeatherForecast.Abstractions.Common;
using GraniteStateUsersGroups.RetCons;

namespace WeatherForecast.Enhanced;


[RetCon(typeof(IWeatherForecaster), 
    ArrangerKey = "Environmental", 
    ArrangerArguments = ["Development"])]
public class EnhancedWeatherForecaster : IWeatherForecaster
{
    private IDistributedCache _cache;

    public EnhancedWeatherForecaster(IDistributedCache cache)
    {
        _cache = cache;
    }

    public Abstractions.Models.WeatherForecast[] Forecast()
    {
        return ForecastForZipCode("");
    }

    public Abstractions.Models.WeatherForecast[] ForecastForZipCode(string Zipcode)
    {
        return _cache.Cache($"forecast:{Zipcode}", () =>
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
                new Abstractions.Models.WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    Constants.Summaries[Random.Shared.Next(Constants.Summaries.Length)]
                ))
                .ToArray();

            return forecast;
        });
    }
}
