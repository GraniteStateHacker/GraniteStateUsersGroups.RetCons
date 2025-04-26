using GraniteStateUsersGroups.RetCons.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GraniteStateUsersGroups.RetCons.Tests.ImplementationsB;

[RetCon.Default(null)]
public class DefaultConfiguration
{
    public static List<string> SideEffectsLog { get; } = new();

    public class Config : ISelfConfig, ISelfConfigAfterBuild
    {
        public void Configure(WebApplicationBuilder builder, RetCon.RetConBaseAttribute attribute, IConfiguration configuration, ILogger logger)
        {
            SideEffectsLog.Add($"{nameof(Config)}.{nameof(Configure)} method called for {nameof(DefaultConfiguration)}.");
        }

        public void PostBuildConfig(IApplicationBuilder app, RetCon.RetConBaseAttribute attribute, IConfiguration? configuration, ILogger logger)
        {
            SideEffectsLog.Add($"{nameof(Config)}.{nameof(PostBuildConfig)} method called for {nameof(DefaultConfiguration)}.");
        }
    }
}
