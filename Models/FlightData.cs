

namespace Deer_o_matic.Models;

/// <summary>
/// Represents all the data related to a flight.
/// Created from a .kml file.
/// </summary>
public class FlightData
{
    public readonly string name;
    public readonly string path;
    public readonly AnimalMark[] animalMarks;



    public FlightData(string name, string path, AnimalMark[] placemarks)
    {
        this.name = name;
        this.path = path;
        this.animalMarks = placemarks;
    }

}