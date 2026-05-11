using FluentAssertions;
using LightDIContainer.Core;
using LightDIContainer.Tests.Fixtures;

namespace LightDIContainer.Tests;

public class TransientLifetimeTests
{
    private readonly Container _container;

    public TransientLifetimeTests()
    {
        _container = new Container();
    }

    [Fact]
    public void Resolve_Transient_ReturnsNewInstanceEachTime()
    {
        _container.AddTransient<ILogger, ConsoleLogger>();

        var first = _container.Resolve<ILogger>();
        var second = _container.Resolve<ILogger>();

        first.Should().NotBeSameAs(second);
    }

    [Fact]
    public void Resolve_Transient_ReturnsCorrectImplementationType()
    {
        _container.AddTransient<ILogger, ConsoleLogger>();

        var logger = _container.Resolve<ILogger>();

        logger.Should().BeOfType<ConsoleLogger>();
    }

    [Fact]
    public void Resolve_Transient_WithDependency_InjectsCorrectly()
    {
        _container.AddTransient<ILogger, ConsoleLogger>();
        _container.AddTransient<IRepository, UserRepository>();

        var repo = _container.Resolve<IRepository>();

        repo.Should().NotBeNull();
        repo.Should().BeOfType<UserRepository>();
    }

    [Fact]
    public void Resolve_Transient_WithDependency_DependencyIsAlsoTransient_NewInstanceEachCall()
    {
        _container.AddTransient<ILogger, ConsoleLogger>();
        _container.AddTransient<IRepository, UserRepository>();

        var repo1 = _container.Resolve<IRepository>();
        var repo2 = _container.Resolve<IRepository>();

        repo1.Should().NotBeSameAs(repo2);
    }

    [Fact]
    public void Resolve_Transient_SelfRegistered_ReturnsNewInstanceEachTime()
    {
        _container.AddTransient<ILogger, ConsoleLogger>();
        _container.AddTransient<StandaloneService>();

        var svc1 = _container.Resolve<StandaloneService>();
        var svc2 = _container.Resolve<StandaloneService>();

        svc1.Should().NotBeSameAs(svc2);
    }

    [Fact]
    public void Resolve_Transient_NoDependencies_CreatesInstance()
    {
        _container.AddTransient<NoDepService>();

        var svc = _container.Resolve<NoDepService>();

        svc.Should().NotBeNull();
        svc.WasCreated.Should().BeTrue();
    }

    [Fact]
    public void Resolve_Transient_DeepGraph_AllNodesResolved()
    {
        _container.AddTransient<ILogger, ConsoleLogger>();
        _container.AddTransient<IRepository, UserRepository>();
        _container.AddTransient<IService, UserService>();
        _container.AddTransient<IOrderService, OrderService>();

        var orderSvc = _container.Resolve<IOrderService>();

        orderSvc.Should().NotBeNull();
        orderSvc.Should().BeOfType<OrderService>();
    }
}