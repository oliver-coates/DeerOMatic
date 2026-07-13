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


    public readonly HttpClient _httpClient = new();
    private const string BaseUrl = 
        "https://services1.arcgis.com/3JjYDyG3oajxU6HO/arcgis/rest/services/Pesticides_HaveBeenLaid_HFV/FeatureServer/0/query"  ;

    private AreaData? _areaData;


    readonly Dictionary<string, string> requestAllParameters = new Dictionary<string, string>
    {
        { "f",                  "geoJson"  },
        { "where",              "1=1"},
        { "outFields",          "Pesticide"},
        { "returnGeometry",     "false"},
        { "resultRecordCount",  "10000"}
    };


    public async Task Initialise()
    {
        try
        {
            // Figure out how many geometries we need to request
            int totalNumGeometriesToRequest = await GetNumGeometriesToRequest(); 

            var parameters= new Dictionary<string, string>
            {
                { "f",                  "geoJson"  },
                { "where",              "1=1"},
                { "outFields",          "Pesticide"},
                { "returnGeometry",     "true"},
                { "resultOffset",       "0"},
                { "resultRecordCount",  "50"}
            };

            List<Geometry> geometry = new();

            int geometriesRequested = 0;
            while (geometriesRequested < totalNumGeometriesToRequest)
            {
                int numGeoemetryToRequestThisPacket = 50;
                if (geometriesRequested + numGeoemetryToRequestThisPacket > totalNumGeometriesToRequest)
                {
                    numGeoemetryToRequestThisPacket = totalNumGeometriesToRequest - geometriesRequested;
                }
                
                Console.WriteLine($"Requesting {numGeoemetryToRequestThisPacket}");

                // Request & parse here...
                parameters["resultOffset"] = geometriesRequested.ToString();
                string data = await Query(parameters);
                Console.WriteLine("Recieved!");

                geometry.AddRange(await AttemptParse(data));

                geometriesRequested += numGeoemetryToRequestThisPacket;
            }

            // await AttemptParse(recievedData);

            _areaData = new AreaData("Doc Poison Areas", [.. geometry]);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error fetching poison area from DOC ArcGIS: {ex.Message}");
        }
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