using System.Collections.Generic;
using System.Windows.Input;

namespace YAERP.UI.Services;

public record PaletteCommandItem(
    string Title,
    string Subtitle,
    string Category,
    string IconKey,
    ICommand ExecutionCommand,
    KeyGesture? Shortcut = null);

public interface ICommandPaletteService
{
    void RegisterCommand(PaletteCommandItem item);
    void UnregisterCommand(PaletteCommandItem item);
    IReadOnlyList<PaletteCommandItem> Search(string query);
    IReadOnlyList<PaletteCommandItem> GetAllCommands();
}
