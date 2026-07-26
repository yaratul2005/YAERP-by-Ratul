// Zero Code-Behind Rule
using YAERP.UI.ViewModels;

namespace YAERP.UI.Views;

public partial class InventoryView
{
    public InventoryView(InventoryViewModel viewModel)
    {
        DataContext = viewModel;
    }
    public object DataContext { get; set; } = default!;
}
