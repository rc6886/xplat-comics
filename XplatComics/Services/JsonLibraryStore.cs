using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using XplatComics.Models;

namespace XplatComics.Services;

public class JsonLibraryStore : ILibraryStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _filePath;

    public JsonLibraryStore()
    {
        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "XplatComics");
        Directory.CreateDirectory(appData);
        _filePath = Path.Combine(appData, "library.json");
    }

    public async Task<List<ComicBook>> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new List<ComicBook>();

        await using var stream = File.OpenRead(_filePath);
        return await JsonSerializer.DeserializeAsync<List<ComicBook>>(stream, JsonOptions)
               ?? new List<ComicBook>();
    }

    public async Task SaveAsync(List<ComicBook> books)
    {
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, books, JsonOptions);
    }
}
