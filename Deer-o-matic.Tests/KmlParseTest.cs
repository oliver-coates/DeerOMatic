using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Deer_o_matic.Models;
using NUnit.Framework;
using SharpKml.Base;
using SharpKml.Dom;

namespace Deer_o_matic.Tests;

[TestFixture]
public class KmlParseTest
{
    const string TEST_KML_FILE_PATH = "deer-o-matic/Assets/TestData/test.kml";
    const string TEST_KMZ_FILE_PATH = "deer-o-matic/Assets/TestData/test.kmz";

    [SetUp]
    public void Setup()
    {
        // Validate test files exist
    }

    [Test]
    public void TestParsing()
    {
        // Ensure the file exists
        string path = GetAbsoluteFilePath(TEST_KML_FILE_PATH);
        Assert.IsTrue(File.Exists(path));
        
        // Read the file as a string
        string kmlInput = File.ReadAllText(path);
        
        // Parse the string into a parser object
        Parser parser = new Parser();
        parser.ParseString(kmlInput, false);

        // Get the parsed string as a kml object from the root of the parser
        Kml kml = (Kml) parser.Root;
        Assert.IsNotNull(kml);

        // The 'feature' of the KML can be cast into the document
        Document doc = (Document) kml.Feature;
        Assert.IsNotNull(doc);

        // Pull all the folders from the document out and assemble them into a dictionary by name
        Dictionary<string, Folder> rootFolders = KmlProcessor.GetSubFolders(doc);
        Assert.AreEqual(rootFolders.Count, 4); // There should be 4 folders

        // Grab the GLUI folder from within the Markers folder
        Folder markerFolder = rootFolders["Markers"];
        Dictionary<string, Folder> markerSubFolders = KmlProcessor.GetSubFolders(markerFolder);
        Assert.AreEqual(markerSubFolders.Count, 3); // There should be 3 subfolders

        Folder gluiFolder = markerSubFolders["Glui"];
        Assert.NotNull(gluiFolder);

        // Grab all placemarks
        Placemark[] placemarks = KmlProcessor.GetPlacemarks(gluiFolder);
        Assert.AreEqual(placemarks.Count(), 22); // There should be 22 placemarks

        

        Assert.Pass();
    }

    private string GetAbsoluteFilePath(string pathFromRoot)
    {
        string root = System.AppDomain.CurrentDomain.BaseDirectory;
        root = root.Split("deer-o-matic")[0];

        string path = root;

        foreach (string dir in pathFromRoot.Split('/'))
        {
            path = Path.Combine(path, dir);
        }

        return path;
    }
}
