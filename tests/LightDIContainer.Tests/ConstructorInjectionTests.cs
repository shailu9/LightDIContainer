using FluentAssertions;
using LightDIContainer.Core;
using LightDIContainer.Tests.Fixtures;

namespace LightDIContainer.Tests;

public class ConstructorInjectionTests
{
    private readonly Container _container;

    public ConstructorInjectionTests()
    {
        _container = new Container();
    }

    [Fact]
    public void Resolve_WithSingleDependency_InjectsDependency()
    {
        _container.AddTransient<ILogger, ConsoleLogger>();
        _container.AddTransient<IRepository, UserRepository>();

        var repo = _container.Resolve<IRepository>();

        repo.Should().BeOfType<UserRepository>();
        // Verify injection worked by exercising the dependency
        var act = () => repo.Save("test");
        act.Should().NotThrow();
    }

    [Fact]
    public void Resolve_WithMultipleDependencies_AllInjected()
    {
        _container.AddTransient<ILogger, ConsoleLogger>();
        _container.AddTransient<IRepository, UserRepository>();
        _container.AddTransient<IService, UserService>();

        // UserService needs both IRepository and ILogger
        var svc = _container.Resolve<IService>();

        svc.Should().BeOfType<UserService>();
        var act = () => svc.Execute();
        act.Should().NotThrow();
    }

    [Fact]
    public void Resolve_DeepDependencyChain_AllLevelsInjected()
    {
        // OrderService → UserService → UserRepository → ConsoleLogger
        _container.AddTransient<ILogger, ConsoleLogger>();
        _container.AddTransient<IRepository, UserRepository>();
        _container.AddTransient<IService, UserService>();
        _container.AddTransient<IOrderService, OrderService>();

        var orderSvc = _container.Resolve<IOrderService>();

        orderSvc.Should().BeOfType<OrderService>();
        var act = () => orderSvc.PlaceOrder("widget");
        act.Should().NotThrow();
    }

    [Fact]
    public void Resolve_ByNonGenericType_ReturnsCorrectInstance()
    {
        _container.AddTransient<ILogger, ConsoleLogger>();

        var logger = _container.Resolve(typeof(ILogger));

        logger.Should().NotBeNull();
        logger.Should().BeAssignableTo<ILogger>();
    }
}

public class MixedLifetimeTests
{
    private readonly Container _container;

    public MixedLifetimeTests()
    {
        _container = new Container();
    }

    [Fact]
    public void Singleton_DependingOn_Transient_IsCreatedOnce()
    {
        _container.AddTransient<ILogger, ConsoleLogger>();
        _container.AddSingleton<IRepository, UserRepository>();

        var repo1 = _container.Resolve<IRepository>();
        var repo2 = _container.Resolve<IRepository>();

        // Repository is singleton — same instance
        repo1.Should().BeSameAs(repo2);
    }

    [Fact]
    public void Transient_DependingOn_Singleton_GetsTheSameSingletonEachTime()
    {
        _container.AddSingleton<ILogger, ConsoleLogger>();
        _container.AddTransient<IRepository, UserRepository>();

        var repo1 = _container.Resolve<IRepository>();
        var repo2 = _container.Resolve<IRepository>();

        // Repos are different (transient)
        repo1.Should().NotBeSameAs(repo2);

        // But both should share the SAME logger singleton (verify via state)
        var sharedLogger = (ConsoleLogger)_container.Resolve<ILogger>();
        repo1.Save("from-repo1");
        repo2.Save("from-repo2");

        sharedLogger.Messages.Should().Contain("Saved: from-repo1");
        sharedLogger.Messages.Should().Contain("Saved: from-repo2");
    }

    [Fact]
    public void Mixed_DeepGraph_SingletonSharedAcrossAllTransientBranches()
    {
        _container.AddSingleton<ILogger, ConsoleLogger>();   // shared
        _container.AddTransient<IRepository, UserRepository>(); // new each time
        _container.AddTransient<IService, UserService>();        // new each time
        _container.AddTransient<IOrderService, OrderService>();  // new each time

        var order1 = _container.Resolve<IOrderService>();
        var order2 = _container.Resolve<IOrderService>();

        order1.Should().NotBeSameAs(order2);   // transient
        order1.PlaceOrder("item-A");
        order2.PlaceOrder("item-B");

        // All logging went through the single ILogger singleton
        var logger = _container.Resolve<ILogger>();
        logger.Messages.Should().Contain("Placing order: item-A");
        logger.Messages.Should().Contain("Placing order: item-B");
    }
}