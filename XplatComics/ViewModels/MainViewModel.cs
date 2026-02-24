using CommunityToolkit.Mvvm.ComponentModel;
using XplatComics.Services;

namespace XplatComics.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase? _currentPage;

    private readonly LibraryViewModel _libraryViewModel;

    public MainViewModel(LibraryViewModel libraryViewModel)
    {
        _libraryViewModel = libraryViewModel;
        _currentPage = _libraryViewModel;
    }

    public void NavigateToLibrary()
    {
        CurrentPage = _libraryViewModel;
    }
}
