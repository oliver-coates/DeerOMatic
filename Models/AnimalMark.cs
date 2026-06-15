using System;
using System.Numerics;

namespace Deer_o_matic.Models;

/// <summary>
/// Point representing a shot animal. Formed from placemark data from the kml
/// </summary>
public class AnimalMark
{
    public readonly string name;
    public readonly DateTime time;
    public readonly Vector2 coordinates;

    public AnimalMark(SharpKml.Dom.Placemark placemark)
    {
        name = placemark.Name;

        time = KmlProcessor.GetDateTimeForPlacemark(placemark.Time);

        SharpKml.Dom.Point point = (SharpKml.Dom.Point) placemark.Geometry;
        coordinates = new Vector2(
            (float) point.Coordinate.Latitude,
            (float) point.Coordinate.Longitude
        );
    }

    public override string ToString()
    {
        return $"Animal Mark. Name '{name}', Coords: '{coordinates.X:0.00f}, {coordinates.Y:0.00f}', Time: {time}'";
    }
}