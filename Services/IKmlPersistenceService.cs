using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Deer_o_matic.Services;

public interface IKmlPersistenceService
{
    /// <summary>
    /// Zips and saves a provided file into the persistent data path so it can be reloaded on application startup.
    /// </summary>
    public Task SaveKmlFileAsync(PickedFile file, string category);

    /// <summary>
    /// Retrieves all files under the category within the persistent data path.
    /// </summary>
    public Task<PickedFile[]> GetAllKmlFiles(string category);
}

public class KmlPersistenceService : IKmlPersistenceService
{
    private readonly string _rootDataPath;

    public KmlPersistenceService()
    {
        _rootDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Deer-o-matic");
        // Create the directory if it doesn't exist
        Directory.CreateDirectory(_rootDataPath);
    }

    public async Task<PickedFile[]> GetAllKmlFiles(string category)
    {
        string categoryFolderPath = Path.Combine(_rootDataPath, category);

        if (Directory.Exists(categoryFolderPath) == false)
        {
            return [];
            // throw new DirectoryNotFoundException($"Could not find directory for '{category}' in persistent data folder.");
        }

        List<PickedFile> readFiles = new List<PickedFile>();

        foreach (string filePath in Directory.GetFiles(categoryFolderPath))
        {
            if (filePath.EndsWith(".kmz"))
            {
                string name = filePath.Split('/').Last();
                string extension = "." + name.Split('.').Last();
                string content = await UnzipAndRead(filePath);
                Uri path = new(filePath);

                readFiles.Add(new PickedFile(name, extension, content, path));
            }
        }

        return [.. readFiles];
    }

    public async Task SaveKmlFileAsync(PickedFile file, string category)
    {

        // Unzip the archive if needed:
        if (file.extension == ".kmz")
        {
            string path = file.pathUri.LocalPath;
            file.content = await UnzipAndRead(path);
        }

        string categoryFolderPath = Path.Combine(_rootDataPath, category);

        string zipFileName = file.name.Split('.').First() + ".kmz";
        string zipPath = Path.Combine(_rootDataPath, category, zipFileName);        

        if (Directory.Exists(zipPath))
        {
            throw new DuplicateNameException("A poison data already exists with this name");
        }

        // Create a temporary directory to save our content to
        string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Save the file content to the temp directory
            string fileName = file.name.Split('.').First() + ".kml";
            string tempFilePath = Path.Combine(tempDir, fileName);
            await File.WriteAllTextAsync(tempFilePath, file.content);

            // Zip the files from the temp directory to the proper directory
            Directory.CreateDirectory(categoryFolderPath);
            await ZipFile.CreateFromDirectoryAsync(tempDir, zipPath);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }

    }

    private async Task<string> UnzipAndRead(string path)
    {
        using ZipArchive archive = await ZipFile.OpenReadAsync(path);
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (entry.Name.EndsWith(".kml"))
            {
                Stream s = entry.Open();
                StreamReader reader = new StreamReader(s);
                return await reader.ReadToEndAsync();
            }
        }

        throw new Exception($"Could not find any files ending with .kml in '{path}'.");
    }
}