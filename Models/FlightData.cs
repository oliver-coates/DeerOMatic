

using System;

namespace Deer_o_matic.Models;

/// <summary>
/// Represents all the data related to a flight.
/// Created from a .kml file.
/// </summary>
public class FlightData
{
    public const int MAXIMUM_REFRIDGERATION_HOURS = 10;

    public readonly string name;
    public readonly string path;
    public readonly DateTime? startTime; // Time of the first animal shot
    public readonly DateTime? refrigerationTime;
    public readonly AnimalMark[] animalMarks;



    public FlightData(string name, string path, DateTime? refridgerationTime, AnimalMark[] placemarks)
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

    /// <summary>
    /// Checks the time between the refridgeration and the first kill and throws an error if it is greater than <see cref="MAXIMUM_REFRIDGERATION_HOURS"/>.
    /// Only call this method if <see cref="startTime"/> and <see cref="refrigerationTime"/> is not null.
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="RefridgerationTimeException"></exception>
    public void ValidateTime()
    {
        if (startTime == null)
        {
            throw new NullReferenceException("No start time could be parsed.");
        }

        if (refrigerationTime == null)
        {
            throw new NullReferenceException("No Refridgeration Time found.");
        }

        DateTime start = (DateTime) startTime;
        DateTime fridge = (DateTime) refrigerationTime;

        // Ensure that the refrigeration time is later than the start time
        if (DateTime.Compare(start, fridge) > 0)
        {
            throw new RefridgerationTimeException("Refridgeration time cannot be earlier than the start time.");        
        }

        TimeSpan difference = (fridge - start);
        double hoursUntilFridge = difference.TotalHours;

        if (hoursUntilFridge > MAXIMUM_REFRIDGERATION_HOURS)
        {
            throw new RefridgerationTimeException($"Time between first kill and refridgeration cannot exceed {MAXIMUM_REFRIDGERATION_HOURS} hours (It is {difference.Hours}:{difference.Minutes})");
        }
    }

    [System.Serializable]
    public class RefridgerationTimeException : System.Exception
    {
        public RefridgerationTimeException() { }
        public RefridgerationTimeException(string message) : base(message) { }
        public RefridgerationTimeException(string message, System.Exception inner) : base(message, inner) { }
    }
}