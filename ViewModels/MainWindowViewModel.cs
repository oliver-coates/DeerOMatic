using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Deer_o_matic.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<FlightDataViewModel> FlightData { get; } = [];

    [ObservableProperty]
    public partial string? HunterName { get; set; }

    [RelayCommand]
    public void AddNewFlightData()
    {
        if (HunterName == null)
        {
            FlightData.Add(new FlightDataViewModel("Unnamed"));        
        }
        else
        {
            FlightData.Add(new FlightDataViewModel(HunterName));
            
        }
    }

    [RelayCommand]
    public void RemoveFlightData(FlightDataViewModel toRemove)
    {
        FlightData.Remove(toRemove);
    }
}
