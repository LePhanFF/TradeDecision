@echo off
setlocal
set "DEST=%USERPROFILE%\Documents\NinjaTrader 8\bin\Custom"
set "ADDON=%DEST%\AddOns\TPOAddon"
set "SRC=%~dp0src"
if not exist "%ADDON%" mkdir "%ADDON%"
copy /y "%SRC%\AddOn\*.cs" "%ADDON%\" >nul
copy /y "instruments.txt" "%ADDON%\" >nul
copy /y "%SRC%\Core\*.cs"  "%ADDON%\" >nul
copy /y "%SRC%\Ui\*.cs"    "%ADDON%\" >nul
echo ✅ Installed to: %ADDON%
echo Open NinjaTrader -> NinjaScript Editor -> F5 to compile.
endlocal
