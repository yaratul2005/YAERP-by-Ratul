#!/bin/bash
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

dotnet test tests/YAERP.Domain.Tests/YAERP.Domain.Tests.csproj
dotnet test tests/YAERP.Application.Tests/YAERP.Application.Tests.csproj
dotnet test tests/YAERP.Infrastructure.Tests/YAERP.Infrastructure.Tests.csproj
# Skipping UI and Architecture tests as WPF/WindowsDesktop framework isn't available in this linux runner
