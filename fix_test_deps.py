import re

with open('tests/YAERP.UI.Tests/ViewModels/MainViewModelUpdaterTests.cs', 'r') as f:
    content = f.read()

if "using YAERP.UI.Services;" not in content:
    content = content.replace("using YAERP.UI.ViewModels;", "using YAERP.UI.ViewModels;\nusing YAERP.UI.Services;\nusing YAERP.UI.Workspace;")

with open('tests/YAERP.UI.Tests/ViewModels/MainViewModelUpdaterTests.cs', 'w') as f:
    f.write(content)
