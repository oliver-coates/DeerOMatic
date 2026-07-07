using SharpKml.Base;
using NetTopologySuite.Geometries;
using Mapsui.Styles;

namespace Deer_o_matic.Models;

/// <summary>
/// Represents a loaded file containing geometry
/// </summary>
public class AreaData : IColourable
{
    public readonly string name;
    public Color color;
    public readonly Geometry[] geometries;



    public AreaData(string name, Geometry[] geometry, Color? color = null)
    {
        this.name = name;
        this.geometries = geometry;

        if (color == null)
        {
            this.color = MapLayerTypes.RequestAreaColor(this);            
        }
        else
        {
            this.color = (Color) color;
        }
    }

    ~AreaData()
    {    
        MapLayerTypes.ReleaseAreaColor(this);
    }


    
}