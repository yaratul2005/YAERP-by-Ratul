[Setup]
AppName=YAERP by Ratul
AppVersion=1.0.0
AppPublisher=Ratul
DefaultDirName={autopf}\YAERP
DefaultGroupName=YAERP
OutputDir=..\dist
OutputBaseFilename=YAERP-v1.0-win-x64-Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "..\src\YAERP.UI\bin\Release\net8.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\YAERP"; Filename: "{app}\YAERP.UI.exe"
Name: "{autodesktop}\YAERP"; Filename: "{app}\YAERP.UI.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
