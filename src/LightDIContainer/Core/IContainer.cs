namespace LightDIContainer.Core;

public interface IContainer
{
    IContainer AddTransient<TService,TImplementation>() where TImplementation : class,TService;
    IContainer AddSingleton<TService,TImplementation>() where TImplementation:class,TService;

    IContainer AddTransient<TImplementation>() where TImplementation : class;
    IContainer AddSingleton<TImplementation>() where TImplementation : class;

    TService Resolve<TService>();
    object Resolve(Type serviceType);
}