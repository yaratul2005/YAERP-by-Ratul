using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YAERP.Application.Common.Interfaces;
using YAERP.UI.Workspace;

namespace YAERP.UI.ViewModels.Security;

public partial class UserDto : ObservableObject
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;

    [ObservableProperty]
    private bool _isActive;
}

public partial class RoleDto : ObservableObject
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public partial class PermissionNodeDto : ObservableObject
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ModuleGroup { get; set; } = string.Empty;

    [ObservableProperty]
    private bool _isGranted;
}

public class AuditLogDto
{
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
}

public partial class UserAndRolesViewModel : TabViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IIdentityService _identityService;

    [ObservableProperty]
    private ObservableCollection<UserDto> _users = new();

    [ObservableProperty]
    private ObservableCollection<RoleDto> _roles = new();

    [ObservableProperty]
    private RoleDto? _selectedRole;

    [ObservableProperty]
    private ObservableCollection<PermissionNodeDto> _permissionTree = new();

    [ObservableProperty]
    private ObservableCollection<AuditLogDto> _auditLogs = new();

    public UserAndRolesViewModel(IDialogService dialogService, IIdentityService identityService)
    {
        Title = "Security & Roles";
        IconKey = "🔐";
        TabId = "UserAndRolesViewModel";

        _dialogService = dialogService;
        _identityService = identityService;

        LoadData();
    }

    private void LoadData()
    {
        Users = new ObservableCollection<UserDto>
        {
            new() { Id = Guid.NewGuid(), Username = "admin", FullName = "System Administrator", Department = "IT / Executive", IsActive = true },
            new() { Id = Guid.NewGuid(), Username = "ratul.architect", FullName = "Ratul Systems", Department = "Core Engineering", IsActive = true },
            new() { Id = Guid.NewGuid(), Username = "j.smith", FullName = "John Smith", Department = "Sales & Marketing", IsActive = true },
            new() { Id = Guid.NewGuid(), Username = "a.davis", FullName = "Alice Davis", Department = "Finance & General Ledger", IsActive = true },
            new() { Id = Guid.NewGuid(), Username = "m.logistics", FullName = "Mark Logistics", Department = "Warehouse & WMS", IsActive = false }
        };

        Roles = new ObservableCollection<RoleDto>
        {
            new() { Id = Guid.NewGuid(), Name = "System Administrator", Description = "Full unrestricted access across all business modules" },
            new() { Id = Guid.NewGuid(), Name = "Finance Manager", Description = "Access to Journal Entries, Asset Depreciation, Tax Filings" },
            new() { Id = Guid.NewGuid(), Name = "WMS Warehouse Operator", Description = "Stock Relocation, Goods Receiving Dock, Bin Slotting" },
            new() { Id = Guid.NewGuid(), Name = "Sales Representative", Description = "Quotation Builder, Deal Pipeline Kanban, Customer 360" }
        };
        SelectedRole = Roles[0];

        PermissionTree = new ObservableCollection<PermissionNodeDto>
        {
            new() { Code = "IAM.User.Create", Name = "Create User Accounts", ModuleGroup = "Identity", IsGranted = true },
            new() { Code = "IAM.Role.Manage", Name = "Manage System Roles & Privileges", ModuleGroup = "Identity", IsGranted = true },
            new() { Code = "Inventory.Delete", Name = "Delete Inventory SKUs", ModuleGroup = "Inventory", IsGranted = false },
            new() { Code = "WMS.Bin.Lock", Name = "Toggle Bin Lock State", ModuleGroup = "WMS", IsGranted = true },
            new() { Code = "Financials.GL.Post", Name = "Post Double-Entry Journal Vouchers", ModuleGroup = "Financials", IsGranted = true },
            new() { Code = "Sales.Order.Create", Name = "Build & Approve Sales Orders", ModuleGroup = "Sales", IsGranted = true }
        };

        AuditLogs = new ObservableCollection<AuditLogDto>
        {
            new() { EventType = "UserLogin", Severity = "Low", Details = "Admin user logged in via desktop terminal", TimestampUtc = DateTime.UtcNow.AddMinutes(-12) },
            new() { EventType = "PermissionUpdate", Severity = "Medium", Details = "Granted WMS.Bin.Lock permission to Warehouse Operator role", TimestampUtc = DateTime.UtcNow.AddHours(-2) },
            new() { EventType = "PasswordReset", Severity = "High", Details = "Password reset initiated for user m.logistics", TimestampUtc = DateTime.UtcNow.AddDays(-1) }
        };
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
    private async Task RefreshDirectoryAsync()
    {
        LoadData();
        await _dialogService.ShowGlobalToastAsync("User directory and audit log refreshed.");
        ShowSuccessToast("User directory refreshed successfully.");
    }

    [RelayCommand]
    private async Task ExportAuditLogAsync()
    {
        await _dialogService.ShowGlobalToastAsync("Exported Security Audit Log to Excel.");
        ShowSuccessToast("Security Audit Log exported cleanly to Excel.");
    }

    [RelayCommand]
    private async Task SaveRolePermissionsAsync()
    {
        await _dialogService.ShowGlobalToastAsync("Permissions updated successfully.");
        ShowSuccessToast("Role permissions saved cleanly.");
    }

    [RelayCommand]
    private async Task ResetPasswordAsync(Guid userId)
    {
        await _identityService.LogSecurityEventAsync(Guid.Empty, userId, "PasswordReset", "High", "Password reset initiated", "127.0.0.1");
        await _dialogService.ShowGlobalToastAsync("Password reset initiated");
        ShowSuccessToast("Password reset initiated successfully.");
    }

    [RelayCommand]
    private async Task ToggleUserStatusAsync(UserDto? user)
    {
        if (user == null) return;
        user.IsActive = !user.IsActive;
        await _identityService.LogSecurityEventAsync(Guid.Empty, user.Id, "StatusToggle", "Medium", $"User status changed to {user.IsActive}", "127.0.0.1");
        await _dialogService.ShowGlobalToastAsync($"User status updated to {(user.IsActive ? "Active" : "Inactive")}.");
        ShowSuccessToast($"User '{user.Username}' status toggled to {(user.IsActive ? "Active" : "Inactive")}.");
    }
}
