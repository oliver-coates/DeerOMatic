
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BruTile.Wmts.Generated;
using Deer_o_matic.Models;
using NetTopologySuite.Algorithm;
using SharpKml.Base;
using SharpKml.Dom;

namespace Deer_o_matic.Services;

public interface IKmlProcessor
{
    public Task<FlightData> CreateFlightData(PickedFile file);

    public Task<AreaData[]> ReadAreasFromKmz(string content);

}

public class KmlProcessor : IKmlProcessor
{

    #region Public Methods

    /// <summary>
    /// Creates a flight data object from a provided file output object.
    /// </summary>
    public async Task<FlightData> CreateFlightData(PickedFile file)
    {
        Kml kml = await Parse(file);

        Placemark[] placemarks = GetPlacemarksFromKml(kml);

        AnimalMark[] animalMarks = ConvertPlacemarksToAnimalMarks(placemarks);

        FlightData flightData = new (file.name, file.path, null, animalMarks);

        return flightData;
    }

    public async Task<AreaData[]> ReadAreasFromKmz(string content)
    {
        Kml kml = await Parse(content);

        Document doc = (Document) kml.Feature;
        
        // The root folder contains every WARO zone:
        Folder rootFolder = (Folder) doc.Features.ElementAt(0); 

        
        List<AreaData> areaDatas = new ();
        foreach (Feature placemark in rootFolder.Features)
        {

            Placemark waroArea = (Placemark) placemark;

            AreaData data;
            if (waroArea.Geometry is MultipleGeometry multipleGeometry)
            {
                data = new AreaData(placemark.Name, [.. multipleGeometry.Geometry]);
            }
            else if (waroArea.Geometry is Polygon polygon)
            {
                Geometry[] g = new Geometry[1]
                {
                    polygon
                };

                data = new AreaData(placemark.Name, g);
            }
            else
            {
                throw new NullReferenceException($"Unxpected geometry type for '{placemark.Name}' of '{placemark.GetType()}' while parsing area.");
            }
        
            areaDatas.Add(data);
        }

        return [.. areaDatas];
    }

    #endregion

    #region Internal Methods
    
    /// <summary>
    /// Creates a KML object from a string
    /// </summary>
    private static async Task<Kml> Parse(string stringInput)
    {
        // Parse the string into a parser object
        Parser parser = new Parser();
        parser.ParseString(stringInput, false);

        return (Kml) parser.Root;
    }

    /// <summary>
    /// Creates a KML object from a provided fileOutput. Accepts .kml and .kmz files.
    /// </summary>
    private static async Task<Kml> Parse(PickedFile file)
    {
        if (file.extension is ".kml" or ".kmz")
        {
            return await Parse(file.content);        
        }
        else
        {
            throw new NullReferenceException();            
        }
    }

    
    /// <summary>
    /// Pulls a list of placemarks from a provided kml file.
    /// </summary>
    private static Placemark[] GetPlacemarksFromKml(Kml kml)
    {
        // The 'feature' of the KML can be cast into the document
        Document doc = (Document) kml.Feature;

        // Pull all the folders from the document out and assemble them into a dictionary by name
        Dictionary<string, Folder> rootFolders = GetSubFolders(doc);

        // Grab the GLUI folder from within the Markers folder
        Folder markerFolder = rootFolders["Markers"];
        Dictionary<string, Folder> markerSubFolders = GetSubFolders(markerFolder);

        Folder gluiFolder = markerSubFolders["Glui"];

        // Grab all placemarks
        Placemark[] placemarks = GetPlacemarksWithinContainer(gluiFolder);

        return placemarks;
    }

    /// <summary>
    /// Takes a contianer (such as a document and folder) and returns a dictionary mapping names to subfolders within the container.
    /// </summary>
    /// <param name="container"></param>
    /// <returns></returns>
    private static Dictionary<string, Folder> GetSubFolders(Container container)
    {
        Dictionary<string, Folder> folders = new();
        
        foreach (Feature feature in container.Features)
        {    
            if (feature is Folder)
            {
                folders.Add(feature.Name, (Folder)feature);            
            }
        }

        return folders;
    }

    /// <summary>
    /// Takes a container and retreieves all placemarks that are within this container.
    /// </summary>
    /// <param name="container"></param>
    /// <returns></returns>
    private static Placemark[] GetPlacemarksWithinContainer(Container container)
    {
        List<Placemark> placemarks = new ();

        foreach (Feature f in container.Features)
        {
            if (f is Placemark)
            {
                Placemark placemark = (Placemark) f;

                placemarks.Add(placemark);
            }
        }

        return placemarks.ToArray();
    }

    /// <summary>
    /// Converts an array of placemarks into animal mark objects
    /// </summary>
    /// <param name="placemarks"></param>
    /// <returns></returns>
    private static AnimalMark[] ConvertPlacemarksToAnimalMarks(Placemark[] placemarks)
    {
        AnimalMark[] animalMarks = new AnimalMark[placemarks.Length];

        for (int placemarkIndex = 0; placemarkIndex < placemarks.Length; placemarkIndex++)
        {
            Placemark p = placemarks[placemarkIndex];
            
            AnimalMark a = new AnimalMark(p);
            a.displayName = (placemarkIndex+1).ToString();

            animalMarks[placemarkIndex] = a;
        }

        return animalMarks;
    }

    #endregion

}