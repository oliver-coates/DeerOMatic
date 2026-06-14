

namespace Deer_o_matic.Models;

/// <summary>
/// Represents all the data related to a flight.
/// Created from a .kml file.
/// </summary>
public class FlightData
{
    public readonly string name;
    public readonly AnimalMark[] placemarks;



    public FlightData(string name, AnimalMark[] placemarks)
    {
        this.name = name;
        this.placemarks = placemarks;
    }

}