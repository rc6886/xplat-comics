using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using XplatComics.Models;

namespace XplatComics.Services.Archives;

public interface IComicArchive : IDisposable
{
    int PageCount { get; }
    IReadOnlyList<ComicPage> Pages { get; }
    Task<Stream> GetPageStreamAsync(int pageIndex, CancellationToken ct = default);
    Task<Stream> GetCoverStreamAsync(CancellationToken ct = default);
}
