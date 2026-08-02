using System;

namespace YAERP.UI.Services;

public class ThemeService : IThemeService
{
    public AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    public void SetTheme(AppTheme theme)
    {
        CurrentTheme = theme;
        // Broadcast theme change across System.Windows.Application resources
        if (System.Windows.Application.Current != null)
        {
            var res = System.Windows.Application.Current.Resources;
            if (theme == AppTheme.Dark)
            {
                res["BackgroundCanvasBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(15, 23, 42));
                res["ModernCardBackgroundBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 41, 59));
                res["TextPrimaryBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(248, 250, 252));
                res["TextSecondaryBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(148, 163, 184));
            }
            else
            {
                res["BackgroundCanvasBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(248, 250, 252));
                res["ModernCardBackgroundBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                res["TextPrimaryBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(15, 23, 42));
                res["TextSecondaryBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 116, 139));
            }
        }
    }

    public void ToggleTheme()
    {
        SetTheme(CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light);
    }
}
