using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YAERP.UI.Services;

public interface INavigationService
{
    ObservableObject? CurrentView { get; }
    void NavigateTo<TViewModel>() where TViewModel : ObservableObject;
}
