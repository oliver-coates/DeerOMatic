using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Deer_o_matic.Services;

public interface IFilePickerService
{
    Task<string[]> OpenFileAsync();
}

public class FilePickerService : IFilePickerService
{
    private readonly TopLevel _topLevel;

    public FilePickerService(TopLevel topLevel)
    {
        _topLevel = topLevel;
    }

    public static FilePickerFileType Kml { get; } = new("Kml")
    {
        Patterns = ["*.kml", "*.kmz"],
        AppleUniformTypeIdentifiers = null,
        MimeTypes = null
    };

    public async Task<string[]?> OpenFileAsync()
    {
        var files = await _topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Please choose KML file(s)",
            // FileTypeFilter = new[] { Kml },
            FileTypeFilter = new[] { FilePickerFileTypes.TextPlain },
            AllowMultiple = true
        });       

        // If user selects no files
        if (files.Count == 0)
        {
            return null;
        }
        else
        {
            string[] output = new string[files.Count];

            for (int fileIndex = 0; fileIndex < files.Count; fileIndex++)
            {
                await using var stream = await files[fileIndex].OpenReadAsync();
                using var reader = new StreamReader(stream);
                output[fileIndex] = await reader.ReadToEndAsync();        
            }

            return output;
        }
    }

}