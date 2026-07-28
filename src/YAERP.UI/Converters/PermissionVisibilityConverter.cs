using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using YAERP.Domain.Common;

namespace YAERP.UI.Converters;

public class PermissionVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is string requiredPermission)
        {
            if (SecurityContext.CurrentUser.Permissions.Contains(requiredPermission))
            {
                return Visibility.Visible;
            }
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
