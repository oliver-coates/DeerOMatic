using CommunityToolkit.Mvvm.ComponentModel;
using Deer_o_matic.Models;

namespace Deer_o_matic.ViewModels;

public partial class FlightDataViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _name;

    public FlightDataViewModel(string name)
    {
        _name = name;
    }

    public FlightData Get()
    {
        return new FlightData(this.Name, new AnimalMark[0]);
    }
}