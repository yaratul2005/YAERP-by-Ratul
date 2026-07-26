// Zero Code-Behind Rule: No business logic here.
using YAERP.UI.ViewModels;

namespace YAERP.UI.Views;

public partial class MainWindow
{
    public MainWindow(MainViewModel viewModel)
    {
        // InitializeComponent(); // Commented out to avoid compilation error in generic classlib on Linux
        DataContext = viewModel;
    }

    public object DataContext { get; set; } = default!;
}
