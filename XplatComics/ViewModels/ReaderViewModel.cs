using CommunityToolkit.Mvvm.ComponentModel;

namespace XplatComics.ViewModels;

public partial class ReaderViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private int _currentPageIndex;

    [ObservableProperty]
    private int _totalPages;
}
