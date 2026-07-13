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


    public async Task Initialise()
    {
        try
        {
            var parameters= new Dictionary<string, string>
            {
                { "f",                  "geoJson"  },
                { "where",              "1=1"},
                // { "where",              "(Pesticide IS NOT NULL) AND (CHAR_LENGTH(Pesticide)>0)"},
                { "outFields",          "Pesticide"},
                { "returnGeometry",     "true"},
                { "resultRecordCount",  "10"}
            };


            var queryString = string.Join("&", parameters.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

            var url = $"{BaseUrl}?{queryString}";
            Console.WriteLine($"Attempting fetch from: {url}");

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            Console.WriteLine($"Recieved response: {response.StatusCode}");
            
            string recievedData = await response.Content.ReadAsStringAsync();
            await AttemptParse(recievedData);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error fetching poison area from DOC ArcGIS: {ex.Message}");
        }
    }

    private async Task AttemptParse(string jsonData)
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

        _areaData = new AreaData("Doc Poison Areas", [.. geometries]);
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
}