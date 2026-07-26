// Zero Code-Behind Rule
using YAERP.UI.ViewModels;

namespace YAERP.UI.Views;

public partial class SalesView
{
    public SalesView(SalesViewModel viewModel)
    {
        DataContext = viewModel;
    }
    public object DataContext { get; set; } = default!;
}
