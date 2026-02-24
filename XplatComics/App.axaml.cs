using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using XplatComics.Services;
using XplatComics.ViewModels;
using XplatComics.Views;

namespace XplatComics;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        // Services
        services.AddSingleton<ILibraryStore, JsonLibraryStore>();
        services.AddSingleton<IPageCacheService, PageCacheService>();
        services.AddSingleton<IThumbnailService, ThumbnailService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<LibraryViewModel>();
        services.AddTransient<ReaderViewModel>();

        // Navigation (registered after building so it can receive IServiceProvider)
        var provider = services.BuildServiceProvider();
        var mainViewModel = provider.GetRequiredService<MainViewModel>();

        // Register navigation service with the built provider
        var navigationService = new NavigationService(provider, mainViewModel);
        ServiceLocator.Services = provider;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
