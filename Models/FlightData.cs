

using System;

namespace Deer_o_matic.Models;

/// <summary>
/// Represents all the data related to a flight.
/// Created from a .kml file.
/// </summary>
public class FlightData
{
    public readonly string name;
    public readonly string path;
    public readonly DateTime? startTime; // Time of the first animal shot
    public readonly string refrigerationTime; // As a string as the user just types the time in directly
    public readonly AnimalMark[] animalMarks;



    public FlightData(string name, string path, string refridgerationTime, AnimalMark[] placemarks)
    {
        this.name = name;
        this.path = path;
        this.refrigerationTime = refridgerationTime;
        this.animalMarks = placemarks;

        if (placemarks != null && placemarks.Length > 0)
        {
            this.startTime = placemarks[0].time;        
        }
        else
        {
            this.startTime = null;
        }
    }

}