
namespace GraniteStateUsersGroups.RetCons;

public interface IRetConArranger
{
    public bool CanRegister(RetConAttribute attribute);
    public void ConfigureService(RetConAttribute attribute, Type targetImplementation);
    public void InitializeService(RetConAttribute attribute, IServiceProvider serviceProvider);
    
}
