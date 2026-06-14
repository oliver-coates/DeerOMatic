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

    public AnimalMark(string input)
    {
        // TODO: Setup placemark creation from input data (need to setup kml parsing first)

        name = $"Unnamed placemark ({input})";
        time = DateTime.UtcNow;
        coordinates = new Vector2(0.0f, 0.0f);
    }
}