using GraniteStateUsersGroups.RetCons.Tests.Implementations;
using GraniteStateUsersGroups.RetCons.Tests.Implementationsß;
using GraniteStateUsersGroups.RetCons.Tests.Interfaces;
using GraniteStateUsersGroups.RetCons.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.Runtime.CompilerServices;

namespace GraniteStateUsersGroups.RetCons.Tests;

public class RetConUnitTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void PreTest()
    {
        var app = GetConfiguredApplication();
        Assert.Multiple(() =>
        {
            Assert.That(app, Is.Not.Null);
        });
    }

    [Test]
    public void DefaultImplementation()
    {
        var app = GetConfiguredApplication();
        var iClass1Implementation = app.Services.GetService(typeof(IClass1));
        var iClass2Implementation = app.Services.GetService(typeof(IClass2));

        Assert.Multiple(() =>
        {
            Assert.That(iClass1Implementation, Is.Not.Null);
            Assert.That(iClass1Implementation, Is.TypeOf<DualInterfaceImplementation>());

            Assert.That(iClass2Implementation, Is.Not.Null);
            Assert.That(iClass2Implementation, Is.TypeOf<DualInterfaceßImplementation>());
        });
    }

    [Test]
    public void DependencyA()
    {
        var app = GetConfiguredApplication();
        var iClass1Implementation = app.Services.GetService(typeof(IClass1)) as IClass1;

        Assert.Multiple(() =>
        {
            Assert.That(iClass1Implementation, Is.Not.Null);
            Assert.That(iClass1Implementation, Is.TypeOf<DependencyAlphaImplementation>());
            Assert.That(iClass1Implementation?.Name, Is.EqualTo(nameof(DependencyAlphaImplementation)));

            var iClass2Implementation = app.Services.GetService(typeof(IClass2));
            Assert.That(iClass2Implementation, Is.Not.Null);
            Assert.That(iClass2Implementation, Is.TypeOf<DualInterfaceßImplementation>());
        });
    }

    [Test]
    public void Dependencyß()
    {
        var app = GetConfiguredApplication();
        var iClass1Implementation = app.Services.GetService(typeof(IClass1)) as IClass1;
        var config = (iClass1Implementation as DualInterfaceßImplementation)?.GetConfig();
        var iClass2Implementation = app.Services.GetService(typeof(IClass2));

        Assert.Multiple(() =>
        {
            Assert.That(iClass1Implementation, Is.Not.Null);
            Assert.That(iClass1Implementation, Is.TypeOf<DualInterfaceßImplementation>());
            Assert.That(iClass1Implementation?.Name, Is.EqualTo(nameof(DualInterfaceßImplementation)));

            Assert.That(config, Is.Not.Null);
            Assert.That(config?.Value1, Is.EqualTo("😎"));

            Assert.That(iClass2Implementation, Is.Not.Null);
            Assert.That(iClass2Implementation, Is.TypeOf<DualInterfaceImplementation>());
        });
    }

    public WebApplication GetConfiguredApplication([CallerMemberName] string? test = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Configuration.AddJsonFile($"appsettings.{test}.json", false);
        builder.AddRetConTargetServices(RetConDiscoveryLevel.AllowUnsignedAssemblies);
        var app = builder.Build();
        var settings = app.Configuration.GetValue<string>("settings");

        Assert.Multiple(() =>
        {
            Assert.That(settings, Is.Not.Empty);
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings, Is.EqualTo(test));
        });

        return app;
    }
}
