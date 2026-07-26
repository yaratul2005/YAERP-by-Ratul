using System.Windows.Controls;
using System.Windows.Input;

namespace YAERP.UI.Views;

public partial class PosView : UserControl
{
    public PosView()
    {
        InitializeComponent();
    }

    private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Preview keydown handler for UI terminal shortcuts
    }
}
