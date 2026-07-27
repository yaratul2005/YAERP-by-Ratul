using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YAERP.UI.Services;

namespace YAERP.UI.Views.Controls;

public partial class CommandPaletteOverlay : UserControl
{
    public new static readonly DependencyProperty IsVisibleProperty = DependencyProperty.Register(
        nameof(IsVisible), typeof(bool), typeof(CommandPaletteOverlay), new PropertyMetadata(false, OnIsVisibleChanged));

    public new bool IsVisible
    {
        get => (bool)GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    public CommandPaletteOverlay()
    {
        InitializeComponent();
    }

    private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CommandPaletteOverlay control && (bool)e.NewValue)
        {
            control.Dispatcher.BeginInvoke(() =>
            {
                control.SearchInput.Focus();
                control.SearchInput.SelectAll();
            });
        }
    }

    private void SearchInput_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            IsVisible = false;
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            if (ResultsList.Items.Count > 0)
            {
                ResultsList.SelectedIndex = 0;
                var item = (ListBoxItem)ResultsList.ItemContainerGenerator.ContainerFromIndex(0);
                item?.Focus();
            }
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            ExecuteSelected();
            e.Handled = true;
        }
    }

    private void ResultsList_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            IsVisible = false;
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            ExecuteSelected();
            e.Handled = true;
        }
        else if (e.Key == Key.Up && ResultsList.SelectedIndex == 0)
        {
            SearchInput.Focus();
            e.Handled = true;
        }
    }

    private void ResultsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        ExecuteSelected();
    }

    private void ExecuteSelected()
    {
        if (ResultsList.SelectedItem is PaletteCommandItem item && item.ExecutionCommand?.CanExecute(null) == true)
        {
            item.ExecutionCommand.Execute(null);
            IsVisible = false;
        }
    }
}
