namespace LightDIContainer.Exceptions;

/// <summary>
/// Thrown when a requested service type has not been registered in the container.
/// </summary>
public sealed class ServiceNotRegisteredException : Exception
{
    public Type ServiceType { get; }

    public ServiceNotRegisteredException(Type serviceType)
        : base($"Service of type '{serviceType.FullName}' has not been registered.")
    {
        ServiceType = serviceType;
    }
}

/// <summary>
/// Thrown when a circular dependency is detected during resolution.
/// </summary>
public sealed class CircularDependencyException : Exception
{
    public IReadOnlyList<Type> ResolutionChain { get; }

    public CircularDependencyException(IEnumerable<Type> resolutionChain)
        : base(BuildMessage(resolutionChain))
    {
        ResolutionChain = resolutionChain.ToList().AsReadOnly();
    }

    private static string BuildMessage(IEnumerable<Type> chain)
    {
        var names = chain.Select(t => t.Name);
        return $"Circular dependency detected: {string.Join(" → ", names)}";
    }
}