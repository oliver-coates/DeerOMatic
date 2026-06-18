using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
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

    // Services:
    private readonly IFilePickerService _filePicker;
    private readonly IDocumentCreationService _documentCreation;
    private readonly IPdfExportService _pdfExport;

    // Commands:
    public AsyncRelayCommand ExportCommand { get; }

    public MainWindowViewModel(
        FileUploadViewModel fileUpload,
        HunterDeclarationViewModel hunterDeclaration,
        IDocumentCreationService documentCreation,
        IPdfExportService pdfExport,
        IFilePickerService filePicker)
    {
        FileUpload = fileUpload;
        HunterDeclaration = hunterDeclaration;

        _documentCreation = documentCreation;
        _pdfExport = pdfExport;
        _filePicker = filePicker;

        ExportCommand = new AsyncRelayCommand(ExportAsync);
    }


    private async Task ExportAsync()
    {
        // TODO: Export here
    }
}
