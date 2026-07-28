using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YAERP.Application.Common.Interfaces;

namespace YAERP.UI.ViewModels.Drawers;

public class ConflictDto
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string LocalPayload { get; set; } = string.Empty;
    public string RemotePayload { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
}

public partial class ConflictResolverDrawerViewModel : ObservableObject
{
    private readonly ISyncEngineService _syncEngineService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<ConflictDto> _conflicts = new();

    [ObservableProperty]
    private ConflictDto? _selectedConflict;

    public ConflictResolverDrawerViewModel(ISyncEngineService syncEngineService, IDialogService dialogService)
    {
        _syncEngineService = syncEngineService;
        _dialogService = dialogService;
        LoadConflicts();
    }

    private void LoadConflicts()
    {
        // Mock data
        Conflicts.Add(new ConflictDto
        {
            Id = Guid.NewGuid(),
            EntityType = "Customer",
            LocalPayload = "{ \"Name\": \"Acme Corp\", \"CreditLimit\": 50000 }",
            RemotePayload = "{ \"Name\": \"Acme Corporation\", \"CreditLimit\": 45000 }",
            DetectedAt = DateTime.UtcNow
        });
    }

    [RelayCommand]
    private async Task KeepLocalVersionAsync()
    {
        if (SelectedConflict != null)
        {
            await _syncEngineService.ResolveConflictAsync(SelectedConflict.Id, "ClientWins");
            Conflicts.Remove(SelectedConflict);
            SelectedConflict = null;
            await _dialogService.ShowGlobalToastAsync("Resolved using Local version");
        }
    }

    [RelayCommand]
    private async Task AcceptCloudVersionAsync()
    {
        if (SelectedConflict != null)
        {
            await _syncEngineService.ResolveConflictAsync(SelectedConflict.Id, "ServerWins");
            Conflicts.Remove(SelectedConflict);
            SelectedConflict = null;
            await _dialogService.ShowGlobalToastAsync("Resolved using Cloud version");
        }
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        _dialogService.CloseActiveOverlay();
        await Task.CompletedTask;
    }
}
