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
    private readonly IKmlPickerService _FilePicker;
    private readonly IDocumentCreationService _DocumentCreation;
    private readonly IPdfExportService _PdfExport;
    private readonly INotificationService _Notifications;
    private readonly IDocumentValidationService _DocumentValidator;

    // Commands:
    public AsyncRelayCommand ExportCommand { get; }

    // Settings
    [ObservableProperty]
    private bool _exportPdfFillable = true;

    [ObservableProperty]
    private bool _doCheckDocPoisonAreas = true;

    // Panel Selection State:
    private int _selectedTabIndex = 0;
    public int SelectedTabIndex
    {
        get
        {
            return _selectedTabIndex;
        }
        set
        {
            SetProperty(ref _selectedTabIndex, value);
            UpdateSelectedContext();
        }
    }

    private object? _selectedContent;
    public object? SelectedContent
    {
        get
        {
            return _selectedContent;
        }
        set
        {
            SetProperty(ref _selectedContent, value);
        }
    }


    public MainWindowViewModel(
        FileUploadViewModel fileUpload,
        HunterDeclarationViewModel hunterDeclaration,
        MapViewModel huntMap,
        IDocumentCreationService documentCreation,
        IPdfExportService pdfExport,
        IKmlPickerService filePicker,
        INotificationService notifications,
        IDocumentValidationService validator
        )
    {
        FileUpload = fileUpload;
        HunterDeclaration = hunterDeclaration;
        HuntMap = huntMap;
        _DocumentValidator = validator;

        _DocumentCreation = documentCreation;
        _PdfExport = pdfExport;
        _FilePicker = filePicker;
        _Notifications = notifications;

        ExportCommand = new AsyncRelayCommand(ExportAsync);

        UpdateSelectedContext();
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
            // Creates a hunter declaration documenet:
            HunterDeclarationDocumentData data = _DocumentCreation.BuildDocument(FileUpload, HunterDeclaration);
    
            // Validates all the data within the document
            _DocumentValidator.ValidateDocument(data, HuntMap.AreaData, DoCheckDocPoisonAreas);

            await _PdfExport.ExportDocumentsAsync(data, saveFolder, ExportPdfFillable);        
        
            await _Notifications.ShowSuccessAsync("✓ Exported Successfully");
        }
        catch (Exception e)
        {
            await _Notifications.ShowErrorAsync(e.ToString());
        }

    }

    private void UpdateSelectedContext()
    {
        SelectedContent = SelectedTabIndex switch
        {
            0 => HunterDeclaration,
            1 => FileUpload,
            2 => HuntMap,
            _ => HunterDeclaration
        };
    }
}
