using System;

namespace NeroiStack;

/// <summary>
/// Service locator for accessing dependency injection services from the Core library.
/// The platform-specific App should initialize this during startup using the Initialize method.
/// </summary>
public static class ServiceLocator
{
    private static IServiceProvider? _services;
    
    /// <summary>
    /// Gets the current service provider instance.
    /// </summary>
    public static IServiceProvider? Services => _services;
    
    /// <summary>
    /// Initializes the service locator with the given service provider.
    /// This should be called once during application startup by the platform-specific App.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use.</param>
    /// <exception cref="InvalidOperationException">Thrown if already initialized.</exception>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        if (_services != null)
        {
            throw new InvalidOperationException("ServiceLocator has already been initialized.");
        }
        _services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }
    
    /// <summary>
    /// Resets the service locator. Only for testing purposes.
    /// </summary>
    internal static void Reset()
    {
        _services = null;
    }
}
