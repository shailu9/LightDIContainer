namespace LightDIContainer.Core;

public class ServiceDescriptor
{
    public Type ServiceType { get; }
    public Type ImplementationType { get; }
    public ServiceLifetime Lifetime { get; }

    public ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(implementationType);

        if (!serviceType.IsAssignableFrom(implementationType))
            throw new ArgumentException(
                $"Type '{implementationType.Name}' does not implement or extend '{serviceType.Name}'.",
                nameof(implementationType));

        ServiceType = serviceType;
        ImplementationType = implementationType;
        Lifetime = lifetime;
    }
}