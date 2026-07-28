import re

with open('src/YAERP.UI/ViewModels/MainViewModel.cs', 'r') as f:
    content = f.read()

# Update CheckSyncStatusAsync and manual sync to include IDialogService logic if needed,
# but mostly we just need the trigger for ConflictResolverDrawer
# We will inject IDialogService and add OpenConflictResolverCommand

new_fields = """    private readonly IDialogService _dialogService;
"""

if "_dialogService" not in content:
    content = content.replace("private readonly IModalService _modalService;", "private readonly IModalService _modalService;\n    private readonly IDialogService _dialogService;")
    content = content.replace("IModalService modalService,", "IModalService modalService,\n        IDialogService dialogService,")
    content = content.replace("_modalService = modalService;", "_modalService = modalService;\n        _dialogService = dialogService;")

conflict_command = """
    [RelayCommand]
    private async Task OpenConflictResolverAsync()
    {
        await _dialogService.ShowDrawerAsync("Conflict Resolver", "ConflictResolverDrawerViewModel");
    }
"""

if "OpenConflictResolverAsync" not in content:
    content = content.replace("private async Task ManualSyncNowAsync()", conflict_command + "\n    [RelayCommand]\n    private async Task ManualSyncNowAsync()")

with open('src/YAERP.UI/ViewModels/MainViewModel.cs', 'w') as f:
    f.write(content)
