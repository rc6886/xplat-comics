using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace XplatComics.Services;

public interface IThumbnailService
{
    Task<Bitmap?> GetThumbnailAsync(string comicId, string filePath, CancellationToken ct = default);
}
