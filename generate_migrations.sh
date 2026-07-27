#!/bin/bash
# Temporarily remove WPF from UI project so EF can run in linux sandbox
sed -i 's/<TargetFramework>net10.0-windows<\/TargetFramework>/<TargetFramework>net10.0<\/TargetFramework>/g' src/YAERP.UI/YAERP.UI.csproj
sed -i 's/<UseWPF>true<\/UseWPF>//g' src/YAERP.UI/YAERP.UI.csproj

dotnet ef migrations add PhaseB_Manufacturing -p src/YAERP.Infrastructure/YAERP.Infrastructure.csproj -s src/YAERP.UI/YAERP.UI.csproj

# Revert WPF settings
git checkout src/YAERP.UI/YAERP.UI.csproj
