using GraniteStateUsersGroups.RetCons;
using WeatherForecast.Abstractions.Common;
using WeatherForecast.Abstractions.Interfaces;

namespace WeatherForecast.Classic;


[RetCon.Default(typeof(IWeatherForecaster))]
public class WeatherForecaster : IWeatherForecaster
{
    public WeatherForecaster()
    {
    }


    public Abstractions.Models.WeatherForecast[] Forecast()
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
    }

    public Abstractions.Models.WeatherForecast[] ForecastForZipCode(string Zipcode) => Forecast();
    
}