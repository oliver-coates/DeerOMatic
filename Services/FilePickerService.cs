using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Deer_o_matic.Services;

public interface IFilePickerService
{
    Task<FileOutput[]> OpenFilesAsync();
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

    public async Task<FileOutput[]> OpenFilesAsync()
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
            FileOutput[] output = new FileOutput[files.Count];

            for (int fileIndex = 0; fileIndex < files.Count; fileIndex++)
            {
                output[fileIndex] = await FileOutput.CreateAsnyc(files[fileIndex]);
            }

            return output;
        }
    }

}

public class FileOutput
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

    private FileOutput() { }

    async public static Task<FileOutput> CreateAsnyc(IStorageFile file)
    {
        FileOutput output = new FileOutput()
        {
            name = file.Name,
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