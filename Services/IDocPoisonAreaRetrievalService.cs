using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Deer_o_matic.Models;
using NetTopologySuite.Geometries;

namespace Deer_o_matic.Services;

public interface IDocPoisonAreaRetrievalService : IInitialisable
{
    public Task<AreaData> GetPesticidesDataAsync();
}

public class DocPoisonAreaRetrievalService : IDocPoisonAreaRetrievalService
{
    public enum State
    {
        Waiting = 0, // No request has yet been made
        RequestInProgress = 1, // The request is in progress
        Success = 2, // All the data was loaded and parsed without error
        Error = 3 // A problem occured
    }

    private State state;
    public State CurrentState 
    { 
        get 
        {
            return state;
        }
        private set
        {
            state = value;
            OnStateChanged?.Invoke(value);
        } 
    }
    public event Action<State>? OnStateChanged;

    public const int NUM_RECORDS_REQUESTED_PER_PACKET = 25;

    private const string BaseUrl = 
        "https://services1.arcgis.com/3JjYDyG3oajxU6HO/arcgis/rest/services/Pesticides_HaveBeenLaid_HFV/FeatureServer/0/query"  ;

    private AreaData? _areaData;

    private readonly HttpClient _httpClient = new();
    private readonly INotificationService Notifications;


    readonly Dictionary<string, string> requestAllParameters = new Dictionary<string, string>
    {
        { "f",                  "geoJson"  },
        { "where",              "1=1"},
        { "outFields",          "Pesticide"},
        { "returnGeometry",     "false"},
        { "resultRecordCount",  "10000"}
    };

    readonly Dictionary<string, string> requestPacketParameters = new Dictionary<string, string>
    {
        { "f",                  "geoJson"  },
        { "where",              "1=1"},
        { "outFields",          "Pesticide"},
        { "returnGeometry",     "true"},
        { "resultOffset",       "-"}, // This will be set at each request
        { "resultRecordCount",  "-"} // This will be set at each request
    };


    public DocPoisonAreaRetrievalService(INotificationService notifications)
    {
        Notifications = notifications;

        CurrentState = State.Waiting;
    }



    public async Task Initialise()
    {
        await GetDocPoisonData();   
    }

    private async Task GetDocPoisonData()
    {
        CurrentState = State.RequestInProgress;

        try
        {
            // Figure out how many geometries we need to request
            int totalNumGeometriesToRequest = await GetNumGeometriesToRequest(); 

            List<Geometry> geometry = await RequestAllGeometry(requestPacketParameters, totalNumGeometriesToRequest);

            _areaData = new AreaData("Doc Poison Areas", [.. geometry]);
            CurrentState = State.Success;
        }
        catch (Exception ex)
        {
            await Notifications.ShowErrorAsync($"Error fetching poison area from DOC ArcGIS: {ex.Message}");
            CurrentState = State.Error;
        }
    }

    private async Task<List<Geometry>> RequestAllGeometry(Dictionary<string, string> parameters, int totalNumGeometriesToRequest)
    {
        List<Geometry> geometry = new();
        
        int totalNumgeometriesRequested = 0;
        while (totalNumgeometriesRequested < totalNumGeometriesToRequest)
        {
            // Figure out how many packets we need to request,
            int numGeoemetryToRequestThisPacket;
            if (totalNumgeometriesRequested + NUM_RECORDS_REQUESTED_PER_PACKET > totalNumGeometriesToRequest)
            {
                // If the num of geometries requested is going to be more than the total num,
                // shave it down so it sits within it
                numGeoemetryToRequestThisPacket = totalNumGeometriesToRequest - totalNumgeometriesRequested;
            }
            else
            {
                numGeoemetryToRequestThisPacket = NUM_RECORDS_REQUESTED_PER_PACKET;
            }
            
            // Request the data packet
            parameters["resultOffset"] = totalNumgeometriesRequested.ToString();
            parameters["resultRecordCount"] = numGeoemetryToRequestThisPacket.ToString();

            string data = await Query(parameters);

            // Parse the incoming geometry:
            geometry.AddRange(await AttemptParse(data));

            // Increment how many geometries we have requested
            totalNumgeometriesRequested += numGeoemetryToRequestThisPacket;
        }

        return geometry;
    }

    private async Task<string> Query(Dictionary<string, string> parameters)
    {
        var queryString = string.Join("&", parameters.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

        var url = $"{BaseUrl}?{queryString}";
        Console.WriteLine($"Attempting fetch from: {url}");

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        Console.WriteLine($"Recieved response: {response.StatusCode}");

        return await response.Content.ReadAsStringAsync();
    }

    private async Task<List<Geometry>> AttemptParse(string jsonData)
    {
        var json = JsonDocument.Parse(jsonData);
    
        JsonElement features = json.RootElement.GetProperty("features");
    
        List<Geometry> geometries = new();

        foreach (JsonElement feature in features.EnumerateArray())
        {
            var geometry = feature.GetProperty("geometry");
            var properties = feature.GetProperty("properties");

            if (ConvertJsonElementToGeometry(geometry) is Geometry outGeo && outGeo != null)
            {
                geometries.Add(outGeo);
            }
        }

        return geometries;
    }

    private Geometry? ConvertJsonElementToGeometry(JsonElement geometry)
    {
        var type = geometry.GetProperty("type").GetString();
        var coordinates = geometry.GetProperty("coordinates");

        switch (type)
        {
            case "MultiPolygon":
                return ConvertMutiPolygon(coordinates);
            
            case "Polygon":
                return ConvertPolygon(coordinates[0]);

            default:
                Console.WriteLine ($"Warning: Encountered unknown type '{type}' when converting json element to geometry.");
                return null;
        }
    }

    private static MultiPolygon ConvertMutiPolygon(JsonElement jsonGeometryData)
    {
        List<Polygon> Polygons = new();
        
        foreach (var polygonCoordinates in jsonGeometryData.EnumerateArray())
        {
            Polygons.Add(ConvertPolygon(polygonCoordinates[0]));
        }
    
        return new MultiPolygon([.. Polygons]);
    }

    public static Polygon ConvertPolygon(JsonElement jsonGeometryData)
    {
        List<Coordinate> ringCoordinates = new ();

        foreach (var polygonCoordinate in jsonGeometryData.EnumerateArray())
        {
            var lon = polygonCoordinate[0].GetDouble();
            var lat = polygonCoordinate[1].GetDouble();

            ringCoordinates.Add(new Coordinate(lon, lat));    
        }
        
        return new (new LinearRing([.. ringCoordinates]));
    }

    public async Task<AreaData> GetPesticidesDataAsync()
    {
        if (_areaData == null)
        {
            throw new NullReferenceException("Area data is null.");
        }

        return _areaData;
    }

    private async Task<int> GetNumGeometriesToRequest()
    {
        string recievedData = await Query(requestAllParameters); 

        return GetElementCount(recievedData);
    }

    private int GetElementCount(string jsonData)
    {
        var json = JsonDocument.Parse(jsonData);
    
        JsonElement features = json.RootElement.GetProperty("features");

        return features.GetArrayLength();
    }
}