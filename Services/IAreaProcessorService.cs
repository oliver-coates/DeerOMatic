using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace Deer_o_matic.Services;

public interface IAreaProcessorService
{
    public Polygon GetArea();
}


public class AreaProcessorService : IAreaProcessorService
{
    public Polygon GetArea()
    {
        // TODO: 
        // Extract KMZ file from Assets/Areas/WARO_Areas.kmz
        // Figure out how to get it into the nts Geometry Polygon object
        
        List<Coordinate> points = new List<Coordinate>();

        // Points then become a linearRing which we can turn into a polygon
        LinearRing linearRing = new LinearRing([.. points]); // <-- [.. points] converts the list to an array
        return new Polygon(linearRing);
    }
}