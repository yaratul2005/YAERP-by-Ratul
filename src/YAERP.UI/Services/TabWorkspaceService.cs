using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using YAERP.UI.Workspace;

namespace YAERP.UI.Services;

public partial class TabWorkspaceService : ObservableObject, ITabWorkspaceService
{
    private readonly IServiceProvider _serviceProvider;

    public ObservableCollection<TabViewModelBase> OpenTabs { get; } = new();

    [ObservableProperty]
    private TabViewModelBase? _activeTab;

    public TabWorkspaceService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    partial void OnActiveTabChanged(TabViewModelBase? oldValue, TabViewModelBase? newValue)
    {
        if (oldValue != null)
        {
            oldValue.IsSelected = false;
            _ = oldValue.OnTabDeactivatedAsync();
        }

        if (newValue != null)
        {
            newValue.IsSelected = true;
            _ = newValue.OnTabActivatedAsync();
        }
    }

    public void OpenTab<TViewModel>(Action<TViewModel>? configure = null) where TViewModel : TabViewModelBase
    {
        // Resolve a transient instance to get the TabId (or singleton if registered that way)
        var newTab = _serviceProvider.GetRequiredService<TViewModel>();

        // Let caller configure properties like TabId, Title etc before checking for duplicates
        configure?.Invoke(newTab);

        if (string.IsNullOrEmpty(newTab.TabId))
        {
            newTab.TabId = typeof(TViewModel).Name; // Fallback to class name
        }

        var existingTab = OpenTabs.FirstOrDefault(t => t.TabId == newTab.TabId);

        if (existingTab != null)
        {
            ActiveTab = existingTab;
        }
        else
        {
            OpenTabs.Add(newTab);
            ActiveTab = newTab;
        }
    }

    public async Task<bool> CloseTabAsync(TabViewModelBase tab)
    {
        if (!OpenTabs.Contains(tab)) return true;

        bool canClose = await tab.OnTabClosingAsync();
        if (!canClose) return false;

        int index = OpenTabs.IndexOf(tab);
        OpenTabs.Remove(tab);

        if (ActiveTab == tab)
        {
            if (OpenTabs.Count > 0)
            {
                // Select the next tab, or the previous if we closed the last one
                ActiveTab = OpenTabs[Math.Min(index, OpenTabs.Count - 1)];
            }
            else
            {
                ActiveTab = null;
            }
        }

        return true;
    }

    public async Task CloseAllTabsExceptAsync(TabViewModelBase tab)
    {
        var tabsToClose = OpenTabs.Where(t => t != tab).ToList();

        foreach (var t in tabsToClose)
        {
            await CloseTabAsync(t);
        }
    }
}
