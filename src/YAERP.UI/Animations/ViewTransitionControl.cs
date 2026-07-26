using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace YAERP.UI.Animations;

public class ViewTransitionControl : ContentControl
{
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);

        if (newContent != null && VisualTreeHelper.GetChildrenCount(this) > 0)
        {
            var contentPresenter = VisualTreeHelper.GetChild(this, 0) as FrameworkElement;
            if (contentPresenter != null)
            {
                var transform = new TranslateTransform(0, 15);
                contentPresenter.RenderTransform = transform;
                contentPresenter.Opacity = 0;

                var opacityAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                var translateAnimation = new DoubleAnimation(15, 0, TimeSpan.FromMilliseconds(250))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                contentPresenter.BeginAnimation(OpacityProperty, opacityAnimation);
                transform.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
            }
        }
    }
}
