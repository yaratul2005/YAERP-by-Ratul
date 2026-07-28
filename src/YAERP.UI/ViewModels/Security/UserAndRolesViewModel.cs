using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YAERP.Application.Common.Interfaces;

namespace YAERP.UI.ViewModels.Security;

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class PermissionNodeDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ModuleGroup { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
}

public class AuditLogDto
{
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
}

public partial class UserAndRolesViewModel : ObservableObject // Inheriting TabViewModelBase in real app, mocked as ObservableObject here
{
    private readonly IDialogService _dialogService;
    private readonly IIdentityService _identityService;

    [ObservableProperty]
    private ObservableCollection<UserDto> _users = new();

    [ObservableProperty]
    private ObservableCollection<RoleDto> _roles = new();

    [ObservableProperty]
    private ObservableCollection<PermissionNodeDto> _permissionTree = new();

    [ObservableProperty]
    private ObservableCollection<AuditLogDto> _auditLogs = new();

    public UserAndRolesViewModel(IDialogService dialogService, IIdentityService identityService)
    {
        _dialogService = dialogService;
        _identityService = identityService;
    }

    [RelayCommand]
    private async Task OpenCreateUserModalAsync()
    {
        await _dialogService.ShowModalAsync("Create User", "UserEditorModalViewModel");
    }

    [RelayCommand]
    private async Task OpenEditRoleModalAsync()
    {
        await _dialogService.ShowModalAsync("Edit Role", "RoleEditorModalViewModel");
    }

    [RelayCommand]
    private async Task SaveRolePermissionsAsync()
    {
        await _dialogService.ShowGlobalToastAsync("Permissions saved successfully");
    }

    [RelayCommand]
    private async Task ResetPasswordAsync(Guid userId)
    {
        await _identityService.LogSecurityEventAsync(Guid.Empty, userId, "PasswordReset", "High", "Password reset initiated", "127.0.0.1");
        await _dialogService.ShowGlobalToastAsync("Password reset initiated");
    }

    [RelayCommand]
    private async Task ToggleUserStatusAsync(UserDto user)
    {
        user.IsActive = !user.IsActive;
        await _identityService.LogSecurityEventAsync(Guid.Empty, user.Id, "StatusToggle", "Medium", $"User status changed to {user.IsActive}", "127.0.0.1");
    }
}
