using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using YAERP.UI.Workspace;

namespace YAERP.UI.Services;

public interface ITabWorkspaceService : INotifyPropertyChanged
{
    ObservableCollection<TabViewModelBase> OpenTabs { get; }
    TabViewModelBase? ActiveTab { get; set; }

    void OpenTab<TViewModel>(Action<TViewModel>? configure = null) where TViewModel : TabViewModelBase;
    Task<bool> CloseTabAsync(TabViewModelBase tab);
    Task CloseAllTabsExceptAsync(TabViewModelBase tab);
}
