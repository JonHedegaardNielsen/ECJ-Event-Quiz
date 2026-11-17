@echo off
REM Alias - samme flow som Start-All.bat (én ServerUI-app)
cd /d "%~dp0"
call "%~dp0Start-All.bat"
exit /b %ERRORLEVEL%
