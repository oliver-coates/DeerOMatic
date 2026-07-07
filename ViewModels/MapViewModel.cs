using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using SharpKml.Dom;

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
    /// Dictionary relating names to the each ay.
    /// </summary>
    private Dictionary<IDisplayableOnMap, ILayer[]> layerDictionary;

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
        SimpleMap.Layers.Add(OpenStreetMap.CreateTileLayer(), MapLayerTypes.BACKGROUND_LAYER_INT);   
        
        // Subscribe to methods for adding and removing flight & area data :
        FileUploadViewModel.OnFlightDataChanged += FlightDataChanged;
        AreaData.CollectionChanged += AreaDataChanged;

        // Command creation:
        PickFilesCommand = new AsyncRelayCommand(PickFileAsnyc);
    }


    #region Flight & Area Data Changes Response Methods
    private void FlightDataChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems == null)
                {
                    return;
                }
                DisplayFlightData(e.NewItems.OfType<FlightDataViewModel>());
                break;
            
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems == null)
                {
                    return;
                }
                RemoveFlightData(e.OldItems.OfType<FlightDataViewModel>());
                break;
            
            case NotifyCollectionChangedAction.Reset:
                ClearAllFlightData();
                break;
        }

    }

    public void DisplayFlightData(IEnumerable<FlightDataViewModel> flightDataViewModels)
    {
        foreach (FlightDataViewModel flightDataViewModel in flightDataViewModels)
        {
            // Collect all the features from this layer:
            List<IFeature> features = new();

            foreach (AnimalMark mark in flightDataViewModel.marks)
            {
                var coords = mark.coordinates;
                features.Add(CreateMarker(coords.X, coords.Y, mark.displayName));
            }

            // Create the two layers that the animal marks are to be displayed on
            MemoryLayer pointLayer, textLayer;
            CreateLayers(flightDataViewModel, features, out pointLayer, out textLayer);

            // Add these layers
            SimpleMap.Layers.AddOnTop(pointLayer, MapLayerTypes.PLACEMARKS_LAYER_INT);
            SimpleMap.Layers.AddOnTop(textLayer, MapLayerTypes.PLACEMARKS_LAYER_INT);

            // Zoom to the newly created layers
            SimpleMap.Navigator.ZoomToBox(pointLayer.Extent, MBoxFit.Fit, 100);
        }

      
    }

    private void RemoveFlightData(IEnumerable<FlightDataViewModel> flightDataToRemove)
    {
        foreach (FlightDataViewModel flightDataViewModel in flightDataToRemove)
        {
            ILayer[] layers = layerDictionary[flightDataViewModel];

            SimpleMap.Layers.Remove(layers[0]);
            SimpleMap.Layers.Remove(layers[1]);

            layerDictionary.Remove(flightDataViewModel);
        }
    }

    private void ClearAllFlightData()
    {
        IEnumerable<IDisplayableOnMap> keys = layerDictionary.Keys;

        foreach (IDisplayableOnMap mapObject in keys)
        {
            if (mapObject is FlightDataViewModel)
            {
                ILayer[] layers = layerDictionary[mapObject];

                SimpleMap.Layers.Remove(layers[0]);
                SimpleMap.Layers.Remove(layers[1]);

                layerDictionary.Remove(mapObject);
            }            
        }
    }
    
    private void AreaDataChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems == null)
                {
                    return;
                }
                DisplayAreaData(e.NewItems.OfType<AreaDataViewModel>());
                break;
            
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems == null)
                {
                    return;
                }
                RemoveAreaData(e.OldItems.OfType<AreaDataViewModel>());
                break;
        }

    }

    private void DisplayAreaData(IEnumerable<AreaDataViewModel> toDisplay)
    {
        foreach (AreaDataViewModel areaData in toDisplay)
        {
            // Add all geometry from the area data into an array of features:
            int numGeometry = areaData.geometry.Length;
            IFeature[] features = new IFeature[numGeometry];
            
            for (int index = 0; index < areaData.geometry.Length; index++)
            {
                features[index] = Mapsui.Nts.Extensions.GeometryExtensions.ToFeature(areaData.geometry[index]);
            }

            // Create Layer:
            BaseLayer layer = MapLayerTypes.CreateZoneLayer(areaData.Name, features);
            SimpleMap.Layers.AddOnTop(layer, MapLayerTypes.AREAS_LAYER_INT);

            // Zoom to the newly created layer:
            SimpleMap.Navigator.ZoomToBox(layer.Extent, MBoxFit.Fit, 100);

            // Register the newly created layer within the dictionary:
            layerDictionary.Add(areaData, [layer]);
        }
    }
    
    private void RemoveAreaData(IEnumerable<AreaDataViewModel> toRemove)
    {
        foreach (AreaDataViewModel areaDataViewModel in toRemove)
        {
            ILayer layerToRemove = layerDictionary[areaDataViewModel][0];
            
            SimpleMap.Layers.Remove(layerToRemove);
            layerDictionary.Remove(areaDataViewModel);
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
        layerDictionary.Add(flightDataViewModel, layers);
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