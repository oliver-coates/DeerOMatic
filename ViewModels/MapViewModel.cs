using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using AM = Avalonia.Media;
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
    private readonly IPoisonAreaManagerService PoisonAreaManager;
    private readonly IKmlPickerService KmlPicker;
    private readonly INotificationService Notifications;
    private readonly IDocPoisonAreaRetrievalService PoisonAreaRequester;

    private bool _hasRequestedFiles;

    [ObservableProperty]
    private Map _simpleMap;

    public ObservableCollection<AreaDataViewModel> AreaData { get; set; } = [];

    [ObservableProperty]
    private string _docPoisonDownloadStatus;
    [ObservableProperty]
    private AM.Brush _docPoisonDownloadStatusColor;

    /// <summary>
    /// Dictionary relating names to the each ay.
    /// </summary>
    private Dictionary<IDisplayableOnMap, ILayer[]> layerDictionary;

    public AsyncRelayCommand PickFilesCommand {get;}

    public MapViewModel(IPoisonAreaManagerService poisonAreaManager, IKmlPickerService kmlPicker, INotificationService notifications, IDocPoisonAreaRetrievalService poisonAreaRequester)
    {
        PoisonAreaManager = poisonAreaManager;
        KmlPicker = kmlPicker;
        Notifications = notifications;
        PoisonAreaRequester = poisonAreaRequester;

        layerDictionary = [];

        // Create the map with the OpenStreetMap layer as a base.
        _simpleMap = new Map
        {
            CRS = "EPSG:3857"
        };
        SimpleMap.Layers.Add(OpenStreetMap.CreateTileLayer(), MapLayerTypes.BACKGROUND_LAYER_INT);   
        
        // Subscribe to event for when flight data is added or removed :
        FileUploadViewModel.OnFlightDataChanged += FlightDataChanged;
        // Subscribe to event when poison data is retrieved
        PoisonAreaRequester.OnStateChanged += OnPoisonAreaRequesterStateChanged;

        // Command creation:
        PickFilesCommand = new AsyncRelayCommand(PickFileAsnyc);

        _hasRequestedFiles = false;

        DocPoisonDownloadStatus = "-";
        _docPoisonDownloadStatusColor = new AM.SolidColorBrush();
    }


    #region File Loading
    // Called everytime the user opens the map.
    // Essentially a lazy initialisation
    internal async Task CheckToLoadMapData()
    {
        await Task.Delay(TimeSpan.FromSeconds(0.25));

        if (_hasRequestedFiles == false)
        {
            _hasRequestedFiles = true;
            
            // Get all poison data files from memory:
            AreaDataViewModel[] data = await PoisonAreaManager.GetAllPoisonAreas();
            AreaData = new ObservableCollection<AreaDataViewModel>(data); 

            // Call a manual reload as the collection has been reset 
            OnPropertyChanged(nameof(AreaData));
            // Display the loaded data on the map
            DisplayAreaData(data);
            
            // Finally, ensure we are subscribed to the posion data's changed events,
            // these will be called as the user adds/removes poison data at runtime.
            PoisonAreaManager.SubscribeToPoisonDataNotificationEvents(OnPoisonAreasChanged);

        }

    }

    #endregion

    #region Flight Changes Response Methods

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
            MapLayerTypes.ReleaseAnimalMarkColor(flightDataViewModel);
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
                MapLayerTypes.ReleaseAnimalMarkColor((IColourable) mapObject);
            }            
        }
    }

    #endregion

    #region Area Data change response methods
    private void OnPoisonAreasChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems == null)
                {
                    return;
                }

                List<AreaDataViewModel> newAreaData = new();
                foreach (object o in e.NewItems)
                {
                    AreaDataViewModel m = (AreaDataViewModel) o; 
                    
                    newAreaData.Add(m);
                    AreaData.Add(m);
                }

                DisplayAreaData(newAreaData);
                break;
            
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems == null)
                {
                    return;
                }

                List<AreaDataViewModel> areaDataToRemove = new();
                foreach (object o in e.OldItems)
                {
                    AreaDataViewModel m = (AreaDataViewModel) o; 

                    areaDataToRemove.Add(m);
                    AreaData.Remove(m);
                }

                RemoveAreaData(areaDataToRemove);
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
            BaseLayer layer = MapLayerTypes.CreateZoneLayer(areaData.Name, features, areaData.MapColour);
            SimpleMap.Layers.AddOnTop(layer, MapLayerTypes.AREAS_LAYER_INT);

            // Register the newly created layer within the dictionary:
            layerDictionary.Add(areaData, [layer]);
        }
    }
    
    private void RemoveAreaData(IEnumerable<AreaDataViewModel> toRemove)
    {
        foreach (AreaDataViewModel areaDataViewModel in toRemove)
        {
            MapLayerTypes.ReleaseAreaColor(areaDataViewModel.Get());

            ILayer layerToRemove = layerDictionary[areaDataViewModel][0];
            
            SimpleMap.Layers.Remove(layerToRemove);
            
            layerDictionary.Remove(areaDataViewModel);

        }
    }

    #endregion

    #region Poison Data Requester methods

    private void OnPoisonAreaRequesterStateChanged(IDocPoisonAreaRetrievalService.Status status)
    {
        switch (status.code)
        {
            case IDocPoisonAreaRetrievalService.StateCode.Waiting:
                DocPoisonDownloadStatus = "Waiting for server...";
                DocPoisonDownloadStatusColor = new AM.SolidColorBrush(AM.Colors.Gray);
                break;
            
            case IDocPoisonAreaRetrievalService.StateCode.RequestInProgress:
                DocPoisonDownloadStatus = $"Downloading data ({status.numDownloaded}/{status.numToDownload})";
                DocPoisonDownloadStatusColor = new AM.SolidColorBrush(AM.Colors.Yellow);
                break;
            
            case IDocPoisonAreaRetrievalService.StateCode.Success:
                // Display the data:
                AreaData? gisPoison = PoisonAreaRequester.GetPesticidesData();
                if (gisPoison == null) 
                { 
                    Notifications.ShowErrorAsync("Recieved null poison data when getting DOC pesticides data after a success code.");
                    OnPoisonAreaRequesterStateChanged(new IDocPoisonAreaRetrievalService.Status() {code = IDocPoisonAreaRetrievalService.StateCode.Error});
                    return;
                }
                AreaDataViewModel gisPoisonVM = new(gisPoison);
                DisplayAreaData([gisPoisonVM]);    
                
                DocPoisonDownloadStatus = $"Success! ({status.numDownloaded} areas)";
                DocPoisonDownloadStatusColor = new AM.SolidColorBrush(AM.Colors.Green);
                break;

            case IDocPoisonAreaRetrievalService.StateCode.Error:
                DocPoisonDownloadStatus = "Error!";
                DocPoisonDownloadStatusColor = new AM.SolidColorBrush(AM.Colors.Red);
                break;
        }
    }

    #endregion

    [RelayCommand]
    public void DeleteAreaData(AreaDataViewModel toRemove)
    {
        PoisonAreaManager.RemovePoisonArea(toRemove);
    }

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
            await PoisonAreaManager.AddPoisonArea(file);
        }
    }


    private void CreateLayers(FlightDataViewModel flightDataViewModel, List<IFeature> features, out MemoryLayer pointLayer, out MemoryLayer textLayer)
    {
        Color layerColor = MapLayerTypes.RequestAnimalMarkColor(flightDataViewModel);

        pointLayer = MapLayerTypes.CreatePointLayer(flightDataViewModel.Name + " (Points)", features, layerColor);
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

}