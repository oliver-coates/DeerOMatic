using System;
using System.Collections.Generic;
using System.Linq;
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
    private static readonly Color[] LayerColours = {
        Color.IndianRed,
        Color.Azure,
        Color.Yellow,
        Color.ForestGreen,
        Color.White,
        Color.Aquamarine,
        Color.Beige,
        Color.BurlyWood,
        Color.Crimson};

    [ObservableProperty]
    private Map _simpleMap;

    private Dictionary<string, ILayer> layerDictionary;

    public MapViewModel()
    {
        _simpleMap = new Map();

        layerDictionary = new Dictionary<string, ILayer>();

        SimpleMap.Layers.Add(OpenStreetMap.CreateTileLayer(), -1);   

        FileUploadViewModel.OnFlightDataAdded += LoadFlightData;
        FileUploadViewModel.OnFlightDataRemoved += RemoveFlightData;
        FileUploadViewModel.OnFlightDataCleared += ClearFlightData;
    }

    public void LoadFlightData(FlightDataViewModel flightDataViewModel)
    {
        List<IFeature> features = new();

        foreach (AnimalMark mark in flightDataViewModel.marks)
        {
            var coords = mark.coordinates;

            features.Add(CreateMarker(coords.X, coords.Y, mark.name));
        }

        MemoryLayer layer = CreateAnimalLayer(flightDataViewModel.Name, features, SimpleMap.Layers.Count);
            
        SimpleMap.Layers.AddOnTop(layer, 0); 

        SimpleMap.Navigator.ZoomToBox(layer.Extent, MBoxFit.Fit, 100);
        
        layerDictionary.Add(flightDataViewModel.Name, layer);
    }   

    private void RemoveFlightData(FlightDataViewModel flightData)
    {
        ILayer layer = layerDictionary[flightData.Name];

        SimpleMap.Layers.Remove(layer);

        layerDictionary.Remove(flightData.Name);
    }

    private void ClearFlightData()
    {
        string[] layerNames = layerDictionary.Keys.ToArray();

        foreach (string layerName in layerNames)
        {
            ILayer layer = layerDictionary[layerName];
            SimpleMap.Layers.Remove(layer);
            layerDictionary.Remove(layerName);
        }
    }

    private IFeature CreateMarker(double latitude, double longitude, string label)
    {
        PointFeature point = new PointFeature(SphericalMercator.FromLonLat(longitude, latitude));
        
        return point;
    }

    private MemoryLayer CreateAnimalLayer(string name, List<IFeature> features, int layerIndex = 0)
    {
        return new MemoryLayer
        {
            Name = name,
            Features = features,
            Style = new SymbolStyle
            {
                SymbolScale = 0.5,
                Fill = new Brush(LayerColours[layerIndex-1]) // Subtracting one index as the base map layer exists
            }
        };
    }
   
}