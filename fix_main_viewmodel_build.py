with open('src/YAERP.UI/ViewModels/MainViewModel.cs', 'r') as f:
    content = f.read()

import re

# Remove duplicate relay command
content = re.sub(r'\[RelayCommand\]\n\s*\[RelayCommand\]', '[RelayCommand]', content)

with open('src/YAERP.UI/ViewModels/MainViewModel.cs', 'w') as f:
    f.write(content)
