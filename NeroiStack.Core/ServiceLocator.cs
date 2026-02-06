using System;

namespace NeroiStack;

/// <summary>
/// Service locator for accessing dependency injection services from the Core library.
/// The platform-specific App should set the Services property during initialization.
/// </summary>
public static class ServiceLocator
{
    public static IServiceProvider? Services { get; set; }
}
