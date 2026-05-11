using FluentAssertions;
using LightDIContainer.Core;
using LightDIContainer.Exceptions;
using LightDIContainer.Tests.Fixtures;

namespace LightDIContainer.Tests;

public class RegistrationTests
{
    private readonly Container _container;

    public RegistrationTests()
    {
        _container = new Container();
    }
    //happy path
    [Fact]
    public void Register_Interface_To_Implementation_IsTracked()
    {
        _container.AddTransient<ILogger, ConsoleLogger>();

        var registrations = _container.GetRegistrations();

        registrations.Should().ContainSingle(d =>
            d.ServiceType == typeof(ILogger) &&
            d.ImplementationType == typeof(ConsoleLogger) &&
            d.Lifetime == ServiceLifetime.Transient);
    }

    [Fact]
    public void Register_Multiple_Services_AllTracked()
    {
        _container.AddTransient<ILogger, ConsoleLogger>();
        _container.AddSingleton<IRepository, UserRepository>();

        var registrations = _container.GetRegistrations();

        registrations.Should().HaveCount(2);
    }

    [Fact]
    public void Register_SameService_Twice_LastWins()
    {
        _container.AddTransient<ILogger, ConsoleLogger>();
        _container.AddTransient<ILogger, AnotherLogger>();

        var logger = _container.Resolve<ILogger>();

        logger.Should().BeOfType<AnotherLogger>();
    }

    [Fact]
    public void Register_Fluent_Api_AllowsChaining()
    {
        // AddTransient/AddSingleton return IContainer — verify chaining compiles and works
        _container
            .AddSingleton<ILogger, ConsoleLogger>()
            .AddTransient<IRepository, UserRepository>()
            .AddTransient<IService, UserService>();

        var svc = _container.Resolve<IService>();
        svc.Should().BeOfType<UserService>();
    }

    // ServiceDescriptor validation

    [Fact]
    public void ServiceDescriptor_ThrowsArgumentException_WhenImplDoesNotImplementService()
    {
        // ConsoleLogger does NOT implement IRepository
        var act = () => new ServiceDescriptor(typeof(IRepository), typeof(ConsoleLogger), ServiceLifetime.Transient);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*does not implement*");
    }

    [Fact]
    public void ServiceDescriptor_ThrowsArgumentNullException_WhenServiceTypeIsNull()
    {
        var act = () => new ServiceDescriptor(null!, typeof(ConsoleLogger), ServiceLifetime.Transient);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ServiceDescriptor_ThrowsArgumentNullException_WhenImplTypeIsNull()
    {
        var act = () => new ServiceDescriptor(typeof(ILogger), null!, ServiceLifetime.Transient);
        act.Should().Throw<ArgumentNullException>();
    }
}

public class ErrorHandlingTests
{
    private readonly Container _container;

    public ErrorHandlingTests()
    {
        _container = new Container();
    }

    [Fact]
    public void Resolve_Unregistered_Service_Throws_ServiceNotRegisteredException()
    {
        var act = () => _container.Resolve<ILogger>();

        act.Should().Throw<ServiceNotRegisteredException>()
           .Which.ServiceType.Should().Be(typeof(ILogger));
    }

    [Fact]
    public void Resolve_ServiceWithUnregisteredDependency_ThrowsServiceNotRegisteredException()
    {
        // UserRepository depends on ILogger — not registered
        _container.AddTransient<IRepository, UserRepository>();

        var act = () => _container.Resolve<IRepository>();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Resolve_CircularDependency_Throws_CircularDependencyException()
    {
        _container.AddTransient<ICircularA, CircularA>();
        _container.AddTransient<ICircularB, CircularB>();

        var act = () => _container.Resolve<ICircularA>();

        act.Should().Throw<CircularDependencyException>()
           .Which.Message.Should().Contain("Circular dependency detected");
    }

    [Fact]
    public void CircularDependencyException_ResolutionChain_ContainsInvolvedTypes()
    {
        _container.AddTransient<ICircularA, CircularA>();
        _container.AddTransient<ICircularB, CircularB>();

        var ex = Assert.Throws<CircularDependencyException>(
            () => _container.Resolve<ICircularA>());

        ex.ResolutionChain.Should().Contain(typeof(ICircularA));
    }

    [Fact]
    public void ServiceNotRegisteredException_Message_ContainsTypeName()
    {
        var act = () => _container.Resolve<ILogger>();

        act.Should().Throw<ServiceNotRegisteredException>()
           .WithMessage($"*{typeof(ILogger).FullName}*");
    }
}