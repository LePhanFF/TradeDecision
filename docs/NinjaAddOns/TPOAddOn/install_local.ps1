$Dest = Join-Path $env:USERPROFILE "Documents\NinjaTrader 8\bin\Custom"
$Addon = Join-Path $Dest "AddOns\TPOAddon"
Write-Host "Installing to $Addon"

New-Item -ItemType Directory -Path $Addon -Force | Out-Null
Copy-Item "$PSScriptRoot\src\AddOn\*.cs" $Addon -Force
Copy-Item "$PSScriptRoot\src\Core\*.cs"  $Addon -Force
Copy-Item "$PSScriptRoot\src\Ui\*.cs"    $Addon -Force

New-Item -ItemType Directory -Path (Join-Path $Dest "TPOAddon_Docs") -Force | Out-Null
if (Test-Path "$PSScriptRoot\README.md")   { Copy-Item "$PSScriptRoot\README.md"   (Join-Path $Dest "TPOAddon_Docs") -Force }
if (Test-Path "$PSScriptRoot\LICENSE.txt") { Copy-Item "$PSScriptRoot\LICENSE.txt" (Join-Path $Dest "TPOAddon_Docs") -Force }
if (Test-Path "$PSScriptRoot\ROADMAP.md")  { Copy-Item "$PSScriptRoot\ROADMAP.md"  (Join-Path $Dest "TPOAddon_Docs") -Force }

Write-Host ""
Write-Host "✅ Files copied."
Write-Host "Next:"
Write-Host "1) Open NinjaTrader 8"
Write-Host "2) New -> NinjaScript Editor"
Write-Host "3) Press F5 to compile"
Write-Host "4) Control Center: New -> NinjaAddOn TPO v9.4.3"
