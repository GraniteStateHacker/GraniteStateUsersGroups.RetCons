using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GraniteStateUsersGroups.RetCons;

public static class RetConComposer
{
    public static readonly LogLevel LogLevel = GetDefaultLogLevel();

    private static LogLevel GetDefaultLogLevel() => LogLevel.Information;

    public const string RetConServiceKey = "RetConServiceKey";

    public static IHostApplicationBuilder RegisterRetConServices(this IHostApplicationBuilder builder, AssemblyDiscoveryStrategy discoveryStrategy)
    {
        ArgumentNullException.ThrowIfNull(builder);
        RetCon.Context.Builder = builder;
        RegisterBaseServices();
        InitializeRetCons(discoveryStrategy);
        return builder;
    }

    private static void RegisterBaseServices()
    {
        var builder = RetCon.Context.Builder;
        RetCon.Context.Services = builder!.Services!;
        RetCon.Context.ConfigurationBuilder = builder.Configuration;
        RetCon.Context.Logger = LoggerFactory.Create(b =>
            {
                b.AddDebug(); b.SetMinimumLevel(LogLevel.Trace);
            })
            .CreateLogger(typeof(RetCon.RetConBaseAttribute));
    }

    private static void InitializeRetCons(AssemblyDiscoveryStrategy discoveryStrategy)
    {
        try
        {
            var builder = RetCon.Context.Builder;
            LogRetConStarted(RetCon.Context.Logger!, null);
            DiscoverRetCons(discoveryStrategy);
            RegisterSelectedRetCons();
        }
        catch (Exception ex)
        {
            LogRetConFailed(RetCon.Context.Logger!, ex);
        }
        LogRetConComplete(RetCon.Context.Logger!, null);
    }

    private static void RegisterSelectedRetCons()
    {
        foreach (var set in RetCon.Context)
        {
            RegisterRetCon(set);
        }
    }

    private static void DiscoverRetCons(AssemblyDiscoveryStrategy discoveryStrategy)
    {
        var qualifiedAssemblies = discoveryStrategy();
        qualifiedAssemblies.GetRetConImplementations(AccumulateImplementations);
    }

    private static void AccumulateImplementations(RetCon.RetConBaseAttribute theAttribute, Type theTargetImplementation)
    {
        var targetInterface = theAttribute.For;

        var keyPair = new RetConSet(targetInterface, theAttribute, theTargetImplementation);

        RetCon.Context.Add(keyPair);
    }

    private static void RegisterRetCon(RetConSet set)
    {
        var logger = RetCon.Context.Logger!;
        var container = RetCon.Context.Services!;
        var attribute = set.Attribute;
        var targetImplementation = set.TargetImplementation;

        var serviceLifetime = attribute.Lifetime;

        var implementationName = targetImplementation.AssemblyQualifiedName;
        var interfaceName = set.Interface.AssemblyQualifiedName;
        if (attribute.ChooseThisImplementation())
        {
            attribute.Register(container, targetImplementation);
            RetCon.Context.RegisteredRetCons.Add(new RetConSet(set.Interface, attribute, targetImplementation));
            LogRetConAdded(logger, attribute.GetType().Name, implementationName, interfaceName, serviceLifetime, null);
        }
    }



    private static readonly Action<ILogger, string, string?, string?, ServiceLifetime, Exception?> LogRetConAdded =
    LoggerMessage.Define<string, string?, string?, ServiceLifetime>(
        LogLevel.Information,
        new EventId(0, nameof(RegisterRetCon)),
        "RetCon: {RetconName} added ({ImplementationName}) as implementation for interface '{InterfaceName}' with lifetime '{ServiceLifetime}')."
    );

    private static readonly Action<ILogger, Exception?> LogRetConStarted =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1, nameof(InitializeRetCons)),
            "RetCon: InitializeRetCons started."
        );

    private static readonly Action<ILogger, Exception?> LogRetConFailed =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(2, nameof(InitializeRetCons)),
            "RetCon: InitializeRetCons failed with exceptions (see details)."
        );

    private static readonly Action<ILogger, Exception?> LogRetConComplete =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(3, nameof(InitializeRetCons)),
            "RetCon: InitializeRetCons complete."
        );


}
