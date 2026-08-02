using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YAERP.Application.Common.Interfaces;
using YAERP.UI.Services;

namespace YAERP.UI.ViewModels.Modals;

public partial class GoogleSheetsExportModalViewModel : ObservableObject
{
    private readonly IGoogleAuthService _authService;
    private readonly IGoogleDriveService _driveService;
    private readonly IGoogleSheetsService _sheetsService;
    private readonly IModalService _modalService;
    private readonly IDialogService _dialogService;

    private readonly List<string> _headers;
    private readonly List<List<object>> _rows;

    [ObservableProperty]
    private string _spreadsheetTitle = "YAERP Data Export";

    [ObservableProperty]
    private string _selectedFolderName = "YAERP Root Folder";

    [ObservableProperty]
    private string? _selectedFolderId;

    [ObservableProperty]
    private bool _freezeHeader = true;

    [ObservableProperty]
    private bool _boldHeaders = true;

    [ObservableProperty]
    private bool _alternatingShading = true;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private GoogleUserInfoDto? _userInfo;

    public GoogleSheetsExportModalViewModel(
        IGoogleAuthService authService,
        IGoogleDriveService driveService,
        IGoogleSheetsService sheetsService,
        IModalService modalService,
        IDialogService dialogService,
        string defaultTitle,
        List<string> headers,
        List<List<object>> rows)
    {
        _authService = authService;
        _driveService = driveService;
        _sheetsService = sheetsService;
        _modalService = modalService;
        _dialogService = dialogService;

        SpreadsheetTitle = defaultTitle;
        _headers = headers;
        _rows = rows;

        _ = LoadUserInfoAsync();
    }

    [RelayCommand]
    private async Task LoadUserInfoAsync()
    {
        UserInfo = await _authService.GetCurrentUserInfoAsync();
    }

    [RelayCommand]
    private async Task ConnectAccountAsync()
    {
        IsBusy = true;
        try
        {
            UserInfo = await _authService.AuthenticateAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DisconnectAccountAsync()
    {
        await _authService.DisconnectAsync();
        UserInfo = null;
    }

    [RelayCommand]
    private void SelectDestinationFolder()
    {
        var pickerVm = new GoogleDrivePickerModalViewModel(_driveService, _dialogService, folder =>
        {
            if (folder != null)
            {
                SelectedFolderName = folder.Name;
                SelectedFolderId = folder.Id;
            }
            _modalService.CloseModal();
        });

        _modalService.OpenModal("Select Google Drive Destination Folder", pickerVm);
    }

    [RelayCommand]
    private async Task ExportAndOpenAsync()
    {
        if (!(_authService.IsConnected || UserInfo?.IsConnected == true))
        {
            UserInfo = await _authService.AuthenticateAsync();
        }

        IsBusy = true;
        try
        {
            string url = await _sheetsService.ExportDataGridToSheetAsync(
                SpreadsheetTitle,
                _headers,
                _rows,
                SelectedFolderId,
                FreezeHeader,
                BoldHeaders,
                AlternatingShading);

            await _dialogService.ShowGlobalToastAsync($"Export Successful! Google Sheet URL: {url}");
            _modalService.CloseModal();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowGlobalToastAsync($"Export failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _modalService.CloseModal();
    }
}
