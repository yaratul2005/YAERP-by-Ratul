using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YAERP.UI.Services;

public interface IModalService
{
    bool IsModalOpen { get; }
    string ModalTitle { get; }
    ObservableObject? CurrentModalContent { get; }
    void OpenModal<TViewModel>(string title) where TViewModel : ObservableObject;
    void CloseModal();
}
