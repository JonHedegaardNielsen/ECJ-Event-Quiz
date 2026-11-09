@echo off
setlocal
cd /d "%~dp0"

echo Building ECJ.EventQuiz.ServerUI...
dotnet build "ECJ.EventQuiz.ServerUI\ECJ.EventQuiz.ServerUI.csproj" -c Debug
if errorlevel 1 (
  echo.
  echo Build failed.
  pause
  exit /b 1
)

echo.
echo Starting ServerUI...
dotnet run --project "ECJ.EventQuiz.ServerUI\ECJ.EventQuiz.ServerUI.csproj" -c Debug --no-build

if errorlevel 1 (
  echo.
  echo ServerUI exited with an error.
  pause
  exit /b 1
)
