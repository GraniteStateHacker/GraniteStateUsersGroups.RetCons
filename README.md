# RetCons: Retroactive Continuity for .NET

The RetCons library is a lightweight but powerful .NET utility designed to simplify and enhance dependency injection by allowing developers to retroactively define and manage service implementations. Inspired by the concept of retroactive continuity in storytelling, RetCons enables dynamic and flexible service registration using attributes. It promotes SOLID Principles and Clean Architecture to the extent of being able to completely decouple dependencies right out of your build pipeline and compose them at runtime instead.


## Features

- **Attribute-Based Service Registration**: Use attributes to define and manage service implementations for interfaces.
- **Priority-Based Selection**: Control which implementation is chosen based on priority and conditions.
- **Environment-Specific Implementations**: Register services tailored to specific environments.
- **Configuration-Driven Implementations**: Dynamically register services based on configuration settings.
- **Extensible and Modular**: Easily extend the library to meet your specific needs.
- **Kubernetes-Friendly Composition**: Complement Kubernetes-based platforms with "module-lithic" services as an alternative to microservices.
- **Automatic Discovery of Implementations**: Can discover implementations in external assemblies without static references.
- **Compose Dependency Graph**: Flexibly compose your app's dependency graph at app start up. Reconfigure and restart to re-compose it.
- **Extensible Discovery Strategy Mechanism**: Customize the discovery and registration process using strategies. 

## Why RetCon?

RetCon empowers developers to:

- **Leverage SOLID Principles**: Decouple dependencies from the core application, enabling better maintainability and scalability.
- **Compose Applications Dynamically**: Use .NET reflection to discover and configure new implementations at startup without modifying the core application.
- **Simplify Modular Design**: Build modular applications that can dynamically adapt to new requirements or configurations.

This makes RetCon an excellent choice for applications that require flexibility, modularity, and dynamic composition. Use of Aspire.NET, Kubernetes, and RetCons all complement each other well without interfering with each other.

## Getting Started

### Installation

Add the RetCons.Web library to your ASP.NET project by referencing the `GraniteStateUsersGroups.RetCons.Web` package. (Support for other .NET-based platforms such as WinUI and MAUI coming soon)

### Usage

1. **Define Your Interface and Implementation**:
   ```csharp
   public interface IExampleService
   {
       void Execute();
   }

   [RetCon.Default(typeof(IExampleService))]
   public class DefaultExampleService : IExampleService
   {
       public void Execute() => Console.WriteLine("Default Implementation");
   }
   ```
2. **Register and Initialize Services**:

   Use the following methods in your `Program.cs` file to discover and register service implementations and initialize activated services:

   ```csharp
   var builder = WebApplication.CreateBuilder(args);

   // Discover and register services
   builder.AddRetConTargetServices(RetConDiscoveryLevel.RequireSignedAssemblies);

   var app = builder.Build();

   // Initialize activated services
   app.UseRetConTargetServices();

   app.Run();
   ```
- `AddRetConTargetServices`: Discovers and registers service implementations dynamically based on the RetCon attributes.
   - `UseRetConTargetServices`: Initializes the activated services after the application has been built.

3. **Use `ISelfConfig` and `ISelfConfigAfterBuild` Interfaces**:

   RetCon calls methods in these interfaces on subclasses of the actively selected implementation classes. Here’s how to use them:

  
   - **Configuration Subclass Contained Within the Service Class**:
These examples illustrate how to use `ISelfConfig` and `ISelfConfigAfterBuild` to inject custom logic during the application’s configuration and post-build phases.

     ```csharp
     [RetCon.Default(typeof(IMyServiceInterface)]
     public class MyService : IMyServiceInterface
     {
         public class MyServiceConfig : ISelfConfig, ISelfConfigAfterBuild
         {
             public void Configure(WebApplicationBuilder builder, RetCon.RetConBaseAttribute attribute, IConfiguration configuration, ILogger logger)
             {
                 logger.LogInformation("Configuring MyService with attribute {Attribute}", attribute);
             }

             public void PostBuildConfig(IApplicationBuilder app, RetCon.RetConBaseAttribute attribute, IConfiguration? configuration, ILogger logger)
             {
                 logger.LogInformation("Post-build configuration for MyService with attribute {Attribute}", attribute);
             }
         }
     }
     ```

   These examples illustrate how to use `ISelfConfig` and `ISelfConfigAfterBuild` to inject custom logic during the application’s configuration and post-build phases.  Note, also, that a RetCon with a `null` Interface (`For`) property can be used to execute configuration without actually registering any implementation if desired..

RetCon provides an extensible strategy mechanism to customize the discovery and registration process. You can implement your own strategy by implementing delegate `RetConDiscoveryStrategy` and registering it:
### Example Projects

This repository includes sample projects demonstrating how to use RetCon attributes in real-world scenarios. Explore the samples to see how RetCon can simplify your dependency injection setup.

## Documentation

Comprehensive documentation is planned but not yet available. A wiki will be created to host detailed guides, API references, and advanced usage examples.

## To-Do

The following tasks are still pending:

- Comprehensive documentation needs to be written. [#9](https://github.com/GraniteStateHacker/GraniteStateUsersGroups.RetCons/issues/9)
- A continuous integration pipeline needs to be built. [#6](https://github.com/GraniteStateHacker/GraniteStateUsersGroups.RetCons/issues/6)
- A contribution guide needs to be written. [8](https://github.com/GraniteStateHacker/GraniteStateUsersGroups.RetCons/issues/8)
- A publishing pipeline needs to be built to publish releases to nuget.org. (Depends on [#6](https://github.com/GraniteStateHacker/GraniteStateUsersGroups.RetCons/issues/6)) [#10](https://github.com/GraniteStateHacker/GraniteStateUsersGroups.RetCons/issues/10)
- Consider targets for other platforms (WinUI, MAUI, Azure Functions, et al) [#11](https://github.com/GraniteStateHacker/GraniteStateUsersGroups.RetCons/issues/11)

## Contributing

We welcome contributions from the community! To contribute:

1. Fork the repository.
2. Create a feature branch.
3. Commit your changes.
4. Submit a pull request.

Please review our contribution guidelines (to be written) before submitting.

## License

This project is licensed under the MIT License. See the [LICENSE](https://raw.githubusercontent.com/GraniteStateHacker/GraniteStateUsersGroups.RetCons/refs/heads/main/LICENSE.txt) file for details.

## Acknowledgments

RetCon is developed and maintained by Granite State Users Groups, LLC. Special thanks to all contributors and the open-source community for their support.

RetCons was created and is maintained as an OSS project by [Jim Wilcox, Modern Application Architect](https://www.linkedin.com/in/jimwilcox2/) and [Microsoft MVP in Developer Tools](https://mvp.microsoft.com/en-US/MVP/profile/70a2a0ca-6250-e911-a995-000d3a1362e3). 

Additional contributors are invited, and contributions will be welcomed for evaluation and appreciated in acceptance.


---

Start redefining your dependency injection story with RetCon today!

## Change Log

1.0.0 - Initial Release

1.0.1 - Implement assembly discovery strategy that demands signed assemblies.

1.0.2 - Resolves a bug where the wrong implementation could be registered.

1.0.3 - Nuspec fix (non-functional, "cosmetic" change only)

1.0.4 - Allow `null` type for `For` property in RetCon attributes. This allows for configuration without registering an implementation. 
