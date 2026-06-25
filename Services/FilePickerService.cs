using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Deer_o_matic.Services;

public interface IFilePickerService
{
    Task<PickedFile[]> OpenFilesAsync();
    Task<IStorageFolder?> PickFileSaveLocation();
}

public class KmlPickerService : IFilePickerService
{
    private readonly TopLevel _topLevel;

    public KmlPickerService(TopLevel topLevel)
    {
        _topLevel = topLevel;
    }

    public static FilePickerFileType Kml { get; } = new("Kml")
    {
        Patterns = ["*.kml", "*.kmz"],
        AppleUniformTypeIdentifiers = null,
        MimeTypes = null
    };

    public async Task<PickedFile[]> OpenFilesAsync()
    {
        var files = await _topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Please choose KML file(s)",
            FileTypeFilter = new[] { Kml },
            AllowMultiple = true
        });       

        // If user selects no files
        if (files.Count == 0)
        {
            return [];
        }
        else
        {
            PickedFile[] output = new PickedFile[files.Count];

            for (int fileIndex = 0; fileIndex < files.Count; fileIndex++)
            {
                output[fileIndex] = await PickedFile.CreateAsnyc(files[fileIndex]);
            }

            return output;
        }
    }

    public async Task<IStorageFolder?> PickFileSaveLocation()
    {
        IStorageFolder? downloadsFolder = await _topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Downloads);

        var selectedFolders = await _topLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Choose where to export",
                SuggestedStartLocation = downloadsFolder,
            }
        );

        if (selectedFolders is null || selectedFolders.Count == 0)
        {
            // User cancelelled
            return null;
        }

        return selectedFolders[0];
    }
}

public class PickedFile
{
    /// <summary>
    /// The name and file extension.
    /// </summary>
    public string name = String.Empty;
    /// <summary>
    /// The file extension, including the '.'
    /// </summary>
    public string extension = String.Empty;

    /// <summary>
    /// The content of the file
    /// </summary>
    public string content = String.Empty;

    /// <summary>
    /// The path to the file
    /// </summary>
    public string path = String.Empty;

    private PickedFile() { }

    async public static Task<PickedFile> CreateAsnyc(IStorageFile file)
    {
        PickedFile output = new PickedFile()
        {
            name = file.Name,
            path = file.Path.ToString(),
            extension = GetFileExtension(file.Name),
            content = await ReadContent(file)
        };
        
        return output;
    }

    private static string GetFileExtension(string name)
    {
        // TODO: Fix this: Extensions with multiple '.'s will cause an error
        return $".{name.Split('.')[1]}";
    }

    async private static Task<string> ReadContent(IStorageFile file)
    {
        await using var stream = await file.OpenReadAsync();
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}