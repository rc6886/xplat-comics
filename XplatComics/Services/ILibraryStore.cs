using System.Collections.Generic;
using System.Threading.Tasks;
using XplatComics.Models;

namespace XplatComics.Services;

public interface ILibraryStore
{
    Task<List<ComicBook>> LoadAsync();
    Task SaveAsync(List<ComicBook> books);
}
