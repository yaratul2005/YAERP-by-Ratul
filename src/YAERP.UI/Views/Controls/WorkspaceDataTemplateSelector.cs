using System.Windows;
using System.Windows.Controls;
using YAERP.UI.ViewModels;
using YAERP.UI.ViewModels.Security;
using YAERP.UI.Workspace;

namespace YAERP.UI.Views.Controls;

public class WorkspaceDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate? DashboardTemplate { get; set; }
    public DataTemplate? InventoryTemplate { get; set; }
    public DataTemplate? SalesTemplate { get; set; }
    public DataTemplate? FinanceTemplate { get; set; }
    public DataTemplate? PosTemplate { get; set; }
    public DataTemplate? ApprovalCenterTemplate { get; set; }
    public DataTemplate? PluginHubTemplate { get; set; }
    public DataTemplate? ManufacturingTemplate { get; set; }
    public DataTemplate? WarehouseManagementTemplate { get; set; }
    public DataTemplate? DeepFinancialsTemplate { get; set; }
    public DataTemplate? UserAndRolesTemplate { get; set; }
    public DataTemplate? SystemSettingsTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        return item switch
        {
            DashboardViewModel => DashboardTemplate,
            InventoryViewModel => InventoryTemplate,
            SalesViewModel => SalesTemplate,
            FinanceViewModel => FinanceTemplate,
            PosViewModel => PosTemplate,
            ApprovalCenterViewModel => ApprovalCenterTemplate,
            PluginHubViewModel => PluginHubTemplate,
            ManufacturingViewModel => ManufacturingTemplate,
            WarehouseManagementViewModel => WarehouseManagementTemplate,
            DeepFinancialsViewModel => DeepFinancialsTemplate,
            UserAndRolesViewModel => UserAndRolesTemplate,
            SystemSettingsViewModel => SystemSettingsTemplate,
            _ => base.SelectTemplate(item, container)
        } ?? base.SelectTemplate(item, container);
    }
}
