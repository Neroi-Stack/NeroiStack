# Migration Guide: Single Platform to Multi-Platform

## Overview

NeroiStack has been refactored from a single Desktop-only application to a multi-platform architecture that supports:
- ✅ Desktop (Windows, macOS, Linux)
- ✅ Web (WebAssembly/WASM) 
- ✅ Android
- ✅ iOS

## Project Structure Changes

### New Projects

1. **NeroiStack.Core** - Shared UI library
   - Contains all ViewModels, Views, Components, Converters, Messages
   - Platform-agnostic code shared across all platforms
   - Uses `ServiceLocator` pattern for dependency injection

2. **NeroiStack.Desktop** - Desktop application
   - Replaces the old `NeroiStack` project
   - Uses `IClassicDesktopStyleApplicationLifetime`
   - Full dependency injection with Entity Framework Core

3. **NeroiStack.Browser** - Web application (WASM)
   - Uses `ISingleViewApplicationLifetime`
   - Requires `wasm-tools` workload

4. **NeroiStack.Android** - Android application
   - Targets Android API Level 21+
   - Requires `android` workload

5. **NeroiStack.iOS** - iOS application
   - Targets iOS 11.0+
   - Requires iOS workload and macOS

### Existing Projects (Unchanged)

- **NeroiStack.Agent** - AI Agent business logic
- **NeroiStack.Common** - Common utilities and services
- **NeroiStack.Agent.UnitTest** - Unit tests

### Legacy Project

- **NeroiStack** - Original Desktop-only project
  - Marked for removal after migration is validated
  - Kept temporarily for reference and rollback capability

## Key Changes

### Service Location

The Core library uses a `ServiceLocator` static class instead of `App.Current`:

```csharp
// Old (NeroiStack project)
DataContext = App.Current.Services?.GetRequiredService<MyViewModel>();

// New (NeroiStack.Core)
DataContext = ServiceLocator.Services?.GetRequiredService<MyViewModel>();
```

### Platform-Specific App Setup

Each platform project initializes the `ServiceLocator`:

```csharp
// In Desktop/Browser/Android/iOS App.axaml.cs
public override void OnFrameworkInitializationCompleted()
{
    // Setup DI
    var collection = new ServiceCollection();
    ConfigureServices(collection);
    Services = collection.BuildServiceProvider();
    
    // Set the service locator for Core library
    NeroiStack.ServiceLocator.Services = Services;
    
    // ... rest of initialization
}
```

## Building

### Desktop
```bash
dotnet build NeroiStack.Desktop
dotnet run --project NeroiStack.Desktop
```

### Browser (requires workload)
```bash
dotnet workload install wasm-tools
dotnet build NeroiStack.Browser
```

### Android (requires workload)
```bash
dotnet workload install android
dotnet build NeroiStack.Android -f net10.0-android
```

### iOS (requires workload and macOS)
```bash
dotnet workload install ios
dotnet build NeroiStack.iOS -f net10.0-ios
```

### Build All (without platform-specific projects)
```bash
dotnet build NeroiStack.Core
dotnet build NeroiStack.Desktop
dotnet build NeroiStack.Agent.UnitTest
```

## Migration Steps Completed

1. ✅ Created NeroiStack.Core with shared UI code
2. ✅ Created platform-specific projects (Desktop, Browser, Android, iOS)
3. ✅ Moved ViewModels, Views, Components, Assets to Core
4. ✅ Implemented ServiceLocator pattern for dependency injection
5. ✅ Updated Directory.Packages.props with platform packages
6. ✅ Updated solution file with all projects
7. ✅ Verified Desktop and Core projects build successfully
8. ✅ Legacy project still builds (for rollback capability)

## Next Steps

1. Test Desktop application in production
2. Validate all features work correctly
3. Install platform workloads and test Browser/Android/iOS builds
4. Once validated, remove the legacy `NeroiStack` project

## Rollback Plan

If issues are discovered:
1. The legacy `NeroiStack` project is still functional
2. Simply remove the new projects from the solution
3. Continue using the legacy project until issues are resolved

## Notes

- The Desktop project is a drop-in replacement for the legacy project
- All namespaces remain `NeroiStack` for compatibility
- Platform projects use their own namespaces (e.g., `NeroiStack.Desktop`)
- No breaking changes to the public API
