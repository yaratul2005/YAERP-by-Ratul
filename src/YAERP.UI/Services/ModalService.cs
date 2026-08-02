using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace YAERP.UI.Services;

public class ModalService : ObservableObject, IModalService
{
    private readonly IServiceProvider _serviceProvider;
    private bool _isModalOpen;
    private string _modalTitle = string.Empty;
    private ObservableObject? _currentModalContent;

    public ModalService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool IsModalOpen
    {
        get => _isModalOpen;
        private set => SetProperty(ref _isModalOpen, value);
    }

    public string ModalTitle
    {
        get => _modalTitle;
        private set => SetProperty(ref _modalTitle, value);
    }

    public ObservableObject? CurrentModalContent
    {
        get => _currentModalContent;
        private set => SetProperty(ref _currentModalContent, value);
    }

    public void OpenModal<TViewModel>(string title) where TViewModel : ObservableObject
    {
        ModalTitle = title;
        CurrentModalContent = _serviceProvider.GetRequiredService<TViewModel>();
        IsModalOpen = true;
    }

    public void OpenModal(string title, ObservableObject contentViewModel)
    {
        ModalTitle = title;
        CurrentModalContent = contentViewModel;
        IsModalOpen = true;
    }

    public void CloseModal()
    {
        IsModalOpen = false;
        CurrentModalContent = null;
    }
}
