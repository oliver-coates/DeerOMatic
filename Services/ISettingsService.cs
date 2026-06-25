using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Deer_o_matic.Models;

namespace Deer_o_matic.Services;

public interface ISettingsService
{
    public Task SaveHunterDeclarationSettingsAsync(HunterDeclarationSettings settings);
    public Task<HunterDeclarationSettings?> LoadHunterDeclarationSettingsAsync();
}


public class SettingsService : ISettingsService
{
    private readonly string _settingsPath;

    public SettingsService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "Deer-o-matic");
        
        if (Directory.Exists(appFolder) == false)
        {
            Directory.CreateDirectory(appFolder);
        }

        _settingsPath = Path.Combine(appFolder, "Hunter Declaration Settings.json");
    }

    public async Task<HunterDeclarationSettings?> LoadHunterDeclarationSettingsAsync()
    {
        if (File.Exists(_settingsPath) == false)
        {
            // Ignore if nothing exists at the settings path
            return null;
        }

        var json = await File.ReadAllTextAsync(_settingsPath);

        return JsonSerializer.Deserialize<HunterDeclarationSettings>(json);
    }

    public async Task SaveHunterDeclarationSettingsAsync(HunterDeclarationSettings settings)
    {
        string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_settingsPath, json);
    }

}
