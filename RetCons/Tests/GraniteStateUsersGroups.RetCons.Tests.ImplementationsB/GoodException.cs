
namespace GraniteStateUsersGroups.RetCons.Tests.ImplementationsB
{
    [Serializable]
    internal class GoodException : Exception
    {
        public GoodException()
        {
        }

        public GoodException(string? message) : base(message)
        {
        }

        public GoodException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}