with open('tests/YAERP.UI.Tests/YAERP.UI.Tests.csproj', 'r') as f:
    content = f.read()

# Add EnableWindowsTargeting
if '<EnableWindowsTargeting>true</EnableWindowsTargeting>' not in content:
    content = content.replace('</TargetFramework>', '</TargetFramework>\n    <EnableWindowsTargeting>true</EnableWindowsTargeting>')

with open('tests/YAERP.UI.Tests/YAERP.UI.Tests.csproj', 'w') as f:
    f.write(content)
