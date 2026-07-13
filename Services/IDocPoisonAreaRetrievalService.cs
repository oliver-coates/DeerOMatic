using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Deer_o_matic.Models;
using HarfBuzzSharp;
using NetTopologySuite.Geometries;

namespace Deer_o_matic.Services;

public interface IDocPoisonAreaRetrievalService : IInitialisable
{
    public Task GetPesticidesDataAsync();
}

public class DocPoisonAreaRetrievalService : IDocPoisonAreaRetrievalService
{

    public readonly HttpClient _httpClient = new();
    private const string BaseUrl = 
        "https://services1.arcgis.com/3JjYDyG3oajxU6HO/arcgis/rest/services/Pesticides_HaveBeenLaid_HFV/FeatureServer/0/query"  ;


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

        AreaData areaData = new AreaData("Doc Poison Areas", geometries.ToArray());
    }

    private Geometry? ConvertJsonElementToGeometry(JsonElement geometry)
    {
        var type = geometry.GetProperty("type").GetString();
        var coordinates = geometry.GetProperty("coordinates");

        switch (type)
        {
            case "MultiPolygon":
                return ConvertMutiPolygon(coordinates);
            
            default:
                Console.WriteLine ($"Warning: Encountered unknown type {type} when converting json element to geometry.");
                return null;
        }
    }

    private Geometry? ConvertMutiPolygon(JsonElement coordinates)
    {
        throw new NotImplementedException();
        return null;
    }

    public async Task GetPesticidesDataAsync()
    {
    }

}