using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Deer_o_matic.Models;
using Deer_o_matic.ViewModels;
using NetTopologySuite.Algorithm;

namespace Deer_o_matic.Services;

public interface IPoisonAreaManagerService : IInitialisable
{
    public Task AddPoisonArea(PickedFile toAdd);

    public Task RemovePoisonArea(AreaDataViewModel toRemove);

    public Task<AreaDataViewModel[]> GetAllPoisonAreas();

    public Action<object?, NotifyCollectionChangedEventArgs>? OnPoisonAreasChanged {get; set;}
}

public class PoisonAreaManager : IPoisonAreaManagerService
{
    private readonly IKmlPersistenceService KmlSaveLoad;
    private readonly IAreaProcessorService KmlAreaProcessor;
    private readonly INotificationService Notifications;

    private ObservableCollection<AreaDataViewModel> _areaData;

    private static Action<object?, NotifyCollectionChangedEventArgs>? _OnPoisonAreasChanged; 
    public Action<object?, NotifyCollectionChangedEventArgs>? OnPoisonAreasChanged {
        get => _OnPoisonAreasChanged;
        set
        {
            _OnPoisonAreasChanged = value;
        }
    }
            

    public PoisonAreaManager(IKmlPersistenceService kmlSaveLoad, IAreaProcessorService kmlAreaProcessor, INotificationService notifications)
    {
        Notifications = notifications;
        KmlSaveLoad = kmlSaveLoad;
        KmlAreaProcessor = kmlAreaProcessor;
        _areaData = [];
    }

    #region Initialisation

    public async Task Initialise()
    {
        PickedFile[] files = await KmlSaveLoad.GetAllKmlFiles("PoisonAreas");
        AreaDataViewModel[] data = new AreaDataViewModel[files.Length];

        for (int i = 0; i < files.Length; i++)
        {
            data[i] = await LoadAreaFile(files[i]); 
        }

        _areaData = new ObservableCollection<AreaDataViewModel>(data);
        _areaData.CollectionChanged += (s, e) => _OnPoisonAreasChanged?.Invoke(s, e);;
    }

    private async Task<AreaDataViewModel> LoadAreaFile(PickedFile file)
    {
        AreaData data = await KmlAreaProcessor.ParseKmlAsync(file);
        
        return new AreaDataViewModel(data, file);   
    }

    #endregion

    public async Task AddPoisonArea(PickedFile toAdd)
    {
        if (EnsureFileNameIsUnique(toAdd.name) == false)
        {
            await Notifications.ShowErrorAsync($"Cannot upload multiple area files with the same name ('{toAdd.name}')");   
            return;
        }

        try
        {
            await KmlSaveLoad.SaveKmlFileAsync(toAdd, "PoisonAreas");            
        }
        catch (Exception e)
        {
            await Notifications.ShowErrorAsync($"Error when saving KML file:\n{e.Message}");
        }
    }

    public async Task<AreaDataViewModel[]> GetAllPoisonAreas()
    {
        return [.. _areaData];
    }

    public async Task RemovePoisonArea(AreaDataViewModel toRemove)
    {
        _areaData.Remove(toRemove);

        await KmlSaveLoad.RemoveKmlFileFromDisk(toRemove.File);

    }

    /// <summary>
    /// Ensures there is not other loaded area data with the same name.
    /// </summary>
    /// <returns>True if the name is unique.</returns>
    private bool EnsureFileNameIsUnique(string name)
    {
        string nameContent = name.Split('.').First().ToLower();
        foreach (AreaDataViewModel areaData in _areaData)
        {
            string areaDataNameContent = areaData.Name.Split('.').First().ToLower();
            if (areaDataNameContent == nameContent)
            {
                return false;
            }
        }

        return true;
    }

    
}