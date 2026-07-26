// Zero Code-Behind Rule
using YAERP.UI.ViewModels;

namespace YAERP.UI.Views;

public partial class DashboardView
{
    public DashboardView(DashboardViewModel viewModel)
    {
        DataContext = viewModel;
    }
    public object DataContext { get; set; } = default!;
}
