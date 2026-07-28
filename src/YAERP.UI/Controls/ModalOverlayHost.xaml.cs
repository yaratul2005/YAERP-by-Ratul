using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace YAERP.UI.Controls;

public partial class ModalOverlayHost : UserControl
{
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(ModalOverlayHost), new PropertyMetadata(false));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(ModalOverlayHost), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ModalContentProperty =
        DependencyProperty.Register(nameof(ModalContent), typeof(object), typeof(ModalOverlayHost), new PropertyMetadata(null));

    public static readonly DependencyProperty CloseCommandProperty =
        DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), typeof(ModalOverlayHost), new PropertyMetadata(null));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public object? ModalContent
    {
        get => GetValue(ModalContentProperty);
        set => SetValue(ModalContentProperty, value);
    }


    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape && IsOpen && CloseCommand?.CanExecute(null) == true)
        {
            CloseCommand.Execute(null);
            e.Handled = true;
        }
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == IsOpenProperty && (bool)e.NewValue)
        {
            this.Focus();
            Keyboard.Focus(this);
        }
    }

    public ICommand? CloseCommand
    {
        get => (ICommand?)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public ModalOverlayHost()
    {
        InitializeComponent();
    }
}
