using GraniteStateUsersGroups.RetCons;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WeatherForecast.Enhanced;

[RetCon(typeof(IRetConArranger), ServiceKey = "VolatileCache")]
public class EnvironmentBasedRetConArrangerForRedis : EnvironmentBasedRetConArranger
{
    private readonly RedisCacheOptions _redisCacheOptions;

    public EnvironmentBasedRetConArrangerForRedis(
            IOptions<RedisCacheOptions> redisCacheOptions,
            [FromKeyedServices("RetConServiceKey")] IHostApplicationBuilder builder,
            [FromKeyedServices("RetConServiceKey")] ILogger logger) : base(builder, logger)
    {
        _redisCacheOptions = redisCacheOptions.Value;
    }


    public override void ConfigureService(RetConAttribute attribute, Type targetType)
    {
            
        _builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = _redisCacheOptions.Configuration;
            options.InstanceName = _redisCacheOptions.InstanceName;
        });

    }


}