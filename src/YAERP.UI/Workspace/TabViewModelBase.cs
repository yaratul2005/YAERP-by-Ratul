using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace YAERP.UI.Workspace;

public abstract partial class TabViewModelBase : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _iconKey = string.Empty;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private string _tabId = string.Empty;

    // Contextual Slide-Out Drawer State
    [ObservableProperty]
    private bool _isDrawerOpen;

    [ObservableProperty]
    private string _drawerTitle = string.Empty;

    [ObservableProperty]
    private object? _drawerContent;

    // Contextual Modal Dialog State
    [ObservableProperty]
    private bool _isModalOpen;

    [ObservableProperty]
    private string _modalTitle = string.Empty;

    [ObservableProperty]
    private object? _modalContent;

    // Toast Notification State
    [ObservableProperty]
    private bool _isToastVisible;

    [ObservableProperty]
    private string _toastMessage = string.Empty;

    [ObservableProperty]
    private bool _isSuccessToast = true;

    public ICommand CloseTabCommand { get; }
    public ICommand SaveTabCommand { get; }

    protected TabViewModelBase()
    {
        CloseTabCommand = new AsyncRelayCommand(OnCloseTabCommandExecutedAsync);
        SaveTabCommand = new AsyncRelayCommand(OnSaveTabCommandExecutedAsync);
    }

    public virtual Task OnTabActivatedAsync() => Task.CompletedTask;
    public virtual Task OnTabDeactivatedAsync() => Task.CompletedTask;
    public virtual Task<bool> OnTabClosingAsync() => Task.FromResult(true);

    protected virtual Task OnCloseTabCommandExecutedAsync() => Task.CompletedTask;
    protected virtual Task OnSaveTabCommandExecutedAsync() => Task.CompletedTask;

    public void OpenDrawer(object? content = null, string title = "")
    {
        if (content != null) DrawerContent = content;
        if (!string.IsNullOrWhiteSpace(title)) DrawerTitle = title;
        IsDrawerOpen = true;
    }

    [RelayCommand]
    private void OpenDrawerCommand() => OpenDrawer(null, string.Empty);

    [RelayCommand]
    public void CloseDrawer()
    {
        IsDrawerOpen = false;
        DrawerContent = null;
    }

    public void OpenModal(object? content = null, string title = "")
    {
        if (content != null) ModalContent = content;
        if (!string.IsNullOrWhiteSpace(title)) ModalTitle = title;
        IsModalOpen = true;
    }

    [RelayCommand]
    private void OpenModalCommand() => OpenModal(null, string.Empty);

    [RelayCommand]
    public void CloseModal()
    {
        IsModalOpen = false;
        ModalContent = null;
    }

    public void ShowSuccessToast(string message)
    {
        ToastMessage = message;
        IsSuccessToast = true;
        IsToastVisible = true;
    }

    public void ShowErrorToast(string message)
    {
        ToastMessage = message;
        IsSuccessToast = false;
        IsToastVisible = true;
    }

    [RelayCommand]
    public void HideToast()
    {
        IsToastVisible = false;
        ToastMessage = string.Empty;
    }
}
