@echo off
setlocal

set "SRC=%~dp0src"
set "DIST=%~dp0dist"
set "TMP=%~dp0_tmp_ninja"

if exist "%TMP%" rmdir /s /q "%TMP%"
if not exist "%DIST%" mkdir "%DIST%"

mkdir "%TMP%\bin\Custom\AddOns\TPOAddon"
mkdir "%TMP%\bin\Custom\TPOAddon_Docs"

copy "%SRC%\AddOn\*.cs" "%TMP%\bin\Custom\AddOns\TPOAddon\" >nul
copy "%SRC%\Core\*.cs"  "%TMP%\bin\Custom\AddOns\TPOAddon\" >nul
copy "%SRC%\Ui\*.cs"    "%TMP%\bin\Custom\AddOns\TPOAddon\" >nul

copy "%~dp0README.md"   "%TMP%\bin\Custom\TPOAddon_Docs\" >nul
copy "%~dp0LICENSE.txt" "%TMP%\bin\Custom\TPOAddon_Docs\" >nul
copy "%~dp0ROADMAP.md"  "%TMP%\bin\Custom\TPOAddon_Docs\" >nul

rem Zip with bin as the root of the archive
if exist "%DIST%\TPOAddon_NinjaScriptImport.zip" del "%DIST%\TPOAddon_NinjaScriptImport.zip"
pushd "%TMP%"
powershell -Command "Compress-Archive -Path 'bin' -DestinationPath '%DIST%\TPOAddon_NinjaScriptImport.zip' -Force"
popd

echo.
echo ✅ Built %DIST%\TPOAddon_NinjaScriptImport.zip  (root=bin)
echo NOTE: NinjaTrader may still reject hand-made archives. If so, install locally (install_local.bat), then Tools->Export->NinjaScript Add-On to produce an official import zip.
echo.
endlocal
