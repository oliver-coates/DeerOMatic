using SharpKml.Base;
using NetTopologySuite.Geometries;

namespace Deer_o_matic.Models;

/// <summary>
/// Represents a loaded file containing geometry
/// </summary>
public class AreaData
{
    public readonly string name;
    public readonly Geometry[] geometries;

    public AreaData(string name, Geometry[] geometry)
    {
        this.name = name;
        this.geometries = geometry;        
    }


    
}