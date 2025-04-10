namespace GraniteStateUsersGroups.RetCons.Web;

public interface ISelfConfig
{
    public void Configure(WebApplicationBuilder builder, RetCon.RetConBaseAttribute attribute, IConfiguration configuration, ILogger logger);

   
}
