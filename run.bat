@echo off
REM Lance l'application desktop (admin requis pour le log Security).
set EXE=%~dp0dist\WinLogAnalyzer.exe
if not exist "%EXE%" (
  echo [ERREUR] %EXE% introuvable. Lance d'abord build.bat
  exit /b 1
)
echo [ETAT] Demarrage de WinLog Analyzer...
powershell -Command "Start-Process '%EXE%' -Verb RunAs"
