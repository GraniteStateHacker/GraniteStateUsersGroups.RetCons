using GraniteStateUsersGroups.RetCons.Tests.Interfaces;
using GraniteStateUsersGroups.RetCons.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GraniteStateUsersGroups.RetCons.Tests.Implementationsß;


[RetCon.Default(typeof(IClass2))]
[RetCon.WhenConfigured(typeof(IClass1), nameof(DependencyßConfig))]
public class DualInterfaceßImplementation : IClass1, IClass2
{
    private readonly DependencyßConfig _config;

    public DependencyßConfig GetConfig() => _config;

    public DualInterfaceßImplementation(IOptions<DependencyßConfig> config)
    {
        _config = config.Value;
    }

    public class Configurator : ISelfConfig
    {

        public void Configure(WebApplicationBuilder builder, RetCon.RetConBaseAttribute attribute, IConfiguration configuration, ILogger logger)
        {

            builder.Services.Configure<DependencyßConfig>(configuration.GetSection(nameof(DependencyßConfig)));
        }
    }
}