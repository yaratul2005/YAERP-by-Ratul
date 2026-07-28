using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces;

public interface IDialogService
{
    Task ShowGlobalToastAsync(string message);
    Task ShowDrawerAsync(string title, object content);
    Task ShowModalAsync(string title, object content);
    void CloseActiveOverlay();
}
