using System.Collections.Generic;
using System.Threading.Tasks;
using Deer_o_matic.Models;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.KML;

namespace Deer_o_matic.Services;

public interface IAreaProcessorService
{
    public Task<AreaData> ParseKmlAsync(PickedFile file);
}


public class AreaProcessorService : IAreaProcessorService
{
    private readonly IKmlProcessor KmlProcessor;
    private readonly IKmlPickerService FilePicker;
    private readonly GeometryFactory _geoFactory;

    public AreaProcessorService(IKmlProcessor kmlProcessor, IKmlPickerService filePicker)
    {
        KmlProcessor = kmlProcessor;
        FilePicker = filePicker;

        _geoFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory();

    }

    #region Depreciated
    #if FALSE
    public async Task<Geometry> GetSampleWaroArea()
    {
        PickedFile waroFile = await GetWaroFile();   
        AreaData[] areaData = await KmlProcessor.ParseAreaDataFromKmlAsync(waroFile);

        AreaData sampleData = areaData[0];

        NetTopologySuite.IO.KML.KMLReader reader = new(["altitudeMode", "tesselate", "extrude"]);

        Geometry parsedGeometry = reader.Read(sampleData.geometryXml);

        return parsedGeometry;
    }

    public async Task<Geometry[]> GetAllWaroGeometry()
    {
        PickedFile waroFile = await GetWaroFile();   
        AreaData[] areaData = await KmlProcessor.ParseAreaDataFromKmlAsync(waroFile);
        Geometry[] outGeometry = new Geometry[areaData.Length];
        
        NetTopologySuite.IO.KML.KMLReader reader = new(["altitudeMode", "tesselate", "extrude"]);
        for (int index = 0; index < areaData.Length; index++)
        {
            outGeometry[index] = reader.Read(areaData[index].geometryXml);
        }

        return outGeometry;
    }
    

    private async Task<PickedFile> GetWaroFile()
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

        string content = await reader.ReadToEndAsync();
        
        return FilePicker.GenerateDummyFile("Waro Areas", content, ".kml", "not implemented");
    }
    #endif
    #endregion
    
    /// <summary>
    /// Parses a picked KML file into AreaData objects
    /// </summary>
    public async Task<AreaData> ParseKmlAsync(PickedFile file)
    {
        SharpKml.Dom.Placemark[] areaPlacemarks = await KmlProcessor.ParseAreaDataFromKmlAsync(file);
        
        List<Geometry> geo = new();

        // Create the serializer and reader objects we are going to need to convert SharpKML placemarks into NTS objects
        SharpKml.Base.Serializer serializer = new();
        KMLReader reader = new(["altitudeMode", "tesselate", "extrude"]);

        foreach (SharpKml.Dom.Placemark placemark in areaPlacemarks)
        {
            // Serialize the placemark into raw XML
            serializer.Serialize(placemark.Geometry);

            // Read the raw xml into a NTS geometry object
            Geometry geometry = reader.Read(serializer.Xml);

            geo.Add(geometry);
        }

        return new AreaData(file.name, [.. geo]);
    }

}