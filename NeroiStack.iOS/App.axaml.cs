using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace NeroiStack.iOS;

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
            // NOTE: iOS platform is framework-ready but needs implementation
            // To make this functional:
            // 1. Create a MainView control in NeroiStack.Core for single-view platforms
            // 2. Set up iOS-specific dependency injection (use SQLite with iOS paths)
            // 3. Initialize ServiceLocator.Services with iOS-compatible services
            // singleViewPlatform.MainView = new MainView();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
