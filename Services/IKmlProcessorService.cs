
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deer_o_matic.Models;
using SharpKml.Base;
using SharpKml.Dom;

namespace Deer_o_matic.Services;

public interface IKmlProcessor
{
    public Task<FlightData> ParseFlightDataFromKmlAsync(PickedFile file);

    public Task<Placemark[]> ParseAreaDataFromKmlAsync(PickedFile file);
}

public class KmlProcessor : IKmlProcessor
{

    #region Public Methods

    /// <summary>
    /// Creates a flight data object from a provided file.
    /// </summary>
    public async Task<FlightData> ParseFlightDataFromKmlAsync(PickedFile file)
    {
        Kml kml = await Parse(file);

        Placemark[] placemarks = GetPlacemarksFromKml(kml);

        AnimalMark[] animalMarks = ConvertPlacemarksToAnimalMarks(placemarks);

        FlightData flightData = new (file.name, file.pathUri.ToString(), null, animalMarks);

        return flightData;
    }

    /// <summary>
    /// Takes a KML file and pulls all the placemarks.
    /// This function assumes that all placemarks within the root folder are areas.
    /// </summary>
    public async Task<Placemark[]> ParseAreaDataFromKmlAsync(PickedFile file)
    {
        Kml kml = await Parse(file.content);

        Document doc = (Document) kml.Feature;
        
        // NOTE: The following assumes a schema for every passed KML file, which will probably not be the case - so expect this to break.

        // The root folder contains every zone:
        Folder rootFolder = (Folder) doc.Features.ElementAt(0); 

        List<Placemark> areaPlacemarks = [];
        foreach (Feature feature in rootFolder.Features)
        {
            if (feature is Placemark area)
            {
                areaPlacemarks.Add(area);            
            }
            else
            {
                // NOTE: Consider unexpected type handling here?
            }
        }

        return [.. areaPlacemarks];
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
            throw new NotImplementedException($"Parsing of file extension '{file.extension}' is not handled.");
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