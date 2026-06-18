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

    public ObservableCollection<FlightDataViewModel> FlightData { get; } = [];
    
    public AsyncRelayCommand OpenFileCommand {get; } 

    [ObservableProperty]
    public partial string? HunterName { get; set; }


    public FileUploadViewModel(IFilePickerService filePicker)
    {
        _kmlPicker = filePicker;
        OpenFileCommand = new AsyncRelayCommand(PickKmlAsync);
    }



    [RelayCommand]
    public void RemoveFlightData(FlightDataViewModel toRemove)
    {
        FlightData.Remove(toRemove);
    }

    private async Task PickKmlAsync()
    {
        FileOutput[] kmlFiles = await _kmlPicker.OpenFilesAsync();
        
        if (kmlFiles is null)
        {
            // No files were picked.
            return;
        }

        foreach (FileOutput file in kmlFiles)
        {
            FlightData flightData = KmlProcessor.CreateFlightDataFromFile(file);
            
            FlightData.Add(new FlightDataViewModel(flightData));
        }
    }
}