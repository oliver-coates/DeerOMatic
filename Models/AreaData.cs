using SharpKml.Base;
using SharpKml.Dom;

namespace Deer_o_matic.Models;

/// <summary>
/// Represents an area of land o
/// </summary>
public class AreaData
{
    public readonly string name;
    public readonly string geometryXml;

    public AreaData(string name, Geometry geometry)
    {
        this.name = name;

        Serializer serializer = new();    
        serializer.Serialize(geometry);
        this.geometryXml = serializer.Xml;
    }
}