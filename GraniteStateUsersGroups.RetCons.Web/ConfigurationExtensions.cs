using System.Reflection;

namespace GraniteStateUsersGroups.RetCons.Web;
public static class ConfigurationExtensions
{
       

    public static WebApplicationBuilder AddRetConTargetServices(this WebApplicationBuilder builder, AssemblyDiscoveryStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(builder);
        RetConComposer.RegisterRetConServices(builder, strategy);
        ConfigRetConTargetServices(builder);
        return builder;
    }


    public static WebApplication UseRetConTargetServices(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        InitializeRetConTargetServices(app);
        return app;
    }

    private static void ConfigRetConTargetServices(WebApplicationBuilder builder)
    {
        var logger = RetCon.Context.Logger!;
        var configuration = builder.Configuration;

        foreach (var set in RetCon.Context.RegisteredRetCons)
        {
            var implementation = set.TargetImplementation;
            var selfConfigSubClasses = implementation.GetNestedTypes(BindingFlags.Public).Where(t => t.IsAssignableTo(typeof(ISelfConfig))).FirstOrDefault();
            if (selfConfigSubClasses != null)
            {
                if (Activator.CreateInstance(selfConfigSubClasses) is ISelfConfig configurator)
                {
                    try
                    {
                        logger.Log(RetConComposer.LogLevel, "RetCon: starting configuration of {subclass}.", selfConfigSubClasses.AssemblyQualifiedName);
                        configurator.Configure(builder, set.Attribute, configuration, logger);
                        logger.Log(RetConComposer.LogLevel, "RetCon: finished configuration of {subclass}.", selfConfigSubClasses.AssemblyQualifiedName);
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, "RetCon: failed configuration of {subclass}. Message = '{message}'.", selfConfigSubClasses.AssemblyQualifiedName, ex.Message);
                    }
                }
            }
        }
    }


    private static void InitializeRetConTargetServices(IApplicationBuilder app)
    {
        var logger = RetCon.Context.Logger!;
        var configuration = RetCon.Context.ConfigurationBuilder!.Build();
        foreach (var set in RetCon.Context.RegisteredRetCons)
        {
            var implementation = set.TargetImplementation;
            var selfConfigSubClasses = implementation.GetNestedTypes(BindingFlags.Public).Where(t => t.IsAssignableTo(typeof(ISelfConfigAfterBuild))).FirstOrDefault();
            if (selfConfigSubClasses != null)
            {
                if (Activator.CreateInstance(selfConfigSubClasses) is ISelfConfigAfterBuild configurator)
                {
                    try
                    {
                        logger.Log(RetConComposer.LogLevel, "RetCon: starting post build config of {subclass}.", selfConfigSubClasses.AssemblyQualifiedName);
                        configurator.PostBuildConfig(app, set.Attribute, configuration, logger);
                        logger.Log(RetConComposer.LogLevel, "RetCon: finished post build config of {subclass}.", selfConfigSubClasses.AssemblyQualifiedName);
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, "RetCon: failed post build config of {subclass}. Message = '{message}'.", selfConfigSubClasses.AssemblyQualifiedName, ex.Message);
                    }
                }
            }
        }
    }
}
