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
    public static event Action<FlightData>? OnFlightDataAdded;

    private readonly IFilePickerService _KmlPicker;
    private readonly IKmlProcessor _KmlProcessor;
    private readonly INotificationService _Notifications;

    public ObservableCollection<FlightDataViewModel> FlightData { get; } = [];
    
    public AsyncRelayCommand OpenFileCommand {get; } 

    [ObservableProperty]
    public partial string? HunterName { get; set; }


    public FileUploadViewModel(IFilePickerService filePicker, IKmlProcessor kmlProcessor, INotificationService notifications)
    {
        _KmlPicker = filePicker;
        _KmlProcessor = kmlProcessor;
        _Notifications = notifications;

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
        PickedFile[] kmlFiles = await _KmlPicker.OpenFilesAsync();

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
                FlightData flightData = await _KmlProcessor.CreateFlightData(file);

                FlightData.Add(new FlightDataViewModel(flightData));   

                OnFlightDataAdded?.Invoke(flightData);
            }
            catch (Exception e)
            {
                string errorMessage = $"Error - Unhandled Exception when picking KML: {e.ToString()}"; 
                await _Notifications.ShowErrorAsync(errorMessage);
            }
            
        }
    }

}