using System;
using System.Collections.Generic;
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
    public static Placemark[] GetPlacemarks(Container container)
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

}