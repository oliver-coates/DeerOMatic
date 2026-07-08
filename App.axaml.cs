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
using System.Threading.Tasks;

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
            services.AddSingleton<IKmlPickerService, KmlPickerService>();
        }

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<FileUploadViewModel>();
        services.AddTransient<HunterDeclarationViewModel>();
        services.AddTransient<MapViewModel>();

        services.AddSingleton<IKmlProcessor, KmlProcessor>();
        services.AddSingleton<IKmlPersistenceService, KmlPersistenceService>();
        services.AddSingleton<IAreaProcessorService, AreaProcessorService>();
        services.AddSingleton<IPdfExportService, PdfExportService>();
        services.AddSingleton<IDocumentCreationService, DocumentCreationService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IPointProximityService, PointProximityService>();
        services.AddSingleton<IDocumentValidationService, DocumentValidatorService>();


        var provider = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop2)
        {
            desktop2.MainWindow!.DataContext = provider.GetRequiredService<MainWindowViewModel>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}