using GraniteStateUsersGroups.RetCons.Tests.Implementations;
using GraniteStateUsersGroups.RetCons.Tests.ImplementationsB;
using GraniteStateUsersGroups.RetCons.Tests.Implementationsß;
using GraniteStateUsersGroups.RetCons.Tests.Interfaces;
using GraniteStateUsersGroups.RetCons.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.Runtime.CompilerServices;

namespace GraniteStateUsersGroups.RetCons.Tests;

public class ConfigurationOnlyTests
{
    [SetUp]
    public void Setup()
    {

    }


    [Test]
    public void NullDefault()
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



            Assert.That(DefaultConfiguration.SideEffectsLog.Count, Is.EqualTo(2));
            Assert.That(DefaultConfiguration.SideEffectsLog[0], Is.EqualTo("Config.Configure method called for DefaultConfiguration."));
            Assert.That(DefaultConfiguration.SideEffectsLog[1], Is.EqualTo("Config.PostBuildConfig method called for DefaultConfiguration."));

            Assert.That(ConfigurationWhenConfigured.SideEffectsLog.Count, Is.EqualTo(0));
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
            Assert.That(DefaultConfiguration.SideEffectsLog.Count, Is.EqualTo(2));
            Assert.That(DefaultConfiguration.SideEffectsLog[0], Is.EqualTo("Config.Configure method called for DefaultConfiguration."));
            Assert.That(DefaultConfiguration.SideEffectsLog[1], Is.EqualTo("Config.PostBuildConfig method called for DefaultConfiguration."));

            Assert.That(ConfigurationWhenConfigured.SideEffectsLog.Count, Is.EqualTo(2));
            Assert.That(ConfigurationWhenConfigured.SideEffectsLog[0], Is.EqualTo("Config.Configure method called for ConfigurationWhenConfigured."));
            Assert.That(ConfigurationWhenConfigured.SideEffectsLog[1], Is.EqualTo("Config.PostBuildConfig method called for ConfigurationWhenConfigured."));
        });

    }

    public WebApplication GetConfiguredApplication([CallerMemberName] string? test = null)
    {
        DefaultConfiguration.SideEffectsLog.Clear();
        ConfigurationWhenConfigured.SideEffectsLog.Clear();
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

        app.UseRetConTargetServices();

        return app;
    }
}
