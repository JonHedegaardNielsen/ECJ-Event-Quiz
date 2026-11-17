@echo off
setlocal EnableExtensions EnableDelayedExpansion
cd /d "%~dp0"

set "SPA_DIR=%~dp0ECJ.EventQuiz.ServerUI\wwwroot\app"
set "EXE=%~dp0ECJ.EventQuiz.ServerUI\bin\Debug\net10.0-windows\ECJ.EventQuiz.ServerUI.exe"

echo === ECJ EventQuiz - Start ===
echo Bygger og starter ServerUI som den installerede app.
echo.

where dotnet >nul 2>&1
if errorlevel 1 goto :no_dotnet

where npm >nul 2>&1
if errorlevel 1 goto :no_npm

echo [1/3] Bygger frontend...
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
echo [2/3] Kopierer SPA til ServerUI...
if exist "%SPA_DIR%" rmdir /s /q "%SPA_DIR%"
mkdir "%SPA_DIR%"
xcopy /e /i /y "FrontEnd\dist\*" "%SPA_DIR%\" >nul
if errorlevel 1 (
  echo Kunne ikke kopiere frontend-build.
  goto :fail
)

echo.
echo [3/3] Bygger ServerUI...
dotnet build "ECJ.EventQuiz.ServerUI\ECJ.EventQuiz.ServerUI.csproj" -c Debug
if errorlevel 1 goto :fail

if not exist "%EXE%" (
  echo Kunne ikke finde: %EXE%
  goto :fail
)

echo.
echo Starter ECJ EventQuiz ServerUI...
echo Splash vises foerst. Tryk Start server i vinduet, derefter Aabn quiz.
echo Login: demo / demo123
echo.
start "" "%EXE%"
exit /b 0

:no_dotnet
echo .NET SDK mangler. Installer .NET 10 SDK.
goto :fail

:no_npm
echo Node.js/npm mangler. Installer Node.js.
goto :fail

:fail
echo.
echo Start fejlede.
pause
exit /b 1
