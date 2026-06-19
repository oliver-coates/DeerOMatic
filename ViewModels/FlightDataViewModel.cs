using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Deer_o_matic.Models;

namespace Deer_o_matic.ViewModels;

public partial class FlightDataViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _path;

    [ObservableProperty]
    private string _fridgeTime;

    private AnimalMark[] marks;

    public FlightDataViewModel(FlightData data)
    {
        _name = data.name;
        _path = data.path;
        _fridgeTime = "";
        marks = data.animalMarks;
    }

    public FlightData Get()
    {
        return new FlightData(this.Name, this.Path, DateTime.Now , marks);
    }
}