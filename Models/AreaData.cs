using SharpKml.Dom;

namespace Deer_o_matic.Models;

/// <summary>
/// Represents an area of land o
/// </summary>
public class AreaData
{
    public readonly string name;
    public readonly Geometry[] geometries;

    public AreaData(string name, Geometry[] geometries)
    {
        this.name = name;
        this.geometries = geometries;
    }
}