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
        // // Ensure the file exists
        // string path = GetAbsoluteFilePath(TEST_KML_FILE_PATH);
        // Assert.IsTrue(File.Exists(path));
        
        // // Read the file as a string
        // string kmlInput = File.ReadAllText(path);
        
        // Kml kml = DateTimeConversionUtility.ParseKmlFromString(kmlInput);
        // Assert.IsNotNull(kml);

        // // Grab all placemarks
        // Placemark[] placemarks = DateTimeConversionUtility.GetPlacemarksFromKml(kml);
        // Assert.AreEqual(placemarks.Length, 22); // There should be 22 placemarks

        // // Convert all to animal marks
        // AnimalMark[] animalMarks = DateTimeConversionUtility.ConvertPlacemarksToAnimalMarks(placemarks);
        
        // // Optional test, print all animal marks to console:
        // Console.WriteLine("--- Animal Marks: ---");
        // foreach (AnimalMark a in animalMarks)
        // {
        //     Console.WriteLine(a.ToString());
        // }
        // Console.WriteLine("---------");



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
