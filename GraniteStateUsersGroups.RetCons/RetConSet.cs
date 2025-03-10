namespace GraniteStateUsersGroups.RetCons;

public record RetConSet(Type Interface, object? ServiceKey, RetConAttribute Attribute, Type TargetImplementation)
{
}
