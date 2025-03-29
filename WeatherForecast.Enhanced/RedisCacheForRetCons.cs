using GraniteStateUsersGroups.RetCons;
using GraniteStateUsersGroups.RetCons.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace WeatherForecast.Enhanced;


[RetCon.WhenConfigured(typeof(IDistributedCache), "RedisCache", ServiceKey = "VolatileCache")]
public class RedisCacheForRetCons(IOptions<RedisCacheOptions> optionsAccessor) : RedisCache(optionsAccessor)
{
    public class Configurator : ISelfConfig
    {
        public void Configure(WebApplicationBuilder builder, RetCon.RetConBaseAttribute attribute, IConfiguration config, ILogger logger)
        {
            builder.Services.Configure<RedisCacheOptions>(config.GetSection(nameof(RedisCacheOptions)));
        }
    }
}