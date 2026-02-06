using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace NeroiStack.Android;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            // NOTE: Android platform is framework-ready but needs implementation
            // To make this functional:
            // 1. Create a MainView control in NeroiStack.Core for single-view platforms
            // 2. Set up Android-specific dependency injection (use SQLite with Android paths)
            // 3. Initialize ServiceLocator.Services with Android-compatible services
            // singleViewPlatform.MainView = new MainView();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
