using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Deer_o_matic.Models;
using Deer_o_matic.Services;

namespace Deer_o_matic.ViewModels;

public partial class FileUploadViewModel : ViewModelBase
{
    private readonly IFilePickerService _kmlPicker;
    private readonly IKmlProcessor _kmlProcessor;

    public ObservableCollection<FlightDataViewModel> FlightData { get; } = [];
    
    public AsyncRelayCommand OpenFileCommand {get; } 

    [ObservableProperty]
    public partial string? HunterName { get; set; }


    public FileUploadViewModel(IFilePickerService filePicker, IKmlProcessor kmlProcessor)
    {
        _kmlPicker = filePicker;
        _kmlProcessor = kmlProcessor;
        OpenFileCommand = new AsyncRelayCommand(PickKmlAsync);
    }

    public void RemoveAllFlightData()
    {
        FlightData.Clear();
    }

    [RelayCommand]
    public void RemoveFlightData(FlightDataViewModel toRemove)
    {
        FlightData.Remove(toRemove);
    }

    private async Task PickKmlAsync()
    {
        PickedFile[] kmlFiles = await _kmlPicker.OpenFilesAsync();

        if (kmlFiles is null)
        {
            // No files were picked.
            return;
        }

        await AttemptExtractFiles(kmlFiles);                
    }

    private async Task AttemptExtractFiles(PickedFile[] kmlFiles)
    {
        foreach (PickedFile file in kmlFiles)
        {
            try
            {
                FlightData flightData = await _kmlProcessor.CreateFlightData(file);

                FlightData.Add(new FlightDataViewModel(flightData));   
            }
            catch (Exception e)
            {
                // TODO: Handle exceptions
                Console.WriteLine($"Error - Unhandled Exception when picking KML: {e.ToString()}");
            }
            
        }
    }

}