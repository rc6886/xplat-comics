using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using XplatComics.Services.Archives;

namespace XplatComics.Services;

public interface IPageCacheService
{
    Task<Bitmap> GetPageAsync(IComicArchive archive, int pageIndex, CancellationToken ct = default);
    void Clear();
}
