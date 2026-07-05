using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Deer_o_matic.Models;
using Deer_o_matic.Services;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Providers;
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

    private IAreaProcessorService AreaProcessor; 

    [ObservableProperty]
    private Map _simpleMap;

    private Dictionary<string, ILayer[]> layerDictionary;

    public MapViewModel(IAreaProcessorService areaProcessor)
    {
        AreaProcessor = areaProcessor;

        _simpleMap = new Map
        {
            CRS = "EPSG:3857"
        };

        layerDictionary = new Dictionary<string, ILayer[]>();

        Console.WriteLine("Creating map!");
        SimpleMap.Layers.Add(OpenStreetMap.CreateTileLayer(), -1);   

        FileUploadViewModel.OnFlightDataAdded += LoadFlightData;
        FileUploadViewModel.OnFlightDataRemoved += RemoveFlightData;
        FileUploadViewModel.OnFlightDataCleared += ClearFlightData;
    }

    public async Task TestAddAreaAsync()
    {        
        await Task.Delay(100);  // Small delay to ensure map is ready

        Geometry[] areaGeometry = await AreaProcessor.GetAllWaroGeometry();

        IFeature[] features = new IFeature[areaGeometry.Length];
        for (int index = 0; index < areaGeometry.Length; index++)
        {
            features[index] = Mapsui.Nts.Extensions.GeometryExtensions.ToFeature(areaGeometry[index]);
            
        }

        // Create Layer:
        BaseLayer testAreaLayer = CreateZoneLayer("WARO", features);
        SimpleMap.Layers.AddOnTop(testAreaLayer, 0);

        // Zoom to the newly created layer:
        SimpleMap.Navigator.ZoomToBox(testAreaLayer.Extent, MBoxFit.Fit, 100);
    }

    public void LoadFlightData(FlightDataViewModel flightDataViewModel)
    {
        List<IFeature> features = new();

        foreach (AnimalMark mark in flightDataViewModel.marks)
        {
            var coords = mark.coordinates;

            features.Add(CreateMarker(coords.X, coords.Y, mark.displayName));
        }

        MemoryLayer pointLayer, textLayer;
        CreateLayers(flightDataViewModel, features, out pointLayer, out textLayer);

        SimpleMap.Layers.AddOnTop(pointLayer, 0);
        SimpleMap.Layers.AddOnTop(textLayer, 0);


        SimpleMap.Navigator.ZoomToBox(pointLayer.Extent, MBoxFit.Fit, 100);
    }

    private void RemoveFlightData(FlightDataViewModel flightData)
    {
        ILayer[] layers = layerDictionary[flightData.Name];

        SimpleMap.Layers.Remove(layers[0]);
        SimpleMap.Layers.Remove(layers[1]);

        layerDictionary.Remove(flightData.Name);
    }

    private void ClearFlightData()
    {
        string[] layerNames = layerDictionary.Keys.ToArray();

        foreach (string layerName in layerNames)
        {
            ILayer[] layers = layerDictionary[layerName];
            
            SimpleMap.Layers.Remove(layers[0]);
            SimpleMap.Layers.Remove(layers[1]);
            
            layerDictionary.Remove(layerName);
        }
    }


    private void CreateLayers(FlightDataViewModel flightDataViewModel, List<IFeature> features, out MemoryLayer pointLayer, out MemoryLayer textLayer)
    {
        Color color = LayerColours[layerDictionary.Count];
        
        pointLayer = CreatePointLayer(flightDataViewModel.Name + " (Points)", features, color);
        textLayer = CreateTextLayer(flightDataViewModel.Name + " (Text)", features);
        
        ILayer[] layers = new ILayer[2] { pointLayer, textLayer };
        layerDictionary.Add(flightDataViewModel.Name, layers);
    }

    private IFeature CreateMarker(double latitude, double longitude, string label)
    {
        PointFeature point = new PointFeature(SphericalMercator.FromLonLat(longitude, latitude));

        point["Name"] = label;

        return point;
    }

    private MemoryLayer CreatePointLayer(string name, List<IFeature> features, Color color)
    {
        BaseStyle style = new SymbolStyle
        {
            SymbolScale = 0.75,
            Fill = new Brush(color)
        };

        return new MemoryLayer
        {
            Name = name,
            Features = features,
            Style = style
        };
    }

    private static MemoryLayer CreateTextLayer(string name, List<IFeature> features)
    {
        BaseStyle style = new LabelStyle
        {
            LabelColumn = "Name",

            VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
            HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
            
            Font = new Mapsui.Styles.Font { FontFamily = "Arial", Size = 8 },
            ForeColor = Color.Black,
            BackColor = new Brush(Color.White),
            
        };

        return new MemoryLayer
        {
            Name = name,
            Features = features,
            Style = style
        };

    }
   
    private static BaseLayer CreateZoneLayer(string name, IEnumerable<IFeature> features)
    {
        // --- Styling:
        Brush brush = new Brush
        {
            Color = Color.Green,
            // Background = Color.Red,
            FillStyle = FillStyle.Solid
        };

        VectorStyle style = new()
        {
            Fill = brush,
            // Line = new Pen(Color.DarkRed, width:3),
            Outline = new Pen(Color.Black, width:1)
        };

        // --- Data Providers:
        MemoryProvider memoryProvider = new(features)
        {
            CRS = "EPSG:4326"
        };

        ProjectingProvider dataSource = new (memoryProvider)
        {
            CRS = "EPSG:3857"
        };
        
        // --- Layer:
        Layer layer = new ()
        {
            Name = name,
            DataSource = dataSource,
            Style = style,
            Opacity = 0.5,
            Enabled = true
        };

        // MemoryLayer layer =  new()
        // {            
        //     Name = name,
        //     Features = features,
        //     Style = style,
        //     Opacity = 0.4,
        //     Enabled = true
        // };

        return layer;
    }
}