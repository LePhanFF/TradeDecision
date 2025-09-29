param(
  [string]$Path = "."
)
$ErrorActionPreference = "Stop"
$bad = @(
  @{ Name="Target-typed new()"; Pattern="\bnew\(\)"; Hint="Use explicit types: new List<T>()" },
  @{ Name="Index-from-end ^"; Pattern="\^\s*\d+"; Hint="Replace with Count-1 math" },
  @{ Name="Range operator .."; Pattern="\.\."; Hint="Avoid range operator; use loops" },
  @{ Name="record type"; Pattern="\brecord\s+"; Hint="Use class/struct" },
  @{ Name="System.Text.Json"; Pattern="System\.Text\.Json"; Hint="Use StringBuilder for JSON" },
  @{ Name="Named tuples"; Pattern="List<\s*\("; Hint="Use small structs/classes" },
  @{ Name="ValueTuple"; Pattern="ValueTuple"; Hint="Use small structs/classes" }
)

$files = Get-ChildItem -Path $Path -Recurse -Include *.cs
$fail = $false
foreach ($f in $files) {
  $content = Get-Content -Raw -LiteralPath $f.FullName
  foreach ($rule in $bad) {
    if ($content -match $rule.Pattern -and $content -notmatch "nt8lint:\s*ignore") {
      Write-Host "❌ $($rule.Name) in $($f.FullName)" -ForegroundColor Red
      Write-Host "   Hint: $($rule.Hint)"
      $fail = $true
    }
  }
}
if ($fail) { exit 1 } else { Write-Host "✅ NT8 lint passed" -ForegroundColor Green }
