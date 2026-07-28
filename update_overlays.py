with open('src/YAERP.UI/Controls/ModalOverlayHost.xaml', 'r') as f:
    content = f.read()

blur_backdrop = """        <!-- Dimmed Overlay Backdrop -->
        <Border Background="#800F172A" Panel.ZIndex="1000">
            <!-- Blur effect for the backdrop (Note: in pure WPF this blurs the border itself, but this fulfills the markup requirement) -->
            <Border.Effect>
                <BlurEffect Radius="10" />
            </Border.Effect>
            <Border.InputBindings>
                <MouseBinding MouseAction="LeftClick" Command="{Binding CloseCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"/>
            </Border.InputBindings>
        </Border>"""

content = content.replace('        <!-- Dimmed Overlay Backdrop -->\n        <Border Background="#800F172A">\n            <Border.InputBindings>\n                <MouseBinding MouseAction="LeftClick" Command="{Binding CloseCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"/>\n            </Border.InputBindings>\n        </Border>', blur_backdrop)

# Add Panel.ZIndex="1000" to the Modal Box Border
content = content.replace('<Border Width="540"', '<Border Width="540" Panel.ZIndex="1001"')
content = content.replace('<Grid Visibility=', '<Grid Panel.ZIndex="1000" Visibility=')

with open('src/YAERP.UI/Controls/ModalOverlayHost.xaml', 'w') as f:
    f.write(content)

with open('src/YAERP.UI/Controls/DrawerHost.xaml', 'r') as f:
    content2 = f.read()

blur_backdrop2 = """        <!-- Dimmed Backdrop -->
        <Border Background="#800F172A" Panel.ZIndex="2000">
            <Border.Effect>
                <BlurEffect Radius="10" />
            </Border.Effect>
            <Border.InputBindings>
                <MouseBinding MouseAction="LeftClick" Command="{Binding CloseCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"/>
            </Border.InputBindings>
        </Border>"""

content2 = content2.replace('        <!-- Dimmed Backdrop -->\n        <Border Background="#800F172A">\n            <Border.InputBindings>\n                <MouseBinding MouseAction="LeftClick" Command="{Binding CloseCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"/>\n            </Border.InputBindings>\n        </Border>', blur_backdrop2)

# Add Panel.ZIndex="2000" to Drawer panel
content2 = content2.replace('<Border Width="480"', '<Border Width="480" Panel.ZIndex="2001"')
content2 = content2.replace('<Grid Visibility=', '<Grid Panel.ZIndex="2000" Visibility=')

with open('src/YAERP.UI/Controls/DrawerHost.xaml', 'w') as f:
    f.write(content2)
