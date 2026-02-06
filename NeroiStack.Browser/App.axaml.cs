using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace NeroiStack.Browser;

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
            // NOTE: Browser platform is framework-ready but needs implementation
            // To make this functional:
            // 1. Create a MainView control in NeroiStack.Core for single-view platforms
            // 2. Set up browser-specific dependency injection (may need to use IndexedDB for storage)
            // 3. Initialize ServiceLocator.Services with browser-compatible services
            // singleViewPlatform.MainView = new MainView();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
