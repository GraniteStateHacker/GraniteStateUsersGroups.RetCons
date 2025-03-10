using GraniteStateUsersGroups.RetCons;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace WeatherForecast.Enhanced;


[RetCon(typeof(IDistributedCache), 
    ArrangerKey = "Environmental",
    ArrangerArguments = ["Development"])]

[RetCon(typeof(IDistributedCache),
    ServiceKey = "VolatileCache")]
public class DistributedMemoryCacheWrapperForRetCons : MemoryDistributedCache
{
    public DistributedMemoryCacheWrapperForRetCons(IOptions<MemoryDistributedCacheOptions>? optionsAccessor) : base(optionsAccessor)
    {
    }
}
