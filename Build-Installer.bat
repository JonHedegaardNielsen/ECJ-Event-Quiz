@echo off
setlocal EnableExtensions EnableDelayedExpansion
cd /d "%~dp0"

set "PUBLISH_DIR=%~dp0artifacts\publish\win-x64"
set "DIST_DIR=%~dp0dist"
set "SPA_DIR=%~dp0ECJ.EventQuiz.ServerUI\wwwroot\app"
set "ISCC=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"
set "SETUP_EXE=%DIST_DIR%\ECJ-EventQuiz-Setup.exe"

echo === ECJ EventQuiz - Byg installer ===
echo.

where dotnet >nul 2>&1
if errorlevel 1 goto :no_dotnet

where npm >nul 2>&1
if errorlevel 1 goto :no_npm

if not exist "!ISCC!" goto :no_iscc

echo [1/4] Bygger frontend...
pushd FrontEnd
call npm install
if errorlevel 1 (
  popd
  echo npm install fejlede.
  goto :fail
)
call npm run build
if errorlevel 1 (
  popd
  echo Frontend-build fejlede.
  goto :fail
)
popd

echo.
echo [2/4] Kopierer SPA til ServerUI...
if exist "%SPA_DIR%" rmdir /s /q "%SPA_DIR%"
mkdir "%SPA_DIR%"
xcopy /e /i /y "FrontEnd\dist\*" "%SPA_DIR%\" >nul
if errorlevel 1 (
  echo Kunne ikke kopiere frontend-build.
  goto :fail
)

echo.
echo [3/4] Publicerer self-contained win-x64...
if exist "%PUBLISH_DIR%" rmdir /s /q "%PUBLISH_DIR%"
dotnet publish "ECJ.EventQuiz.ServerUI\ECJ.EventQuiz.ServerUI.csproj" -c Release -r win-x64 --self-contained true -o "%PUBLISH_DIR%" -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true
if errorlevel 1 (
  echo Publish fejlede.
  goto :fail
)

if not exist "%PUBLISH_DIR%\ECJ.EventQuiz.ServerUI.exe" (
  echo Publish-output mangler ECJ.EventQuiz.ServerUI.exe
  goto :fail
)

if not exist "%PUBLISH_DIR%\wwwroot\app\index.html" (
  echo SPA mangler i publish-output. Tjek wwwroot\app.
  goto :fail
)

echo.
echo [4/4] Bygger Inno Setup-installer...
if not exist "%DIST_DIR%" mkdir "%DIST_DIR%"
"!ISCC!" /DPublishDir="%PUBLISH_DIR%" /DOutputDir="%DIST_DIR%" "Installer\EventQuiz.iss"
if errorlevel 1 (
  echo Inno Setup fejlede.
  goto :fail
)

if not exist "%SETUP_EXE%" (
  echo Setup-filen blev ikke oprettet.
  goto :fail
)

echo.
echo Faerdig.
echo Installer: %SETUP_EXE%
echo.
explorer.exe "%DIST_DIR%"
pause
exit /b 0

:no_dotnet
echo .NET SDK mangler. Installer .NET 10 SDK.
goto :fail

:no_npm
echo Node.js/npm mangler. Installer Node.js.
goto :fail

:no_iscc
echo Inno Setup 6 mangler.
echo Forventet sti: !ISCC!
echo Installer fra https://jrsoftware.org/isinfo.php
goto :fail

:fail
echo.
echo Byg fejlede.
pause
exit /b 1
