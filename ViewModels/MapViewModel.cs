using System.Linq;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Deer_o_matic.Models;
using Mapsui;
using Mapsui.Features;
using Mapsui.Layers;
using Mapsui.Tiling;

namespace Deer_o_matic.ViewModels;

public partial class MapViewModel : ViewModelBase
{
    private Map? _simpleMap;
    public Map? SimpleMap
    {
        get => _simpleMap;
        set => SetProperty(ref _simpleMap, value);
    }



    public MapViewModel()
    {
        SimpleMap = new Map();
        SimpleMap.Layers.Add(OpenStreetMap.CreateTileLayer());   
    }

    public void LoadFlightData(FlightData flightData)
    {
        // var doc = XDocument.Parse(kmlContent);

        // foreach (AnimalMark mark in flightData.animalMarks)
        // {
        //     var coords = mark.coordinates;
        //     AddMarker(_map, coords.Y, coords.X, mark.name);
        // }

             
    }

    private void AddMarker(Map map, double latitude, double longitude, string label)
    {
        // var layer = new MemoryLayer { Name = "Placemarks" };
        
        // PointFeature feature = new PointFeature(latitude, longitude);

        // feature.Fields.Append()
    }   
}