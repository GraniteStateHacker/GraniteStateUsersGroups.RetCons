
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GraniteStateUsersGroups.RetCons;

public class RegistrationContext: HashSet<RetConSet>
{
    public IHostApplicationBuilder? Builder { get; set; }

    public IServiceCollection? Services { get; set; }

    public IServiceProvider? ServiceProvider { get; set; } 
    
    public ILogger? Logger { get; set; }

    public IConfigurationBuilder? ConfigurationBuilder { get; set; }

    public HashSet<RetConSet> RegisteredRetCons { get; set; } = [];
}
