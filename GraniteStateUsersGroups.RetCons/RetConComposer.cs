
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Runtime.Loader;
using System.Security;

namespace GraniteStateUsersGroups.RetCons;

public static class RetConComposer
{
    public static readonly LogLevel LogLevel = GetDefaultLogLevel();

    private static LogLevel GetDefaultLogLevel() => LogLevel.Information;

    public const string RetConServiceKey = "RetConServiceKey";

    public static IHostApplicationBuilder RegisterRetConServices(this IHostApplicationBuilder builder, RetConDiscoveryLevel discoveryLevel)
    {
        ArgumentNullException.ThrowIfNull(builder);
        RetCon.Context.Builder = builder;
        RegisterBaseServices();
        InitializeRetCons(discoveryLevel);
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

    private static void InitializeRetCons(RetConDiscoveryLevel discoveryLevel)
    {
        try
        {
            var builder = RetCon.Context.Builder;
            LogRetConStarted(RetCon.Context.Logger!, null);
            DiscoverRetCons(discoveryLevel);
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

    private static void DiscoverRetCons(RetConDiscoveryLevel discoveryLevel)
    {
        var configuration = RetCon.Context.ConfigurationBuilder!.Build();
        var logger = RetCon.Context.Logger;
        var fileSearchPattern = configuration!["ImplementationAssemblySearch"] ?? "*.dll";
        var rootAssembly = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Entry assembly cannot be null.");
        var exeFolder = new DirectoryInfo(rootAssembly.Location)?.Parent ?? throw new InvalidOperationException("Parent directory cannot be null.");

        var fileSearchFolder = configuration["ImplementationAssemblyFolder"] ?? exeFolder.FullName;

        var qualifiedAssemblies = GetAssembliesThatReferenceType(typeof(RetCon.RetConBaseAttribute), logger!, fileSearchFolder, fileSearchPattern, discoveryLevel);
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

    private static readonly Action<ILogger, string, string, Exception?> LogScanningAssemblies =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(4, nameof(GetAssembliesThatReferenceType)),
            "RetCon: Scanning for assemblies matching \"{FullName}\\{FileSearchPattern}\"."
        );

    private static readonly Action<ILogger, string, string, Exception?> LogExaminingLibraries =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(5, nameof(GetAssembliesThatReferenceType)),
            "RetCon: Examining matched libraries for assemblies with references to {FullName}: {LibNames}."
        );

    private static readonly Action<ILogger, string, string, Exception?> LogSkippingAssembly =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(6, nameof(GetAssembliesThatReferenceType)),
            "RetCon: '{AssemblyName}' does not appear to be a compatible assembly. Skipping. {Message}"
        );

    private static IEnumerable<Assembly> GetAssembliesThatReferenceType(Type referencedType, ILogger logger, string searchFolder, string fileSearchPattern, RetConDiscoveryLevel discoveryLevel)
    {
        var keyAssembly = Assembly.GetAssembly(referencedType) ?? throw new ArgumentOutOfRangeException(nameof(referencedType), "referencedType must be ");
        var searchFolderDir = new DirectoryInfo(searchFolder) ?? throw new ArgumentOutOfRangeException(nameof(searchFolder), "searchFolder must be a valid directory path.");
        LogScanningAssemblies(logger, searchFolder, fileSearchPattern, null);

        var localLibFileNames = fileSearchPattern.Split(";")
            .SelectMany(pattern => searchFolderDir.GetFiles(pattern))
            .Distinct()
            .Where(name => !name.Name.StartsWith("system.", StringComparison.InvariantCultureIgnoreCase));

        LogExaminingLibraries(logger, keyAssembly.FullName!, string.Join(";", localLibFileNames.Select(lib => lib.Name)), null);
        var allAssembliesInBinFolder = localLibFileNames
            .Select(anAssembly =>
            {
                Assembly? result = null;
                try
                {
                    result = AssemblyLoadContext.Default.LoadFromAssemblyPath(anAssembly.FullName);
                    if (discoveryLevel == RetConDiscoveryLevel.RequireSignedAssemblies && !IsAssemblySigned(result))
                    {
                        throw new SecurityException("Assembly Not Signed or Invalid Signature.");
                    }
                }
                catch (Exception ex)
                {
                    LogSkippingAssembly(logger, anAssembly.FullName, ex.Message, null);
                }
                return result;
            });
        var qualifiedAssemblies = allAssembliesInBinFolder
            .Where(a => a?.GetReferencedAssemblies().Any(a => a.FullName == keyAssembly.FullName) ?? false)
            .Cast<Assembly>().Union([typeof(RetCon.RetConBaseAttribute).Assembly]).Distinct();
        return qualifiedAssemblies;
    }

    private static bool IsAssemblySigned(Assembly assembly)
    {
        var publicKey = assembly.GetName().GetPublicKey();
        var publicKeyToken = assembly.GetName().GetPublicKeyToken();
        if (publicKey?.Length > 0 && publicKeyToken?.Length > 0)
        {
            return true;
        }

        return false;
    }
}
