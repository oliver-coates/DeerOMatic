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
    public static event Action<FlightDataViewModel>? OnFlightDataAdded;
    public static event Action? OnFlightDataCleared;
    public static event Action<FlightDataViewModel>? OnFlightDataRemoved;

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
        OnFlightDataCleared?.Invoke();
    }

    [RelayCommand]
    public void RemoveFlightData(FlightDataViewModel toRemove)
    {
        FlightData.Remove(toRemove);
        OnFlightDataRemoved?.Invoke(toRemove);
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

                EnsureFlightDataNameIsUnique(flightData.name);

                FlightDataViewModel viewModel = new FlightDataViewModel(flightData); 
                FlightData.Add(viewModel);   

                OnFlightDataAdded?.Invoke(viewModel);
            }
            catch (Exception e)
            {
                string errorMessage = $"Error - Unhandled Exception when picking KML: {e.Message}"; 
                await _Notifications.ShowErrorAsync(errorMessage);
            }
            
        }
    }

    private void EnsureFlightDataNameIsUnique(string name)
    {
        foreach (FlightDataViewModel flightViewModel in FlightData)
        {
            if (flightViewModel.Name == name)
            {
                throw new Exception($"KML files cannot have duplicate names ('{name}' already exists)");
            }
             
        }
    }

}