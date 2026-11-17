; ECJ EventQuiz - Inno Setup 6 script (UTF-8 med BOM)
; Build with Build-Installer.bat (produces dist\ECJ-EventQuiz-Setup.exe)

#define MyAppName "ECJ EventQuiz"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "ECJ"
#define MyAppExeName "ECJ.EventQuiz.ServerUI.exe"
#define MyAppId "ECJ.EventQuiz.ServerUI"

#ifndef PublishDir
  #define PublishDir "..\artifacts\publish\win-x64"
#endif

#ifndef OutputDir
  #define OutputDir "..\dist"
#endif

[Setup]
AppId={{A8F3C2E1-9B47-4D6A-8E21-7C5F0D91B3A4}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename=ECJ-EventQuiz-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
SetupIconFile=..\ECJ.EventQuiz.ServerUI\Assets\AppIcon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "danish"; MessagesFile: "compiler:Languages\Danish.isl"

[Tasks]
Name: "desktopicon"; Description: "Opret genvej på skrivebordet"; GroupDescription: "Ekstra genveje:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Afinstaller {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Start {#MyAppName}"; Flags: nowait postinstall skipifsilent

[Code]
function WebView2RuntimeInstalled: Boolean;
begin
  Result :=
    RegKeyExists(HKLM64, 'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}') or
    RegKeyExists(HKLM64, 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}') or
    RegKeyExists(HKCU, 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}');
end;

function InitializeSetup: Boolean;
begin
  Result := True;
  if not WebView2RuntimeInstalled then
  begin
    MsgBox(
      'Microsoft Edge WebView2 Runtime ser ikke ud til at være installeret.' + #13#10 + #13#10 +
      'Windows 11 har den normalt allerede. Hvis programmet ikke kan starte efter installation,' + #13#10 +
      'installer WebView2 Evergreen Runtime fra Microsoft.',
      mbInformation, MB_OK);
  end;
end;