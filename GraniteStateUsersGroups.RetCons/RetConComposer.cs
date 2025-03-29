
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

    private static void InitializeRetCons( RetConDiscoveryLevel discoveryLevel)
    {
        try
        {
            var builder = RetCon.Context.Builder;
            RetCon.Context.Logger!.Log(_LogLevel, "RetCon: {libname} started.", nameof(InitializeRetCons));
            DiscoverRetCons(discoveryLevel);
            RegisterSelectedRetCons();
        }
        catch (Exception ex)
        {
            RetCon.Context.Logger!.Log(LogLevel.Error, ex, "RetCon: {libname} failed with exceptions (see details).", nameof(InitializeRetCons));
        }
        RetCon.Context.Logger!.Log(_LogLevel, "RetCon: {libname} complete.", nameof(InitializeRetCons));
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
        var rootAssembly = Assembly.GetEntryAssembly() ?? throw new NullReferenceException();
        var exeFolder = new DirectoryInfo(rootAssembly.Location)?.Parent ?? throw new NullReferenceException();

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
            logger.Log(_LogLevel, "RetCon: {retconName} added ({implementationName}) as implementation for interface '{interfaceName}' with lifetime '{serviceLifetime}').", attribute.GetType().Name, implementationName, interfaceName, serviceLifetime);
        }
    }

    private static IEnumerable<Assembly> GetAssembliesThatReferenceType(Type referencedType, ILogger logger, string searchFolder, string fileSearchPattern, RetConDiscoveryLevel discoveryLevel)
    {
        var keyAssembly = Assembly.GetAssembly(referencedType) ?? throw new ArgumentOutOfRangeException(nameof(referencedType), "referencedType must be ");
        var searchFolderDir = //get Directory from string
            new DirectoryInfo(searchFolder) ?? throw new ArgumentOutOfRangeException(nameof(searchFolder), "searchFolder must be a valid directory path.");
        logger.Log(_LogLevel, "RetCon: Scanning for assemblies matching \"{fullName}\\{fileSearchPattern}\".", searchFolder, fileSearchPattern);

        var localLibFileNames = fileSearchPattern.Split(";")
            .SelectMany(pattern => searchFolderDir.GetFiles(pattern))
            .Distinct()
            .Where(name =>   //trim out files that will never be valid
                //!name.Name.ToLower().StartsWith("microsoft.") &&
                !name.Name.StartsWith("system.", comparisonType: StringComparison.InvariantCultureIgnoreCase));

        logger.Log(_LogLevel, "RetCon: Examining matched libraries for assemblies with references to {FullName}: {libNames}.", keyAssembly.FullName, string.Join(";", localLibFileNames.Select(lib => lib.Name)));
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
                    logger.Log(_LogLevel, "RetCon: '{assemblyName}' does not appear to be a compatible assembly. Skipping. {message}", anAssembly.FullName, ex.Message);
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
