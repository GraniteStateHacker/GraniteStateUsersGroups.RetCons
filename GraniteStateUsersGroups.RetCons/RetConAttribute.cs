using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;


namespace GraniteStateUsersGroups.RetCons;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class RetConAttribute : Attribute
{

    public Type For { get; init; }

    public ServiceLifetime Lifetime { get; init; } = ServiceLifetime.Transient;

    public object? ServiceKey { get; init; }

    public object? ArrangerKey { get; init; }

    public object[] ArrangerArguments { get; init; } = Array.Empty<object>();




    public RetConAttribute(Type @for, params object[] args)
    {
        For = @for;
        ArrangerArguments = args;
    }


    public static void GetRetConImplementations(IEnumerable<Assembly> searchAssemblies,
                                                Action<RetConAttribute, Type> callbackInterfaceForImplementation,
                                                ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(searchAssemblies);
        ArgumentNullException.ThrowIfNull(callbackInterfaceForImplementation);
        foreach (var anAssembly in searchAssemblies)
        {
            logger.Log(RetConComposer.LogLevel, "DependencyInjection: found qualified assembly {fullname}.", anAssembly.FullName);
            foreach (var aClass in anAssembly.GetTypes().Where(t => t.IsClass && t.IsPublic))
            {
                foreach (var anAttribute in aClass.GetCustomAttributes<RetConAttribute>(inherit: false))
                {
                    callbackInterfaceForImplementation(anAttribute, aClass);
                }
            }
        }
    }
}
