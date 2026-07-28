with open('src/YAERP.UI/Views/MainWindow.xaml', 'r') as f:
    content = f.read()

update_banner = """
        <!-- Update Notification Banner -->
        <Border Grid.Row="0" Background="#3B82F6" Visibility="{Binding IsUpdateAvailable, Converter={StaticResource BoolToVis}}" Padding="16,8">
            <DockPanel>
                <StackPanel Orientation="Horizontal" DockPanel.Dock="Right" Visibility="{Binding IsDownloadingUpdate, Converter={StaticResource InverseBoolToVis}}">
                    <Button Content="Update Now" Command="{Binding DownloadUpdateCommand}" Style="{DynamicResource PrimaryButtonStyle}"
                            Background="White" Foreground="#1D4ED8" Padding="12,4"/>
                </StackPanel>

                <ProgressBar DockPanel.Dock="Right" Width="150" Height="8" Minimum="0" Maximum="100"
                             Value="{Binding UpdateDownloadProgress}" Margin="0,0,16,0"
                             Visibility="{Binding IsDownloadingUpdate, Converter={StaticResource BoolToVis}}"/>

                <TextBlock Text="{Binding UpdateBannerText}" Foreground="White" FontWeight="SemiBold" VerticalAlignment="Center"/>
            </DockPanel>
        </Border>
"""

if "Update Notification Banner" not in content:
    content = content.replace("<Grid.RowDefinitions>\n            <RowDefinition Height=\"Auto\"/>", "<Grid.RowDefinitions>\n            <RowDefinition Height=\"Auto\"/>\n            <RowDefinition Height=\"Auto\"/>")
    content = content.replace("<!-- Title Bar -->\n        <Border Grid.Row=\"0\"", update_banner + "\n        <!-- Title Bar -->\n        <Border Grid.Row=\"1\"")
    # Shift rows
    content = content.replace('Grid.Row="1"', 'Grid.Row="2"', 1) # This targets the main content area

with open('src/YAERP.UI/Views/MainWindow.xaml', 'w') as f:
    f.write(content)
