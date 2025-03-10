using GraniteStateUsersGroups.RetCons;
using GraniteStateUsersGroups.RetCons.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WeatherForecast.Enhanced;


[RetCon(typeof(IRetConArranger), ServiceKey = "Environmental")]
public class EnvironmentBasedRetConArranger : DefaultRetConArranger
{
    public EnvironmentBasedRetConArranger(
            [FromKeyedServices("RetConServiceKey")] IHostApplicationBuilder builder, 
            [FromKeyedServices("RetConServiceKey")] ILogger logger) : base(builder, logger)
    {
    }

    public override bool CanRegister(RetConAttribute attribute)
    {
        return attribute.ArrangerArguments.Cast<string>().Contains(_builder.Environment.EnvironmentName);
    }

    
}
