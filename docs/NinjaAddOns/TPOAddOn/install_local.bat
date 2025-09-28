@echo off
setlocal

rem --------- Paths (safe quoting) ----------
set "DEST=%USERPROFILE%\Documents\NinjaTrader 8\bin\Custom"
set "ADDON=%DEST%\AddOns\TPOAddon"
set "SRC=%~dp0src"

echo DEST = %DEST%
echo ADDON = %ADDON%
echo SRC   = %SRC%
echo.

rem --------- Create targets ----------
if not exist "%ADDON%" mkdir "%ADDON%"
if not exist "%DEST%\TPOAddon_Docs" mkdir "%DEST%\TPOAddon_Docs"

rem --------- Copy sources ----------
copy /y "%SRC%\AddOn\*.cs" "%ADDON%\" >nul
copy /y "%SRC%\Core\*.cs"  "%ADDON%\" >nul
copy /y "%SRC%\Ui\*.cs"    "%ADDON%\" >nul

rem --------- Copy docs (optional) ----------
if exist "%~dp0README.md"   copy /y "%~dp0README.md"   "%DEST%\TPOAddon_Docs\" >nul
if exist "%~dp0LICENSE.txt" copy /y "%~dp0LICENSE.txt" "%DEST%\TPOAddon_Docs\" >nul
if exist "%~dp0ROADMAP.md"  copy /y "%~dp0ROADMAP.md"  "%DEST%\TPOAddon_Docs\" >nul

echo.
echo ✅ Installed to: %ADDON%
echo Next:
echo   1) Open NinjaTrader 8
echo   2) New -> NinjaScript Editor -> press F5 to compile
echo   3) Control Center -> New -> "NinjaAddOn TPO v9.4.3"
echo.
endlocal
