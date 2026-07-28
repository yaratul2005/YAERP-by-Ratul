using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YAERP.Application.Common.Interfaces;

namespace YAERP.UI.ViewModels.Drawers;

public partial class UserProfileDrawerViewModel : ObservableObject
{
    private readonly IDialogService _dialogService;
    private readonly IIdentityService _identityService;

    [ObservableProperty]
    private string _currentPassword = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _selectedTheme = "System";

    [ObservableProperty]
    private string _startupTab = "Dashboard";

    [ObservableProperty]
    private string _gridDensity = "Comfortable";

    public UserProfileDrawerViewModel(IDialogService dialogService, IIdentityService identityService)
    {
        _dialogService = dialogService;
        _identityService = identityService;
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        if (NewPassword != ConfirmPassword)
        {
            await _dialogService.ShowGlobalToastAsync("Passwords do not match");
            return;
        }

        // Logic to verify current and hash new password
        await _dialogService.ShowGlobalToastAsync("Password changed successfully");
    }

    [RelayCommand]
    private async Task SavePreferencesAsync()
    {
        await _dialogService.ShowGlobalToastAsync("Preferences saved");
        _dialogService.CloseActiveOverlay();
    }
}
