using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using XplatComics.Services.Archives;

namespace XplatComics.Services;

public class ThumbnailService : IThumbnailService
{
    private readonly string _cacheDir;

    public ThumbnailService()
    {
        _cacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "XplatComics", "thumbnails");
        Directory.CreateDirectory(_cacheDir);
    }

    public async Task<Bitmap?> GetThumbnailAsync(string comicId, string filePath, CancellationToken ct = default)
    {
        var cachePath = Path.Combine(_cacheDir, $"{comicId}.jpg");

        if (File.Exists(cachePath))
        {
            return new Bitmap(cachePath);
        }

        try
        {
            using var archive = ComicArchiveFactory.Open(filePath);
            using var coverStream = await archive.GetCoverStreamAsync(ct);
            var bitmap = new Bitmap(coverStream);

            // Save thumbnail to disk cache
            using var fileStream = File.Create(cachePath);
            bitmap.Save(fileStream);

            return bitmap;
        }
        catch
        {
            return null;
        }
    }
}
