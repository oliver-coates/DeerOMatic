using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Deer_o_matic.Models;
using Deer_o_matic.Services;

namespace Deer_o_matic.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // View models:
    public FileUploadViewModel FileUpload { get; }
    public HunterDeclarationViewModel HunterDeclaration { get; }
    public MapViewModel HuntMap { get; }

    // Services:
    private readonly IFilePickerService _FilePicker;
    private readonly IDocumentCreationService _DocumentCreation;
    private readonly IPdfExportService _PdfExport;
    private readonly INotificationService _Notifications;

    // Commands:
    public AsyncRelayCommand ExportCommand { get; }

    // Settings
    [ObservableProperty]
    private bool _exportPdfFillable = false;

    public MainWindowViewModel(
        FileUploadViewModel fileUpload,
        HunterDeclarationViewModel hunterDeclaration,
        MapViewModel huntMap,
        IDocumentCreationService documentCreation,
        IPdfExportService pdfExport,
        IFilePickerService filePicker,
        INotificationService notifications)
    {
        FileUpload = fileUpload;
        HunterDeclaration = hunterDeclaration;
        HuntMap = huntMap;

        _DocumentCreation = documentCreation;
        _PdfExport = pdfExport;
        _FilePicker = filePicker;
        _Notifications = notifications;

        ExportCommand = new AsyncRelayCommand(ExportAsync);
    }


    private async Task ExportAsync()
    {
        IStorageFolder? saveFolder = await _FilePicker.PickFileSaveLocation();

        if (saveFolder == null)
        {
            // User cancelled.
            return;
        }

        try
        {
            HunterDeclarationDocumentData data = _DocumentCreation.BuildDocument(FileUpload, HunterDeclaration);
    
            await _PdfExport.ExportDocumentAsync(data, saveFolder, ExportPdfFillable);        
        
            await _Notifications.ShowSuccessAsync("✓ Exported Successfully");
        }
        catch (Exception e)
        {
            await _Notifications.ShowErrorAsync(e.Message);
        }

    }
}
