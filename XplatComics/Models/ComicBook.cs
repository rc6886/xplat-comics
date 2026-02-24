using System;
using System.Text.Json.Serialization;

namespace XplatComics.Models;

public class ComicBook
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string FilePath { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ComicArchiveType ArchiveType { get; set; }
    public int PageCount { get; set; }
    public int LastReadPage { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public double Progress => PageCount > 0 ? (double)LastReadPage / PageCount : 0;
}
