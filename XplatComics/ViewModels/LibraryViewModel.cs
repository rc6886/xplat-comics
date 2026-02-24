using CommunityToolkit.Mvvm.ComponentModel;

namespace XplatComics.ViewModels;

public partial class LibraryViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _statusMessage = "No comics loaded. Use File > Open to add comics.";
}
