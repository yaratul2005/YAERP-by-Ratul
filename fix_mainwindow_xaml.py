with open('src/YAERP.UI/Views/MainWindow.xaml', 'r') as f:
    content = f.read()

conflict_button = """                <!-- Conflict Resolver Trigger -->
                <Button Command="{Binding OpenConflictResolverCommand}" Style="{DynamicResource GhostButtonStyle}" Margin="0,0,16,0" ToolTip="Resolve Conflicts">
                    <controls:SvgIcon Data="{StaticResource Svg.AlertTriangle}" Width="16" Height="16" Foreground="#EAB308"/>
                </Button>
                <!-- Sync Button -->"""

if "OpenConflictResolverCommand" not in content:
    content = content.replace("<!-- Sync Button -->", conflict_button)

with open('src/YAERP.UI/Views/MainWindow.xaml', 'w') as f:
    f.write(content)
