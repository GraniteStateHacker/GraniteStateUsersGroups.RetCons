using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;


namespace GraniteStateUsersGroups.RetCons;

public static partial class RetCon
{


    public static RegistrationContext Context { get; } = [];

    public abstract class RetConBaseAttribute(Type @for) : Attribute
    {
        public Type For { get; init; } = @for;
        public ServiceLifetime Lifetime { get; init; } = ServiceLifetime.Transient;
        public object? ServiceKey { get; init; }
        public uint Priority { get; init; }
        public object[]? Parameters { get; init; }
        public abstract void Register(IServiceCollection services, Type implementationType);
        public abstract bool ChooseThisImplementation();
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class DefaultAttribute(Type @for) : RetConBaseAttribute(@for)
    {
        public override bool ChooseThisImplementation()
            => !Context.Any(x => x.Interface == For && x.Attribute.ServiceKey == ServiceKey && x.Attribute.Priority > this.Priority && x.Attribute.ChooseThisImplementation());

        public override void Register(IServiceCollection services, Type implementationType)
        {
            if (ChooseThisImplementation())
            {
                var serviceDescriptor = new ServiceDescriptor(For, ServiceKey, implementationType, Lifetime);
                services.Add(serviceDescriptor);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class ForceAttribute : RetConBaseAttribute
    {
        public ForceAttribute(Type @for) : base(@for)
        {
            Priority = uint.MaxValue;
        }

        public override void Register(IServiceCollection services, Type implementationType)
        {
            var serviceDescriptor = new ServiceDescriptor(For, ServiceKey, implementationType, Lifetime);
            services.Add(serviceDescriptor);
        }

        public override bool ChooseThisImplementation() => true;
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class WhenConfiguredAttribute : RetConBaseAttribute
    {
        public string RequiredConfigurationKey { get; init; }

        public WhenConfiguredAttribute(Type @for, string requiredConfigurationKey) : base(@for)
        {
            RequiredConfigurationKey = requiredConfigurationKey;
            Priority = 1;
        }

        public override void Register(IServiceCollection services, Type implementationType)
        {
            var serviceDescriptor = new ServiceDescriptor(For, ServiceKey, implementationType, Lifetime);
            services.Add(serviceDescriptor);
        }

        public override bool ChooseThisImplementation()
        {
            var config = Context.ConfigurationBuilder?.Build();
            var section = config?.GetSection(RequiredConfigurationKey);
            return section != null && section.Exists();
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class ForEnvironmentAttribute : RetConBaseAttribute
    {
        public string EnvironmentName { get; init; }

        public ForEnvironmentAttribute(Type @for, string environmentName) : base(@for)
        {
            EnvironmentName = environmentName;
            Priority = 1;
        }

        public override void Register(IServiceCollection services, Type implementationType)
        {
            var serviceDescriptor = new ServiceDescriptor(For, ServiceKey, implementationType, Lifetime);
            services.Add(serviceDescriptor);
        }

        public override bool ChooseThisImplementation()
        {
            var currentEnvironmentName = Context.Builder?.Environment.EnvironmentName;
            return currentEnvironmentName == EnvironmentName;
        }
    }

    public static void GetRetConImplementations(this IEnumerable<Assembly> searchAssemblies,
                                                   Action<RetConBaseAttribute, Type> callbackInterfaceForImplementation)
    {
        ArgumentNullException.ThrowIfNull(searchAssemblies);
        ArgumentNullException.ThrowIfNull(callbackInterfaceForImplementation);
        var logger = Context.Logger!;
        foreach (var anAssembly in searchAssemblies)
        {
            _logFoundQualifiedAssembly(logger, anAssembly.FullName!, null);
            foreach (var aClass in anAssembly.GetTypes().Where(t => t.IsClass && t.IsPublic))
            {
                foreach (var anAttribute in aClass.GetCustomAttributes<RetConBaseAttribute>(inherit: false))
                {
                    callbackInterfaceForImplementation(anAttribute, aClass);
                }
            }
        }
    }

    private static readonly Action<ILogger, string, Exception?> _logFoundQualifiedAssembly =
        LoggerMessage.Define<string>(
            RetConComposer.LogLevel,
            new EventId(0, nameof(RetCon)),
            "RetCon: found qualified assembly '{FullName}'.");
}

