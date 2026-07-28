using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YAERP.Application.Common.Interfaces;

namespace YAERP.UI.ViewModels.Modals;

public partial class UserEditorModalViewModel : ObservableObject
{
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _department = string.Empty;

    [ObservableProperty]
    private string _selectedRole = string.Empty;

    [ObservableProperty]
    private bool _isMfaEnabled;

    public UserEditorModalViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        // Add save logic here
        await _dialogService.ShowGlobalToastAsync("User saved successfully");
        _dialogService.CloseActiveOverlay();
    }

    [RelayCommand]
    private void Cancel()
    {
        _dialogService.CloseActiveOverlay();
    }
}
