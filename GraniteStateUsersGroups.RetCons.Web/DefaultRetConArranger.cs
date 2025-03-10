
namespace GraniteStateUsersGroups.RetCons.Web
{
    [RetCon(typeof(IRetConArranger))]
    public class DefaultRetConArranger : RetConArrangerBase
    {
        public DefaultRetConArranger([FromKeyedServices(RetConComposer.RetConServiceKey)] IHostApplicationBuilder builder, [FromKeyedServices(RetConComposer.RetConServiceKey)] ILogger logger) 
            : base(builder, logger)
        {
        }

        public override void ConfigureService(RetConAttribute attribute, Type targetType)
        {
            var descriptor = new ServiceDescriptor(attribute.For, attribute.ServiceKey, targetType, attribute.Lifetime);
            _services.Add(descriptor);
        }

        public override void InitializeService(RetConAttribute attribute, IServiceProvider serviceProvider)
        {
            
        }
    }
}
