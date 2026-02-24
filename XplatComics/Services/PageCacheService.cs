using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using XplatComics.Services.Archives;

namespace XplatComics.Services;

public class PageCacheService : IPageCacheService
{
    private const int MaxCacheSize = 10;
    private readonly ConcurrentDictionary<int, Bitmap> _cache = new();
    private readonly LinkedList<int> _accessOrder = new();
    private readonly object _lruLock = new();

    public async Task<Bitmap> GetPageAsync(IComicArchive archive, int pageIndex, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(pageIndex, out var cached))
        {
            TouchLru(pageIndex);
            return cached;
        }

        using var stream = await archive.GetPageStreamAsync(pageIndex, ct);
        var bitmap = new Bitmap(stream);

        if (_cache.TryAdd(pageIndex, bitmap))
        {
            AddToLru(pageIndex);
            EvictIfNeeded();
        }

        return bitmap;
    }

    public void Clear()
    {
        lock (_lruLock)
        {
            foreach (var kvp in _cache)
                kvp.Value.Dispose();
            _cache.Clear();
            _accessOrder.Clear();
        }
    }

    private void TouchLru(int key)
    {
        lock (_lruLock)
        {
            _accessOrder.Remove(key);
            _accessOrder.AddLast(key);
        }
    }

    private void AddToLru(int key)
    {
        lock (_lruLock)
        {
            _accessOrder.AddLast(key);
        }
    }

    private void EvictIfNeeded()
    {
        lock (_lruLock)
        {
            while (_accessOrder.Count > MaxCacheSize)
            {
                var oldest = _accessOrder.First!.Value;
                _accessOrder.RemoveFirst();
                if (_cache.TryRemove(oldest, out var bitmap))
                    bitmap.Dispose();
            }
        }
    }
}
