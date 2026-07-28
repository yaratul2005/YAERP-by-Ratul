import re

for file_path in ['src/YAERP.UI/Controls/ModalOverlayHost.xaml.cs', 'src/YAERP.UI/Controls/DrawerHost.xaml.cs']:
    with open(file_path, 'r') as f:
        content = f.read()

    # Add key binding logic for ESC
    if 'OnKeyDown' not in content:
        method_str = """
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
"""
        content = content.replace('    public ICommand? CloseCommand', method_str + '\n    public ICommand? CloseCommand')

    with open(file_path, 'w') as f:
        f.write(content)
