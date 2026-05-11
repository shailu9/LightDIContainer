using FluentAssertions;
using LightDIContainer.Core;
using LightDIContainer.Tests.Fixtures;

namespace LightDIContainer.Tests;

public class SingletonLifetimeTests
{
    private readonly Container _container;

    public SingletonLifetimeTests()
    {
        _container = new Container();
    }

    [Fact]
    public void Resolve_Singleton_ReturnsSameInstanceEachTime()
    {
        _container.AddSingleton<ILogger, ConsoleLogger>();

        var first = _container.Resolve<ILogger>();
        var second = _container.Resolve<ILogger>();

        first.Should().BeSameAs(second);
    }

    [Fact]
    public void Resolve_Singleton_ReturnsSameInstance_AcrossMultipleCalls()
    {
        _container.AddSingleton<ILogger, ConsoleLogger>();

        var instances = Enumerable.Range(0, 10)
            .Select(_ => _container.Resolve<ILogger>())
            .ToList();

        instances.Should().AllSatisfy(i => i.Should().BeSameAs(instances[0]));
    }

    [Fact]
    public void Resolve_Singleton_ReturnsCorrectImplementationType()
    {
        _container.AddSingleton<ILogger, ConsoleLogger>();

        var logger = _container.Resolve<ILogger>();

        logger.Should().BeOfType<ConsoleLogger>();
    }

    [Fact]
    public void Resolve_Singleton_SharedAcrossDependents_SameInstance()
    {
        // Logger is singleton — both UserRepository and UserService should get the SAME logger
        _container.AddSingleton<ILogger, ConsoleLogger>();
        _container.AddTransient<IRepository, UserRepository>();
        _container.AddTransient<IService, UserService>();

        var service = _container.Resolve<IService>();

        // Call Execute which logs via both the service and the repository
        service.Execute();
        var logger = _container.Resolve<ILogger>();
        // The logger used by the service and repository should be the same instance as the one resolved directly
        logger.Messages.Should().Contain("Saved: User data");
        logger.Messages.Should().Contain("Executing UserService");
    }
}