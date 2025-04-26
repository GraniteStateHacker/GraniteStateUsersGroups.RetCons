using GraniteStateUsersGroups.RetCons.Tests.Interfaces;
using GraniteStateUsersGroups.RetCons.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GraniteStateUsersGroups.RetCons.Tests.Implementations;



//Implementation for IExemplifyInterface selected at startup based on presence of RequiredConfigKey in standard (appsettings.json) configuration.


[RetCon.Default(typeof(IClass1))]
[RetCon.WhenConfigured(typeof(IClass2), "DependencyßConfig")]
public class DualInterfaceImplementation : IClass1 , IClass2
{
    
}



[RetCon.WhenConfigured(typeof(IClass1), "DependencyAlphaConfig")]
public class DependencyAlphaImplementation: IClass1
{
}


[RetCon.WhenConfigured(null, "DependencyßConfig")]
public class ConfigurationWhenConfigured 
{
    public static List<string> SideEffectsLog { get; } = new();
    public class Config : ISelfConfig, ISelfConfigAfterBuild
    {
        public void Configure(WebApplicationBuilder builder, RetCon.RetConBaseAttribute attribute, IConfiguration configuration, ILogger logger)
        {
            SideEffectsLog.Add($"{nameof(Config)}.{nameof(Configure)} method called for {nameof(ConfigurationWhenConfigured)}.");
        }
        public void PostBuildConfig(IApplicationBuilder app, RetCon.RetConBaseAttribute attribute, IConfiguration? configuration, ILogger logger)
        {
            SideEffectsLog.Add($"{nameof(Config)}.{nameof(PostBuildConfig)} method called for {nameof(ConfigurationWhenConfigured)}.");
        }
    }
}


