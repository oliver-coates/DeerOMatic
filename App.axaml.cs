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
using System;

namespace Deer_o_matic;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;
 
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
        
            SetupServices(services);

            _serviceProvider = services.BuildServiceProvider();

            desktop.MainWindow.DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            desktop.Startup += OnStartup;
        }
        
        base.OnFrameworkInitializationCompleted();
    }

    private async void OnStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
    {
        if (_serviceProvider == null)
        {
            throw new Exception("Serivce provider does not exist.");
        }

        await _serviceProvider.GetRequiredService<IApplicationInitialiser>().InitialiseAll();
    }

    private static void SetupServices(ServiceCollection services)
    {
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<FileUploadViewModel>();
        services.AddTransient<HunterDeclarationViewModel>();
        services.AddTransient<MapViewModel>();

        services.AddSingleton<IApplicationInitialiser, ApplicationInitialiser>();
        services.AddSingleton<IKmlPickerService, KmlPickerService>();
        services.AddSingleton<IKmlProcessor, KmlProcessor>();
        services.AddSingleton<IKmlPersistenceService, KmlPersistenceService>();
        services.AddSingleton<IAreaProcessorService, AreaProcessorService>();
        services.AddSingleton<IPdfExportService, PdfExportService>();
        services.AddSingleton<IDocumentDataSplitter, DocumentDataSplitter>();
        services.AddSingleton<IDocumentCreationService, DocumentCreationService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IPointProximityService, PointProximityService>();
        services.AddSingleton<IDocumentValidationService, DocumentValidatorService>();
        services.AddSingleton<IPoisonAreaManagerService, PoisonAreaManager>();
        services.AddSingleton<IDocPoisonAreaRetrievalService, DocPoisonAreaRetrievalService>();
    }
}