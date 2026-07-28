with open('tests/YAERP.UI.Tests/ViewModels/Drawers/ConflictResolverDrawerViewModelTests.cs', 'r') as f:
    content = f.read()

# Make sure we don't have blocking task ops, but here we just use async/await
# They already use async Task for tests, so they should be good.
