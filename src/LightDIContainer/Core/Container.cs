using LightDIContainer.Exceptions;

namespace LightDIContainer.Core;

/// <summary>
/// LightDIContainer — a lightweight DI container supporting constructor injection
/// with Singleton and Transient lifetimes.
/// </summary>
public sealed class Container : IContainer
{
    private readonly List<ServiceDescriptor> _descriptors = new();
    private readonly Dictionary<Type, object> _singletonCache = new();
    private readonly object _lock = new();

    // -------------------------------------------------------------------------
    // Registration API
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public IContainer AddTransient<TService, TImplementation>()
        where TImplementation : class, TService
    {
        Register(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient);
        return this;
    }

    /// <inheritdoc/>
    public IContainer AddSingleton<TService, TImplementation>()
        where TImplementation : class, TService
    {
        Register(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton);
        return this;
    }

    /// <inheritdoc/>
    public IContainer AddTransient<TImplementation>()
        where TImplementation : class
    {
        Register(typeof(TImplementation), typeof(TImplementation), ServiceLifetime.Transient);
        return this;
    }

    /// <inheritdoc/>
    public IContainer AddSingleton<TImplementation>()
        where TImplementation : class
    {
        Register(typeof(TImplementation), typeof(TImplementation), ServiceLifetime.Singleton);
        return this;
    }

    private void Register(Type serviceType, Type implType, ServiceLifetime lifetime)
    {
        var descriptor = new ServiceDescriptor(serviceType, implType, lifetime);
        lock (_lock)
        {
            _descriptors.Add(descriptor);
        }
    }

    // -------------------------------------------------------------------------
    // Resolution API
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public TService Resolve<TService>() => (TService)Resolve(typeof(TService));

    /// <inheritdoc/>
    public object Resolve(Type serviceType) =>
        ResolveInternal(serviceType, resolutionStack: new Stack<Type>());

    private object ResolveInternal(Type serviceType, Stack<Type> resolutionStack)
    {
        // Circular dependency guard — checked and tracked at service-type level
        if (resolutionStack.Contains(serviceType))
        {
            var chain = resolutionStack.Reverse().Append(serviceType);
            throw new CircularDependencyException(chain);
        }

        var descriptor = GetDescriptor(serviceType);

        resolutionStack.Push(serviceType);
        try
        {
            if (descriptor.Lifetime == ServiceLifetime.Singleton)
                return ResolveSingleton(descriptor, resolutionStack);

            return CreateInstance(descriptor.ImplementationType, resolutionStack);
        }
        finally
        {
            resolutionStack.Pop();
        }
    }

    private object ResolveSingleton(ServiceDescriptor descriptor, Stack<Type> resolutionStack)
    {
        // Double-checked locking for thread-safe singleton creation
        if (_singletonCache.TryGetValue(descriptor.ServiceType, out var cached))
            return cached;

        lock (_lock)
        {
            if (_singletonCache.TryGetValue(descriptor.ServiceType, out cached))
                return cached;

            var instance = CreateInstance(descriptor.ImplementationType, resolutionStack);
            _singletonCache[descriptor.ServiceType] = instance;
            return instance;
        }
    }

    private object CreateInstance(Type type, Stack<Type> resolutionStack)
    {
        // Greedy constructor selection: pick the constructor with most parameters
        var constructors = type.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .ToList();

        if (constructors.Count == 0)
            throw new InvalidOperationException(
                $"No public constructors found on type '{type.Name}'.");

        // Try constructors from most to least parameters (greedy strategy)
        foreach (var ctor in constructors)
        {
            var parameters = ctor.GetParameters();

            // Check all parameters can be resolved before committing
            var resolved = new object[parameters.Length];
            bool canResolve = true;

            for (int i = 0; i < parameters.Length; i++)
            {
                try
                {
                    resolved[i] = ResolveInternal(parameters[i].ParameterType, resolutionStack);
                }
                catch (ServiceNotRegisteredException)
                {
                    canResolve = false;
                    break;
                }
            }

            if (canResolve)
                return Activator.CreateInstance(type, resolved)!;
        }

        throw new InvalidOperationException(
            $"Unable to resolve constructor for '{type.Name}'. " +
            $"Ensure all constructor dependencies are registered.");
    }

    private ServiceDescriptor GetDescriptor(Type serviceType)
    {
        lock (_lock)
        {
            // Last registration wins (supports overriding)
            return _descriptors.LastOrDefault(d => d.ServiceType == serviceType)
                ?? throw new ServiceNotRegisteredException(serviceType);
        }
    }

    // -------------------------------------------------------------------------
    // Diagnostics
    // -------------------------------------------------------------------------

    /// <summary>Returns a snapshot of all current service registrations.</summary>
    public IReadOnlyList<ServiceDescriptor> GetRegistrations()
    {
        lock (_lock)
        {
            return _descriptors.ToList().AsReadOnly();
        }
    }
}
