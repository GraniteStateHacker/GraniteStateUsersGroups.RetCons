namespace GraniteStateUsersGroups.RetCons.Web;
public static class ConfigurationExtensions
{
       

    public static WebApplicationBuilder AddRetConTargetServices(this WebApplicationBuilder builder, RetConDiscoveryLevel level)
    {
        ArgumentNullException.ThrowIfNull(builder);
        RetConComposer.RegisterRetConServices(builder, level);
        return builder;
    }

    public static WebApplication UseRetConTargetServices(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        RetConComposer.UseRetConTargetServices(app);
        return app;
    }

}
