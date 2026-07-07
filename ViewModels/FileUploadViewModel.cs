using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Deer_o_matic.Models;
using Deer_o_matic.Services;

namespace Deer_o_matic.ViewModels;

public partial class FileUploadViewModel : ViewModelBase
{

    public static event Action<object?, NotifyCollectionChangedEventArgs>? OnFlightDataChanged;

    private readonly IKmlPickerService _KmlPicker;
    private readonly IKmlProcessor _KmlProcessor;
    private readonly INotificationService _Notifications;

    public ObservableCollection<FlightDataViewModel> FlightData { get; } = [];
    
    public AsyncRelayCommand OpenFileCommand {get; } 


    public FileUploadViewModel(IKmlPickerService filePicker, IKmlProcessor kmlProcessor, INotificationService notifications)
    {
        _KmlPicker = filePicker;
        _KmlProcessor = kmlProcessor;
        _Notifications = notifications;

        OpenFileCommand = new AsyncRelayCommand(PickKmlAsync);

        FlightData.CollectionChanged += FlightDataChanged;
    }


    #region Flight Data Collection changed routing
    // This just exposes this flight data collection changed event so the map can access it.
    private void FlightDataChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnFlightDataChanged?.Invoke(sender, e);
    }
    #endregion

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
                FlightData flightData = await _KmlProcessor.ParseFlightDataFromKmlAsync(file);

                EnsureFlightDataNameIsUnique(flightData.name);

                FlightDataViewModel viewModel = new FlightDataViewModel(flightData); 
                FlightData.Add(viewModel);   
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