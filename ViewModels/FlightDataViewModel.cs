using CommunityToolkit.Mvvm.ComponentModel;
using Deer_o_matic.Models;

namespace Deer_o_matic.ViewModels;

public partial class FlightDataViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _name;

    private AnimalMark[] marks;

    public FlightDataViewModel(string name)
    {
        _name = name;
        marks = [];
    }

    public FlightDataViewModel(FlightData data)
    {
        _name = data.name;
        marks = data.animalMarks;
    }

    public FlightData Get()
    {
        return new FlightData(this.Name, marks);
    }
}