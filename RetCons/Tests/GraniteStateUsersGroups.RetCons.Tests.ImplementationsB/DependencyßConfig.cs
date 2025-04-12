using Microsoft.Extensions.Options;

namespace GraniteStateUsersGroups.RetCons.Tests.Implementationsß;

public class DependencyßConfig : IOptions<DependencyßConfig>
{
    public string? Value1 { get; set; }

    DependencyßConfig IOptions<DependencyßConfig>.Value => this;
}
