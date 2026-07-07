using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Deer_o_matic.Models;
using Deer_o_matic.Services;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;

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

    private readonly IAreaProcessorService AreaProcessor; 
    private readonly IKmlPickerService KmlPicker;

    [ObservableProperty]
    private Map _simpleMap;

    public ObservableCollection<AreaDataViewModel> AreaData {get; } = [];

    /// <summary>
    /// Dictionary relating names to the layer.
    /// </summary>
    private Dictionary<string, ILayer[]> layerDictionary;

    public AsyncRelayCommand PickFilesCommand {get;}

    public MapViewModel(IAreaProcessorService areaProcessor, IKmlPickerService kmlPicker)
    {
        AreaProcessor = areaProcessor;
        KmlPicker = kmlPicker;

        layerDictionary = [];

        // Create the map with the OpenStreetMap layer as a base.
        _simpleMap = new Map
        {
            CRS = "EPSG:3857"
        };
        SimpleMap.Layers.Add(OpenStreetMap.CreateTileLayer(), -1);   
        
        // Subscribe to methods for adding and removing flight data:
        FileUploadViewModel.OnFlightDataAdded += LoadFlightData;
        FileUploadViewModel.OnFlightDataRemoved += RemoveFlightData;
        FileUploadViewModel.OnFlightDataCleared += ClearFlightData;

        // Command creation:
        PickFilesCommand = new AsyncRelayCommand(PickFileAsnyc);
    }

    #region Flight Data Changes Response Methods
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
    #endregion

    private async Task PickFileAsnyc()
    {
        PickedFile[] kmlFiles = await KmlPicker.OpenFilesAsync();

        if (kmlFiles is null)
        {
            // No files were picked.
            return;
        }

        foreach (PickedFile file in kmlFiles)
        {
            AreaData data = await AreaProcessor.ParseKmlAsync(file);

            AreaData.Add(new AreaDataViewModel(data));        
        }
    }

    private void CreateLayers(FlightDataViewModel flightDataViewModel, List<IFeature> features, out MemoryLayer pointLayer, out MemoryLayer textLayer)
    {
        Color color = LayerColours[layerDictionary.Count];
        
        pointLayer = MapLayerTypes.CreatePointLayer(flightDataViewModel.Name + " (Points)", features, color);
        textLayer = MapLayerTypes.CreateTextLayer(flightDataViewModel.Name + " (Text)", features);
        
        ILayer[] layers = new ILayer[2] { pointLayer, textLayer };
        layerDictionary.Add(flightDataViewModel.Name, layers);
    }

    private IFeature CreateMarker(double latitude, double longitude, string label)
    {
        PointFeature point = new PointFeature(SphericalMercator.FromLonLat(longitude, latitude));

        point["Name"] = label;

        return point;
    }


    #region Testing methods
    /// <summary>
    /// Async method testing adding all the WARO areas into the map.
    /// </summary>
    // public async Task TestAddWaroAreasAsync()
    // {        
    //     await Task.Delay(100);  // Small delay to ensure map is ready

    //     Geometry[] areaGeometry = await AreaProcessor.GetAllWaroGeometry();

    //     IFeature[] features = new IFeature[areaGeometry.Length];
    //     for (int index = 0; index < areaGeometry.Length; index++)
    //     {
    //         features[index] = Mapsui.Nts.Extensions.GeometryExtensions.ToFeature(areaGeometry[index]);
            
    //     }

    //     // Create Layer:
    //     BaseLayer testAreaLayer = MapLayerTypes.CreateZoneLayer("WARO", features);
    //     SimpleMap.Layers.AddOnTop(testAreaLayer, 0);

    //     // Zoom to the newly created layer:
    //     SimpleMap.Navigator.ZoomToBox(testAreaLayer.Extent, MBoxFit.Fit, 100);
    // }
    #endregion
}