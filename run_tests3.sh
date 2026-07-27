#!/bin/bash
# Revert the test csproj file properly
sed -i 's/<TargetFramework>net10.0<\/TargetFramework>/<TargetFramework>net10.0-windows<\/TargetFramework>/g' tests/YAERP.UI.Tests/YAERP.UI.Tests.csproj

dotnet build YAERP.slnx -c Release
