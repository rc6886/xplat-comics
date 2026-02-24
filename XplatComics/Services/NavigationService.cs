using System;
using Microsoft.Extensions.DependencyInjection;
using XplatComics.ViewModels;

namespace XplatComics.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;
    private readonly MainViewModel _mainViewModel;

    public NavigationService(IServiceProvider services, MainViewModel mainViewModel)
    {
        _services = services;
        _mainViewModel = mainViewModel;
    }

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        var vm = _services.GetRequiredService<TViewModel>();
        NavigateTo(vm);
    }

    public void NavigateTo(ViewModelBase viewModel)
    {
        _mainViewModel.CurrentPage = viewModel;
    }

    public void GoBack()
    {
        _mainViewModel.NavigateToLibrary();
    }
}
