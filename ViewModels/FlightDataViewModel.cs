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

    [ObservableProperty]
    private DateTime? _fridgeDate;

    public AnimalMark[] marks = new AnimalMark[0];

    public FlightDataViewModel(FlightData data)
    {
        _name = data.name;
        _path = data.path;

        marks = data.animalMarks;

        _fridgeTime = "";
        if (marks != null && marks.Length > 0)
        {
            _fridgeDate = marks[marks.Length-1].time.Date;       
        }
        else
        {
            _fridgeDate = null;
        }

    }

    public FlightData Get()
    {
        if (FridgeDate == null)
        {
            throw new NullReferenceException("No Refridgeration Date Set.");
            
        }

        DateTime fridgeDateTime = (DateTime) FridgeDate;
        
        string[] timeInput = FridgeTime.Split(':');
        long hours = long.Parse(timeInput[0]);
        long minutes = long.Parse(timeInput[1]);

        fridgeDateTime = fridgeDateTime.AddHours(hours);
        fridgeDateTime = fridgeDateTime.AddMinutes(minutes);

        return new FlightData(this.Name, this.Path, fridgeDateTime , marks);
    }
}