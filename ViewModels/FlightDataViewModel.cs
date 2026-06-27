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

        try
        {
            string[] timeInput = FridgeTime.Split(':');
            long hours = long.Parse(timeInput[0]);
            long minutes = long.Parse(timeInput[1]);
        
            fridgeDateTime = fridgeDateTime.AddHours(hours);
            fridgeDateTime = fridgeDateTime.AddMinutes(minutes);
        }
        catch (Exception e)
        {
            if (e is FormatException || e is IndexOutOfRangeException)
            {
                string message;
                if (FridgeTime == String.Empty)
                {
                    message = $"You must set a refridgeration time for flight data {Name}";
                }
                else
                {
                    message = $"Could not parse refridgeration time '{FridgeTime}', ensure it is  in the format 'HH:MM'";
                }

                throw new FormatException(message);    
            }
            else
            {
                throw;
            }
        }
        
        return new FlightData(this.Name, this.Path, fridgeDateTime , marks);
    }
}