using System;
using System.IO;
using XplatComics.Models;

namespace XplatComics.Services.Archives;

public static class ComicArchiveFactory
{
    public static IComicArchive Open(string filePath)
    {
        var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
        return extension switch
        {
            ".cbz" => new CbzArchive(File.OpenRead(filePath)),
            ".cbr" => new CbrArchive(File.OpenRead(filePath)),
            _ => throw new NotSupportedException($"Unsupported archive format: {extension}")
        };
    }

    public static ComicArchiveType GetArchiveType(string filePath)
    {
        var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
        return extension switch
        {
            ".cbz" => ComicArchiveType.Cbz,
            ".cbr" => ComicArchiveType.Cbr,
            _ => throw new NotSupportedException($"Unsupported archive format: {extension}")
        };
    }
}
