@echo off
setlocal
set "HERE=%~dp0"
powershell -ExecutionPolicy Bypass -File "%HERE%\nt8-lint.ps1" "%CD%"
endlocal
