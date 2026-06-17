using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Deer_o_matic.ViewModels;
using Deer_o_matic.Views;
using Avalonia.Controls;
using Deer_o_matic.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Deer_o_matic;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
 
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            desktop.MainWindow = mainWindow; 

            TopLevel? topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
            if (topLevel != null)
            {
                services.AddSingleton(topLevel);            
            }
            services.AddSingleton<IFilePickerService, KmlPickerService>();
        }

        services.AddTransient<MainWindowViewModel>();

        var provider = services.BuildServiceProvider();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop2)
        {
            desktop2.MainWindow!.DataContext = provider.GetRequiredService<MainWindowViewModel>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}