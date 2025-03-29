namespace GraniteStateUsersGroups.RetCons.Web;

public interface ISelfConfigAfterBuild
{
    public void PostBuildConfig(IApplicationBuilder app, RetCon.RetConBaseAttribute attribute, IConfiguration? configuration, ILogger logger);

}