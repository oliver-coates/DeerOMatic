using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Deer_o_matic.Models;
using HarfBuzzSharp;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using NetTopologySuite.Geometries;

namespace Deer_o_matic.ViewModels;

public partial class MapViewModel : ViewModelBase
{
    [ObservableProperty]
    private Map _simpleMap;



    public MapViewModel()
    {
        _simpleMap = new Map();

        SimpleMap.Layers.Add(OpenStreetMap.CreateTileLayer(), -1);   

        FileUploadViewModel.OnFlightDataAdded += LoadFlightData;
        
        // double latitude = -43.590527;
        // double longitude = 170.008469;

        // List<IFeature> testFeatures = new List<IFeature>()
        // {
        //     CreateMarker(latitude, longitude, "Manual!")
        // };

        // MemoryLayer testLayer = CreateAnimalLayer("Test Layer", testFeatures);

        // _simpleMap.Layers.AddOnTop(testLayer);

        // SimpleMap.Navigator.ZoomToBox(testLayer.Extent, MBoxFit.Fit, 100);

    }

    public void LoadFlightData(FlightData flightData)
    {
        List<IFeature> features = new();

        foreach (AnimalMark mark in flightData.animalMarks)
        {
            var coords = mark.coordinates;

            features.Add(CreateMarker(coords.X, coords.Y, mark.name));
        }

        MemoryLayer layer = CreateAnimalLayer(flightData.name, features);
            
        SimpleMap.Layers.AddOnTop(layer, 0); 

        SimpleMap.Navigator.ZoomToBox(layer.Extent, MBoxFit.Fit, 100);

    }   
    
    private IFeature CreateMarker(double latitude, double longitude, string label)
    {
        PointFeature point = new PointFeature(SphericalMercator.FromLonLat(longitude, latitude));
        
        Console.WriteLine($"Adding point at lat: {latitude}, long: {longitude}");

        return point;
    }

    private MemoryLayer CreateAnimalLayer(string name, List<IFeature> features)
    {
        return new MemoryLayer
        {
            Name = name,
            Features = features,
            Style = new SymbolStyle
            {
                SymbolScale = 1.0,
                Fill = new Brush(Color.Red)
            }
        };
    }
   
}