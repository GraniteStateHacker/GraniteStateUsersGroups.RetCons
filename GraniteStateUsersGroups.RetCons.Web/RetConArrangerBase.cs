using Microsoft.Extensions.DependencyInjection;

namespace GraniteStateUsersGroups.RetCons.Web;

public abstract class RetConArrangerBase : IRetConArranger
{
    protected readonly WebApplicationBuilder _builder;
    protected readonly IConfiguration _config;
    protected readonly ILogger _logger;
    protected readonly IServiceCollection _services;    


    public RetConArrangerBase([FromKeyedServices(RetConComposer.RetConServiceKey)] IHostApplicationBuilder builder, [FromKeyedServices(RetConComposer.RetConServiceKey)]  ILogger logger)
    {
        _builder = (WebApplicationBuilder)builder;
        _config = builder.Configuration;
        _logger = logger;
        _services = builder.Services;
    }

    public virtual bool CanRegister(RetConAttribute attribute) => true;
    public abstract void ConfigureService(RetConAttribute attribute, Type targetType);
    public abstract void InitializeService(RetConAttribute attribute, IServiceProvider serviceProvider);
}