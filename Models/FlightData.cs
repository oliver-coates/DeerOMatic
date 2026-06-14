

namespace Deer_o_matic.Models;

/// <summary>
/// Represents all the data related to a flight.
/// Created from a .kml file.
/// </summary>
public class FlightData
{
    public readonly string name;
    public readonly Placemark[] placemarks;



    public FlightData(string name, Placemark[] placemarks)
    {
        this.name = name;
        this.placemarks = placemarks;
    }

}