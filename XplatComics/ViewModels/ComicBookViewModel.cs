using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Media.Imaging;
using XplatComics.Models;

namespace XplatComics.ViewModels;

public partial class ComicBookViewModel : ViewModelBase
{
    [ObservableProperty]
    private Bitmap? _coverImage;

    [ObservableProperty]
    private double _progress;

    public ComicBook Book { get; }

    public string Title => Book.Title;

    public ComicBookViewModel(ComicBook book)
    {
        Book = book;
        _progress = book.Progress;
    }
}
