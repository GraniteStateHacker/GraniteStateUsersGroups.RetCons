namespace GraniteStateUsersGroups.RetCons.Tests.Interfaces;

public interface IClass1
{
    string Name => GetType().Name;
}

public interface IClass2
{
    string Name2 => GetType().Name;
}