using GraniteStateUsersGroups.RetCons;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;

namespace WeatherForecast.Enhanced;

[RetCon(typeof(IDistributedCache),
    ArrangerKey = "Environmental",
    ArrangerArguments = ["QA", "UAT" ,"Production"])]

[RetCon(typeof(IDistributedCache),
    ServiceKey = "VolatileCache",
    ArrangerKey = "VolatileCache",
    ArrangerArguments = ["Development"])]
public class RedisCacheForRetCons : RedisCache
{
    public RedisCacheForRetCons(IOptions<RedisCacheOptions> optionsAccessor) : base(optionsAccessor)
    {
    }
}