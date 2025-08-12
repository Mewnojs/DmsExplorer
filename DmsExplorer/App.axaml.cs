using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DmsExplorer.Services;
using DmsExplorer.ViewModels;
using DmsExplorer.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DmsExplorer;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };

            var services = new ServiceCollection();

            services.AddSingleton<IFilesService>(x => new FilesService(desktop.MainWindow));
            services.AddSingleton<IClipBoardService>(x => new ClipboardService(desktop.MainWindow));

            Services = services.BuildServiceProvider();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };

            var services = new ServiceCollection();

            services.AddSingleton<IFilesService>(x => new FilesService(TopLevel.GetTopLevel(singleViewPlatform.MainView)!));
            services.AddSingleton<IClipBoardService>(x => new ClipboardService(TopLevel.GetTopLevel(singleViewPlatform.MainView)!));

            Services = services.BuildServiceProvider();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public new static App? Current => Application.Current as App;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider? Services { get; private set; }
}
