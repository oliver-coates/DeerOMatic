using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Deer_o_matic.Services;

namespace Deer_o_matic.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IFilePickerService _kmlPicker;

    public ObservableCollection<FlightDataViewModel> FlightData { get; } = [];
    
    public AsyncRelayCommand OpenFileCommand {get; } 

    [ObservableProperty]
    public partial string? HunterName { get; set; }


    public MainWindowViewModel(IFilePickerService filePicker)
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
        string[] content = await _kmlPicker.OpenFileAsync();
        
        if (content is not null)
        {
            Console.WriteLine($"Got:");
        
            foreach (string s in content)
            {
                Console.WriteLine($">> {s}");
            }
        }
        else
        {
            Console.WriteLine($"Content was null");
        }
    }
}
