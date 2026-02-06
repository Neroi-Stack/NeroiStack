using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using NeroiStack.ViewModels;
using NeroiStack.Views;
using Microsoft.Extensions.DependencyInjection;
using NeroiStack.Agent.Data;
using Microsoft.EntityFrameworkCore;
using System;
using NeroiStack.Agent.Interface;
using NeroiStack.Agent.Service;
using NeroiStack.Common.Interface;
using NeroiStack.Common.Service;
using NeroiStack.Agent.Factories;
using NeroiStack.Agent.Extensions;

namespace NeroiStack.Desktop;

public partial class App : Application
{
    public new static App Current => (App)Application.Current!;
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 1. Setup DI
        var collection = new ServiceCollection();
        ConfigureServices(collection);
        Services = collection.BuildServiceProvider();
        
        // Set the service locator for Core library
        NeroiStack.ServiceLocator.Initialize(Services);
        
        using (var scope = Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ChatContext>();
            try
            {
                // dbContext.Database.Migrate();
                dbContext.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                // Log or handle migration failure
                Console.WriteLine($"Database migration failed: {ex.Message}");
            }
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            var mainViewModel = Services.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();
        services.AddDbContext<ChatContext>(options => options.UseSqlite($"Data Source=neroi_chats.db"));
        services.AddScoped<IChatContext>(provider => provider.GetRequiredService<ChatContext>());
        services.AddScoped<IKeyManageService, KeyManageService>();
        services.AddSingleton<IChatService, ChatService>();
        services.AddScoped<IChatManageService, ChatManageService>();
        services.AddScoped<IAgentManageService, AgentManageService>();
        // services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IPluginManageService, PluginManageService>();
        services.AddScoped<IChatInstanceService, ChatInstanceService>();
        services.AddScoped<IEncryption, Encryption>();
        services.AddScoped<IMimeType, MimeType>();

        // Factories & Strategies
        services.AddScoped<IKernelFactory, KernelFactory>();

        // Automatically scan and register all strategies (Provider, PluginLoader, Orchestration)
        services.AddAgentStrategies();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ChatBotViewModel>();
        services.AddTransient<PluginManageViewModel>();
        services.AddTransient<AgentManageViewModel>();
        services.AddTransient<ChatManageViewModel>();
        services.AddTransient<KeyManagementViewModel>();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
