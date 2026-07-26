using System.Windows;
using YAERP.UI.ViewModels;

namespace YAERP.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
