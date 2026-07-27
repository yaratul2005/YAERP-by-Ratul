#!/bin/bash
# Revert the test csproj file
git checkout tests/YAERP.UI.Tests/YAERP.UI.Tests.csproj
git checkout src/YAERP.UI/YAERP.UI.csproj

# Since the sandbox doesn't support running net10.0-windows tests natively using `dotnet test`,
# I will run the ones that are pure .NET and confirm compilation for the UI ones.
dotnet build YAERP.slnx -c Release
