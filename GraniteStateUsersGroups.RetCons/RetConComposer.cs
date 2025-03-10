
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

    internal static readonly LogLevel _LogLevel = LogLevel.Debug;
    private static HashSet<RetConSet>? allRetCons = [];
    
    private static IServiceCollection? services;
    private static IConfigurationManager? configuration;
    private static ILogger? logger;

    public const string RetConServiceKey = "RetConServiceKey";

    public static IHostApplicationBuilder RegisterRetConServices(this IHostApplicationBuilder builder, RetConDiscoveryLevel discoveryLevel)
    {
        ArgumentNullException.ThrowIfNull(builder);
        RegisterBaseServices(builder);
        InitializeRetCons(builder, discoveryLevel);
        return builder;
    }



    private static void RegisterBaseServices(IHostApplicationBuilder builder)
    {
        services = builder.Services;
        configuration = builder.Configuration;
        logger = LoggerFactory.Create(b => { b.AddDebug(); b.SetMinimumLevel(LogLevel.Trace); }).CreateLogger(typeof(RetConComposer));
        services.AddKeyedSingleton(RetConServiceKey, logger);
        services.AddKeyedSingleton(RetConServiceKey, configuration);
        services.AddKeyedSingleton(RetConServiceKey, services);
        services.AddKeyedSingleton(RetConServiceKey, builder);
    }

    private static void InitializeRetCons(IHostApplicationBuilder builder, RetConDiscoveryLevel discoveryLevel)
    {
        try
        {
            logger!.Log(_LogLevel, "RetCon: {libname} started.", nameof(InitializeRetCons));
            DiscoverSelectedRetCons(discoveryLevel);
            RegisterArrangers(builder);
            services!.AddRetConsToContainer(logger!, allRetCons);
            builder.SelfConfigRetCons(configuration!, logger!, allRetCons);
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, ex, "RetCon: {libname} failed with exceptions (see details).", nameof(InitializeRetCons));
        }
        logger.Log(_LogLevel, "RetCon: {libname} complete.", nameof(InitializeRetCons));
    }

    private static void RegisterArrangers(IHostApplicationBuilder builder)
    {
        var arrangerType = typeof(IRetConArranger);
        var arrangerImplementations = allRetCons!.Where(kvp => kvp.Interface.IsAssignableTo(arrangerType));
        
        foreach (var set in arrangerImplementations)
        {
            var attribute = set.Attribute;
            var implementation = set.TargetImplementation;
            var serviceLifetime = ServiceLifetime.Singleton;
            var serviceKey = attribute.ServiceKey;
            services!.Add(new ServiceDescriptor(arrangerType, serviceKey, implementation, serviceLifetime));
            logger!.Log(_LogLevel, "RetCons: added ({implementationName}) as implementation for interface '{interfaceName}' with service key '{serviceKey}' with lifetime '{serviceLifetime}'.", implementation.AssemblyQualifiedName, arrangerType.AssemblyQualifiedName, serviceKey, serviceLifetime);
        }
        
    }

    private static void SelfConfigRetCons(this IHostApplicationBuilder builder, IConfiguration configuration, ILogger logger, HashSet<RetConSet> selectedImplementations)
    {
        foreach (var set in selectedImplementations)
        {
            AttemptSelfConfig(set, builder, configuration, logger); 
        }
    }


    private static void AddRetConsToContainer(this IServiceCollection container, ILogger logger, HashSet<RetConSet> selectedImplementations)
    {
        foreach (var set in selectedImplementations)
        {

            AddToContainer(set, logger, container);
        }
    }

    private static void DiscoverSelectedRetCons(RetConDiscoveryLevel discoveryLevel)
    {
        var fileSearchPattern = configuration!["ImplementationAssemblySearch"] ?? "*.dll";
        var rootAssembly = Assembly.GetEntryAssembly() ?? throw new NullReferenceException();
        var exeFolder = new DirectoryInfo(rootAssembly.Location)?.Parent ?? throw new NullReferenceException();

        var fileSearchFolder = configuration["ImplementationAssemblyFolder"] ?? exeFolder.FullName;

        var qualifiedAssemblies = GetAssembliesThatReferenceType(typeof(RetConAttribute), logger!, fileSearchFolder, fileSearchPattern, discoveryLevel);

        var implementationAccumulator = (RetConAttribute theAttribute, Type theTargetImplementation) =>
            AccumulateImplementations(theAttribute, theTargetImplementation);

        RetConAttribute.GetRetConImplementations(qualifiedAssemblies, implementationAccumulator, logger!);

    }

    private static void AccumulateImplementations(RetConAttribute theAttribute, Type theTargetImplementation)
    {
        var targetInterface = theAttribute.For;
        var serviceKey = theAttribute.ServiceKey;

        var keyPair = new RetConSet(targetInterface, serviceKey, theAttribute, theTargetImplementation);
        
        allRetCons!.Add(keyPair);
    }


    private static void AddToContainer(RetConSet set, ILogger logger, IServiceCollection container)
    {
        var attribute = set.Attribute;
        var targetImplementation = set.TargetImplementation;

        var serviceLifetime = attribute.Lifetime;
        var arrangerKey = attribute.ArrangerKey;
        
        var serviceProvider = container.BuildServiceProvider(); 
        
        var arranger = (arrangerKey== null) 
            ? serviceProvider.GetService<IRetConArranger>()
            : serviceProvider.GetKeyedService<IRetConArranger>(arrangerKey);
        
        if(arranger == null)
        {
            throw new InvalidOperationException($"No Arranger resolved for key '{arrangerKey}'");
        }

        var implementationName = targetImplementation.AssemblyQualifiedName;
        var interfaceName = set.Interface.AssemblyQualifiedName;
        if (arranger.CanRegister(attribute))
        {
            arranger.ConfigureService(attribute, targetImplementation);
            logger.Log(_LogLevel, "DependencyInjection: added ({implementationName}) as implementation for interface '{interfaceName}' with lifetime '{serviceLifetime}' (Configuration Key = '{configKey}').", implementationName, interfaceName, serviceLifetime, arrangerKey);
        }
    }



    private static void AttemptSelfConfig(RetConSet set, IHostApplicationBuilder builder, IConfiguration configuration, ILogger logger)
    {

        var serviceProvider = builder.Services.BuildServiceProvider();
        var arrangerKey = set.Attribute.ArrangerKey;
        var arranger = (arrangerKey == null)
            ? serviceProvider.GetService<IRetConArranger>()
            : serviceProvider.GetKeyedService<IRetConArranger>(arrangerKey);
        var arrangerType = arranger!.GetType();

        try
        {
            

            logger.Log(_LogLevel, "DependencyInjection: starting configuration of {arranger}.", arrangerType.AssemblyQualifiedName);
            if(arranger.CanRegister(set.Attribute))
            {
                arranger.ConfigureService(set.Attribute, set.TargetImplementation);
            }
            logger.Log(_LogLevel, "DependencyInjection: finished configuration of {arranger}.", arrangerType.AssemblyQualifiedName);
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, "DependencyInjection: failed configuration of {arranger}. Message = '{message}'.", arrangerType.AssemblyQualifiedName, ex.Message);
        }
    }

    private static IEnumerable<Assembly> GetAssembliesThatReferenceType(Type referencedType, ILogger logger, string searchFolder, string fileSearchPattern, RetConDiscoveryLevel discoveryLevel)
    {
        var keyAssembly = Assembly.GetAssembly(referencedType) ?? throw new ArgumentOutOfRangeException(nameof(referencedType), "referencedType must be ");
        var searchFolderDir = //get Directory from string
            new DirectoryInfo(searchFolder) ?? throw new ArgumentOutOfRangeException(nameof(searchFolder), "searchFolder must be a valid directory path.");
        logger.Log(_LogLevel, "DependencyInjection: Scanning for assemblies matching \"{fullName}\\{fileSearchPattern}\".", searchFolder, fileSearchPattern);

        var localLibFileNames = fileSearchPattern.Split(";")
            .SelectMany(pattern => searchFolderDir.GetFiles(pattern))
            .Distinct()
            .Where(name =>   //trim out files that will never be valid
                //!name.Name.ToLower().StartsWith("microsoft.") &&
                !name.Name.ToLower().StartsWith("system."));

        logger.Log(_LogLevel, "DependencyInjection: Examining matched libraries for assemblies with references to {FullName}: {libNames}.", keyAssembly.FullName, string.Join(";", localLibFileNames.Select(lib => lib.Name)));
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
                    logger.Log(_LogLevel, "DependencyInjection: '{assemblyName}' does not appear to be a compatible assembly. Skipping. {message}", anAssembly.FullName, ex.Message);
                }
                return result;
            });
        var qualifiedAssemblies = allAssembliesInBinFolder
            .Where(a => a?.GetReferencedAssemblies().Any(a => a.FullName == keyAssembly.FullName) ?? false)
            .Cast<Assembly>().Union(new List<Assembly> { typeof(RetConAttribute).Assembly }).Distinct();
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

    public static void UseRetConTargetServices(IHost app)
    {
        foreach(var aSet in allRetCons!)
        {
            var serviceProvider = app.Services;
            var arrangerKey = aSet.Attribute.ArrangerKey;
            var arranger = (arrangerKey == null)
                ? serviceProvider.GetService<IRetConArranger>()
                : serviceProvider.GetKeyedService<IRetConArranger>(arrangerKey);
            var arrangerType = arranger!.GetType();
            try
            {
                logger.Log(_LogLevel, "DependencyInjection: starting configuration of {arranger}.", arrangerType.AssemblyQualifiedName);
                if (arranger.CanRegister(aSet.Attribute))
                {
                    arranger.InitializeService(aSet.Attribute, serviceProvider);
                }
                logger.Log(_LogLevel, "DependencyInjection: finished configuration of {arranger}.", arrangerType.AssemblyQualifiedName);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "DependencyInjection: failed configuration of {arranger}. Message = '{message}'.", arrangerType.AssemblyQualifiedName, ex.Message);
            }
        }
    }
}
