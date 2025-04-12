using GraniteStateUsersGroups.RetCons.Tests.Interfaces;

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


