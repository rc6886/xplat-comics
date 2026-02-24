using XplatComics.ViewModels;

namespace XplatComics.Services;

public interface INavigationService
{
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    void NavigateTo(ViewModelBase viewModel);
    void GoBack();
}
