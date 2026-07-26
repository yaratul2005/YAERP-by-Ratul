// Zero Code-Behind Rule
using YAERP.UI.ViewModels;

namespace YAERP.UI.Views;

public partial class FinanceView
{
    public FinanceView(FinanceViewModel viewModel)
    {
        DataContext = viewModel;
    }
    public object DataContext { get; set; } = default!;
}
