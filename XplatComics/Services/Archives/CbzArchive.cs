using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XplatComics.Models;

namespace XplatComics.Services.Archives;

public sealed class CbzArchive : IComicArchive
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
    };

    private readonly ZipArchive _archive;
    private readonly List<ComicPage> _pages;

    public CbzArchive(Stream stream)
    {
        _archive = new ZipArchive(stream, ZipArchiveMode.Read);
        _pages = _archive.Entries
            .Where(e => ImageExtensions.Contains(Path.GetExtension(e.FullName)))
            .OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase)
            .Select((e, i) => new ComicPage(i, e.FullName))
            .ToList();
    }

    public int PageCount => _pages.Count;
    public IReadOnlyList<ComicPage> Pages => _pages;

    public Task<Stream> GetPageStreamAsync(int pageIndex, CancellationToken ct = default)
    {
        if (pageIndex < 0 || pageIndex >= _pages.Count)
            throw new ArgumentOutOfRangeException(nameof(pageIndex));

        var entry = _archive.GetEntry(_pages[pageIndex].EntryKey)
            ?? throw new InvalidOperationException($"Entry not found: {_pages[pageIndex].EntryKey}");

        var ms = new MemoryStream();
        using (var entryStream = entry.Open())
        {
            entryStream.CopyTo(ms);
        }
        ms.Position = 0;
        return Task.FromResult<Stream>(ms);
    }

    public Task<Stream> GetCoverStreamAsync(CancellationToken ct = default)
    {
        return GetPageStreamAsync(0, ct);
    }

    public void Dispose()
    {
        _archive.Dispose();
    }
}
