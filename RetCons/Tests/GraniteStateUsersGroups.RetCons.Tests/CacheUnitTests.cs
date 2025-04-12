using GraniteStateUsersGroups.RetCons.Cache.NullCache;
using GraniteStateUsersGroups.RetCons.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.Runtime.CompilerServices;

namespace GraniteStateUsersGroups.RetCons.Tests;

internal class CacheUnitTests
{
    [Test]
    public void NullCache()
    {
        var app = GetConfiguredApplication();
        var cache = app.Services.GetService(typeof(IDistributedCache)) as IDistributedCache;
        Assert.That(cache, Is.Not.Null); 
        Assert.That(cache, Is.TypeOf(typeof(NullCache)));
    }

    public WebApplication GetConfiguredApplication([CallerMemberName] string? test = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Configuration.AddJsonFile($"appsettings.{test}.json", false);
        builder.AddRetConTargetServices(RetConDiscoveryLevel.AllowUnsignedAssemblies);
        var app = builder.Build();
        var settings = app.Configuration.GetValue<string>("settings");
        Assert.That(settings, Is.Not.Empty);
        Assert.That(settings, Is.EqualTo(test));
        return app;
    }
}
