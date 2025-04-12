using GraniteStateUsersGroups.RetCons.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GraniteStateUsersGroups.RetCons.Cache.MemoryCache;


[RetCon.WhenConfigured(typeof(IDistributedCache),"MemoryCache", Lifetime = ServiceLifetime.Singleton)]

[RetCon.Default(typeof(IDistributedCache), Lifetime = ServiceLifetime.Singleton, ServiceKey = "VolatileCache")]
public class DistributedMemoryCacheWrapperForRetCons(IOptions<MemoryDistributedCacheOptions>? optionsAccessor) : MemoryDistributedCache(optionsAccessor!)
{
    public class Configurator : ISelfConfig, ISelfConfigAfterBuild
    {


        public void Configure(WebApplicationBuilder builder, RetCon.RetConBaseAttribute attribute, IConfiguration configuration, ILogger logger)
        {
            logger.LogInformation("Configuring Distributed Memory Cache before call to builder.Build() for service key '{serviceKey}'.", attribute.ServiceKey);
        }

        public void PostBuildConfig(IApplicationBuilder app, RetCon.RetConBaseAttribute attribute, IConfiguration? configuration, ILogger logger)
        {
            logger.LogInformation("Initializing Distributed Memory Cache after call to builder.Build() for service key '{serviceKey}'.", attribute.ServiceKey);
        }
    }
}