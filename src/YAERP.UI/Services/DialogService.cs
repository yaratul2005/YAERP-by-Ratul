using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using YAERP.Application.Common.Interfaces;

namespace YAERP.UI.Services;

public class DialogService : IDialogService
{
    private readonly Stack<DialogOverlay> _overlayStack = new();

    public enum OverlayLevel
    {
        Modal = 1000,
        Drawer = 2000,
        Toast = 3000
    }

    private class DialogOverlay
    {
        public OverlayLevel Level { get; init; }
        public Action CloseAction { get; init; } = () => { };
    }

    public DialogService()
    {
        // Global escape binding requires a way to hook into the main window or global input
        // For WPF, InputManager is often used or hooking to MainWindow.PreviewKeyDown
        EventManager.RegisterClassHandler(typeof(Window), UIElement.PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown));
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (_overlayStack.Any())
            {
                CloseActiveOverlay();
                e.Handled = true;
            }
        }
    }

    public Task ShowGlobalToastAsync(string message)
    {
        // Implementation for Toast Z-3000
        var overlay = new DialogOverlay
        {
            Level = OverlayLevel.Toast,
            CloseAction = () => { /* Close Toast logic */ }
        };
        _overlayStack.Push(overlay);
        return Task.CompletedTask;
    }

    public Task ShowDrawerAsync(string title, object content)
    {
        // Implementation for Drawer Z-2000
        var overlay = new DialogOverlay
        {
            Level = OverlayLevel.Drawer,
            CloseAction = () => { /* Close Drawer logic */ }
        };
        _overlayStack.Push(overlay);
        return Task.CompletedTask;
    }

    public Task ShowModalAsync(string title, object content)
    {
        // Implementation for Modal Z-1000
        var overlay = new DialogOverlay
        {
            Level = OverlayLevel.Modal,
            CloseAction = () => { /* Close Modal logic */ }
        };
        _overlayStack.Push(overlay);
        return Task.CompletedTask;
    }

    public void CloseActiveOverlay()
    {
        if (_overlayStack.TryPop(out var overlay))
        {
            overlay.CloseAction();
        }
    }
}
