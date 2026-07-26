param (
    [string]$OutputDir = "$PSScriptRoot\..\dist"
)

Write-Host "Restoring YAERP.slnx..."
dotnet restore "$PSScriptRoot\..\YAERP.slnx"

Write-Host "Building YAERP.slnx (Release)..."
dotnet build "$PSScriptRoot\..\YAERP.slnx" -c Release --no-restore

Write-Host "Running Tests..."
dotnet test "$PSScriptRoot\..\YAERP.slnx" -c Release --no-build

Write-Host "Publishing YAERP.UI as self-contained standalone executable..."
dotnet publish "$PSScriptRoot\..\src\YAERP.UI\YAERP.UI.csproj" -p:PublishProfile=win-x64-standalone

Write-Host "Copying output to $OutputDir..."
if (!(Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

$PublishPath = "$PSScriptRoot\..\src\YAERP.UI\bin\Release\net8.0\win-x64\publish"
Copy-Item "$PublishPath\*" "$OutputDir\" -Recurse -Force
Rename-Item "$OutputDir\YAERP.UI.exe" "YAERP.exe" -ErrorAction SilentlyContinue

Write-Host "Build and publish complete. Release available at $OutputDir\YAERP.exe" -ForegroundColor Green
