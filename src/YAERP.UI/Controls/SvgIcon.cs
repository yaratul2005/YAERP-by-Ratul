using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace YAERP.UI.Controls;

public class SvgIcon : Control
{
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(Geometry), typeof(SvgIcon), new PropertyMetadata(null));

    public Geometry? Data
    {
        get => (Geometry?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    static SvgIcon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SvgIcon), new FrameworkPropertyMetadata(typeof(SvgIcon)));
    }
}
