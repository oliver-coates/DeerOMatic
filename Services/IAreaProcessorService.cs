using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Deer_o_matic.Models;
using Mapsui.Projections;
using NetTopologySuite.Geometries;

namespace Deer_o_matic.Services;

public interface IAreaProcessorService
{
    public Task<Polygon> GetTestArea();
}


public class AreaProcessorService : IAreaProcessorService
{
    private readonly IKmlProcessor KmlProcessor;

    public AreaProcessorService(IKmlProcessor kmlProcessor)
    {
        KmlProcessor = kmlProcessor;
    }

    public async Task<Polygon> GetTestArea()
    {
        // TODO: 
        // Figure out how to get it into the nts Geometry Polygon object        
        
        // string contents = await GetAreaContents();
        
        // AreaData[] areaData = await KmlProcessor.ReadAreasFromKmz(contents);

        // List<Coordinate> points = new List<Coordinate>
        // {
        //     new Coordinate(Mercator.FromLonLat(-42.62638795276321, 171.39375259816507)),
        //     new Coordinate(Mercator.FromLonLat(-42.58040772904203, 171.44898314766252)),
        //     new Coordinate(Mercator.FromLonLat(-42.596660262604665, 171.49482676408988)),
        //     new Coordinate(Mercator.FromLonLat(-42.62638795276321, 171.39375259816507))
        // };

        var geoFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory();

        Polygon polygon = geoFactory.CreatePolygon(new[] {
            new Coordinate(SphericalMercator.FromLonLat(169.0, -43.0)),
            new Coordinate(SphericalMercator.FromLonLat(170.0, -43.0)),
            new Coordinate(SphericalMercator.FromLonLat(170.0, -44.0)),
            new Coordinate(SphericalMercator.FromLonLat(169.0, -44.0)),
            new Coordinate(SphericalMercator.FromLonLat(169.0, -43.0))
        });

        return polygon;
    }

    private static async Task<string> GetAreaContents()
    {
        // Assembly and resource path determine where the resource is stored:
        var assembly = typeof(IAreaProcessorService).Assembly;
        var resourcePath = "Deer-o-matic.Assets.Areas.WARO_Areas.kmz";

        // Create a resourceStream which pulls the .kmz file out of memory
        using Stream resourceStream = assembly.GetManifestResourceStream(resourcePath)
            ?? throw new FileNotFoundException($"WARO Areas not found! at {resourcePath}"); 

        // Memory stream is created, data is copied into it and then it us used to create a zip archive.
        using MemoryStream memoryStream = new();
        resourceStream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        using var zip = new ZipArchive(memoryStream, ZipArchiveMode.Read);

        // Find the KML entry within the zip
        var kmlEntry = zip.Entries.FirstOrDefault(e => e.Name.EndsWith(".kml", System.StringComparison.OrdinalIgnoreCase));
        if (kmlEntry == null)
        {
            throw new FileNotFoundException(".kml file could not be found within WARO_Areas.kmz");
        }

        // Finally, the unzipped kml entry is read
        using Stream stream = await kmlEntry.OpenAsync();
        using StreamReader reader = new(stream);

        return await reader.ReadToEndAsync();
    }
}