using System;
using System.Collections.Generic;
using Deer_o_matic.Services;
using SharpKml.Base;
using SharpKml.Dom;

namespace Deer_o_matic.Models;

/// <summary>
/// Static class with methods to help process a kml file and turn it into a FlightData object
/// </summary>
public static class KmlProcessor
{
    /// <summary>
    /// Takes a contianer (such as a document and folder) and returns a dictionary mapping names to subfolders within the container.
    /// </summary>
    /// <param name="container"></param>
    /// <returns></returns>
    public static Dictionary<string, Folder> GetSubFolders(Container container)
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
    public static Placemark[] GetPlacemarksWithinContainer(Container container)
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
    public static AnimalMark[] ConvertPlacemarksToAnimalMarks(Placemark[] placemarks)
    {
        AnimalMark[] animalMarks = new AnimalMark[placemarks.Length];

        for (int placemarkIndex = 0; placemarkIndex < placemarks.Length; placemarkIndex++)
        {
            Placemark p = placemarks[placemarkIndex];
            
            AnimalMark a = new AnimalMark(p);

            animalMarks[placemarkIndex] = a;
        }

        return animalMarks;
    }

    /// <summary>
    /// Takes in a timePrimitive object and converts it to a datetime in NZST.
    /// TODO: Test this method rigerously to ensure that it is concerting time & date properly, and check daylight savings time aswell
    /// </summary>
    public static DateTime GetDateTimeForPlacemark(TimePrimitive timePrimitive)
    {
        Timestamp timeStamp = (SharpKml.Dom.Timestamp) timePrimitive;   
        
        if (timeStamp.When == null)
        {
            throw new NullReferenceException();
        }
        
        DateTime utcTime = (DateTime) timeStamp.When;

        // Convert UTC time to nzst
        TimeZoneInfo nzst = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time"); 

        return TimeZoneInfo.ConvertTime(utcTime, nzst);
    }

    /// <summary>
    /// Creates a KML object from a provided fileOutput. Accepts .kml and .kmz files.
    /// </summary>
    public static Kml ParseKmlFromFile(FileOutput fileOutput)
    {
        if (fileOutput.extension == ".kml")
        {
            return ParseKmlFromString(fileOutput.content);
        }
        else if (fileOutput.extension == ".kmz")
        {
            // TODO: Implement unzipping
            throw new NotImplementedException();
        }
        else
        {
            throw new NullReferenceException();
        }
    }

    /// <summary>
    /// Creates a KML object from a string
    /// </summary>
    public static Kml ParseKmlFromString(string kmlInput)
    {
        // Parse the string into a parser object
        Parser parser = new Parser();
        parser.ParseString(kmlInput, false);

        return (Kml) parser.Root;
    }

    /// <summary>
    /// Pulls a list of placemarks from a provided kml file.
    /// </summary>
    public static Placemark[] GetPlacemarksFromKml(Kml kml)
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
    /// Creates a flight data object from a provided file output object.
    /// </summary>
    public static FlightData CreateFlightDataFromFile(FileOutput file)
    {
        Kml kml = ParseKmlFromFile(file);

        Placemark[] placemarks = GetPlacemarksFromKml(kml);

        AnimalMark[] animalMarks = ConvertPlacemarksToAnimalMarks(placemarks);

        FlightData flightData = new (file.name, file.path, animalMarks);

        return flightData;
    }
}