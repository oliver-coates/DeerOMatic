

using System;

namespace Deer_o_matic.Models;

/// <summary>
/// Represents all the data related to a flight.
/// Created from a .kml file.
/// </summary>
public class FlightData
{

    public enum AnimalType
    {
        NotSet = 0,
        RedDeer = 1,
        FallowDeer = 2,
        Tahr = 3,
        Chamois = 4
    }

    public readonly string name;
    public readonly string path;
    public readonly DateTime? startTime; // Time of the first animal shot
    public readonly DateTime? startTimeUtc;
    public readonly DateTime? endTime; // Time of the last animal shot
    public readonly DateTime? refrigerationTime;
    public readonly DateTime? refrigerationTimeUtc;
    public readonly AnimalMark[] animalMarks;
    public readonly AnimalType animalType;



    public FlightData(string name, string path, DateTime? refridgerationTime, DateTime? refridgerationTimeUtc, AnimalMark[] placemarks, AnimalType animalType=AnimalType.RedDeer)
    {
        this.name = name;
        this.path = path;
        this.refrigerationTime = refridgerationTime;
        this.refrigerationTimeUtc = refridgerationTimeUtc;
        this.animalMarks = placemarks;
        this.animalType = animalType;

        if (placemarks != null && placemarks.Length > 0)
        {
            this.startTime = placemarks[0].time;
            this.startTimeUtc = placemarks[0].timeUtc;
        
            this.endTime = placemarks[^1].time;
        }
        else
        {
            this.startTime = null;
            this.startTimeUtc = null;
            this.endTime = null;
        }
    }

    public static string AnimalTypeAsReadableString(AnimalType type)
    {
        switch (type)
        {
            case (AnimalType.NotSet):
                return "Not Set";
            
            case (AnimalType.FallowDeer):
                return "Fallow Deer";
            
            case (AnimalType.RedDeer):
                return "Red Deer";
            
            case (AnimalType.Tahr):
                return "Tahr";
            
            case (AnimalType.Chamois):
                return "Chamois";
            
            default:
                throw new NullReferenceException($"Unhandled animal type '{type}'");
        }
    }
}