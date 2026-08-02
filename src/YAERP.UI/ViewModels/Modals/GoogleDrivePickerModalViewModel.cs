using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YAERP.Application.Common.Interfaces;
using YAERP.UI.Services;

namespace YAERP.UI.ViewModels.Modals;

public partial class GoogleDrivePickerModalViewModel : ObservableObject
{
    private readonly IGoogleDriveService _driveService;
    private readonly IDialogService _dialogService;
    private readonly Action<DriveFolderDto?> _onFolderSelected;

    [ObservableProperty]
    private string _currentFolderPath = "YAERP Root Folder";

    [ObservableProperty]
    private string? _currentFolderId;

    [ObservableProperty]
    private DriveFolderDto? _selectedFolder;

    [ObservableProperty]
    private string _newFolderName = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private ObservableCollection<DriveFolderDto> _folders = new();

    public GoogleDrivePickerModalViewModel(
        IGoogleDriveService driveService,
        IDialogService dialogService,
        Action<DriveFolderDto?> onFolderSelected)
    {
        _driveService = driveService;
        _dialogService = dialogService;
        _onFolderSelected = onFolderSelected;

        _ = LoadFoldersAsync();
    }

    [RelayCommand]
    private async Task LoadFoldersAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _driveService.GetAppFoldersAsync(CurrentFolderId);
            Folders.Clear();
            foreach (var item in list)
            {
                Folders.Add(item);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CreateFolderAsync()
    {
        if (string.IsNullOrWhiteSpace(NewFolderName)) return;

        IsBusy = true;
        try
        {
            var newFolder = await _driveService.CreateFolderAsync(NewFolderName, CurrentFolderId);
            NewFolderName = string.Empty;
            await LoadFoldersAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ConfirmSelection()
    {
        _onFolderSelected(SelectedFolder ?? new DriveFolderDto(CurrentFolderId ?? "folder-root", CurrentFolderPath, null, DateTime.Now.ToString("yyyy-MM-dd")));
    }

    [RelayCommand]
    private void Cancel()
    {
        _onFolderSelected(null);
    }
}
