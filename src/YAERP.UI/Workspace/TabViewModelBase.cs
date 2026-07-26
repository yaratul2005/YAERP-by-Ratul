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
}
