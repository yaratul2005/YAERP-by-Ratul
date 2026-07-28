using System.Windows;
using Xunit;
using YAERP.Domain.Common;
using YAERP.UI.Converters;

namespace YAERP.UI.Tests.Converters;

public class PermissionVisibilityConverterTests
{
    [Fact]
    public void Convert_WithValidPermission_ReturnsVisible()
    {
        SecurityContext.CurrentUser.Permissions.Clear();
        SecurityContext.CurrentUser.Permissions.Add("Inventory.View");

        var converter = new PermissionVisibilityConverter();
        var result = converter.Convert(null, typeof(Visibility), "Inventory.View", null!);

        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WithInvalidPermission_ReturnsCollapsed()
    {
        SecurityContext.CurrentUser.Permissions.Clear();
        SecurityContext.CurrentUser.Permissions.Add("Inventory.View");

        var converter = new PermissionVisibilityConverter();
        var result = converter.Convert(null, typeof(Visibility), "Inventory.Edit", null!);

        Assert.Equal(Visibility.Collapsed, result);
    }
}
