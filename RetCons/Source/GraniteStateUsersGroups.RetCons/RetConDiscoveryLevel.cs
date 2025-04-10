using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Runtime.Loader;

namespace GraniteStateUsersGroups.RetCons;

public delegate Assembly[] AssemblyDiscoveryStrategy();

public static class RetConDiscoveryLevel
{
    private static string qualifyingAssembly { get; } = typeof(RetCon).Assembly.FullName!;


    public static readonly AssemblyDiscoveryStrategy AllowUnsignedAssemblies = () =>
    {
        var configuration = RetCon.Context.ConfigurationBuilder!.Build();
        var fileSearchPath = configuration["ImplementationAssemblySearch"] ?? "*.dll";
        var localLibFileNames = Directory.GetFiles(AppContext.BaseDirectory, fileSearchPath)
            .ToList();

        var logger = RetCon.Context.Logger!;

        LogScanningForAssemblies!(logger, AppContext.BaseDirectory, fileSearchPath, null);
        LogExaminingMatchedLibraries!(logger, qualifyingAssembly, string.Join(";", localLibFileNames.Select(lib => lib)), null);

        var allAssembliesInBinFolder = localLibFileNames
        .Select(AssembliesOnly(logger));
        var retConAssemblyName = typeof(RetCon).Assembly.GetName().FullName;
        var qualifiedAssemblies = allAssembliesInBinFolder
            .Where(a => a?.GetReferencedAssemblies().Any(r => r.FullName == retConAssemblyName) == true)
            .Distinct()
            .ToArray();
        return qualifiedAssemblies.ToArray()!;

    };

    private static Func<string, Assembly?> AssembliesOnly(ILogger logger)
    {
        return anAssembly =>
        {
            Assembly? result = null;
            try
            {
                result = AssemblyLoadContext.Default.LoadFromAssemblyPath(anAssembly);
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    LogSkippingIncompatibleAssembly!(logger!, anAssembly, ex.Message, null);
                }
            }
            return result;
        };
    }

    public static readonly AssemblyDiscoveryStrategy RequireSignedAssemblies = () => {
        var allAssemblies = AllowUnsignedAssemblies();
        var signedAssemblies = allAssemblies
            .Where(a => (a.GetName().GetPublicKey()?.Length ?? 0) > 0)
            .ToArray();
        return signedAssemblies;
    };

    private static readonly Action<ILogger, string, string, Exception?> LogScanningForAssemblies 
        = LoggerMessage.Define<string, string>(
        LogLevel.Information,
        new EventId(0, nameof(LogScanningForAssemblies)),
        "RetCon: Scanning for assemblies in {Path} with pattern {Pattern}."
    );

    private static readonly Action<ILogger, string, string, Exception?> LogExaminingMatchedLibraries 
        = LoggerMessage.Define<string, string>(
        LogLevel.Information,
        new EventId(1, nameof(LogExaminingMatchedLibraries)),
        "RetCon: Examining matched libraries for assemblies with references to {FullName}: {LibNames}."
    );

    private static readonly Action<ILogger, string, string, Exception?> LogSkippingIncompatibleAssembly 
        = LoggerMessage.Define<string, string>(
        LogLevel.Warning,
        new EventId(2, nameof(LogSkippingIncompatibleAssembly)),
        "RetCon: '{AssemblyName}' does not appear to be a compatible assembly. Skipping. {Message}"
    );


}
