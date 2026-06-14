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

}