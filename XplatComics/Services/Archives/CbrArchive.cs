using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpCompress.Archives;
using XplatComics.Models;

namespace XplatComics.Services.Archives;

public sealed class CbrArchive : IComicArchive
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
    };

    private readonly IArchive _archive;
    private readonly List<ComicPage> _pages;
    private readonly Dictionary<string, IArchiveEntry> _entryMap;

    public CbrArchive(Stream stream)
    {
        _archive = ArchiveFactory.OpenArchive(stream);
        var imageEntries = _archive.Entries
            .Where(e => !e.IsDirectory && ImageExtensions.Contains(Path.GetExtension(e.Key ?? "")))
            .OrderBy(e => e.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _pages = imageEntries
            .Select((e, i) => new ComicPage(i, e.Key!))
            .ToList();

        _entryMap = imageEntries.ToDictionary(e => e.Key!, e => e, StringComparer.OrdinalIgnoreCase);
    }

    public int PageCount => _pages.Count;
    public IReadOnlyList<ComicPage> Pages => _pages;

    public Task<Stream> GetPageStreamAsync(int pageIndex, CancellationToken ct = default)
    {
        if (pageIndex < 0 || pageIndex >= _pages.Count)
            throw new ArgumentOutOfRangeException(nameof(pageIndex));

        var entry = _entryMap[_pages[pageIndex].EntryKey];
        var ms = new MemoryStream();
        using (var entryStream = entry.OpenEntryStream())
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
