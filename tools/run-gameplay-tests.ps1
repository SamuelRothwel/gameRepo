param(
    [Parameter(Mandatory = $true)]
    [string]$Suite,
    [string]$GodotPath
)

$projectRoot = Split-Path -Parent $PSScriptRoot
$configPath = Join-Path $PSScriptRoot 'godot-path.json'

if ([string]::IsNullOrWhiteSpace($GodotPath) -and (Test-Path -LiteralPath $configPath)) {
    $config = Get-Content -LiteralPath $configPath -Raw | ConvertFrom-Json
    $GodotPath = $config.godotDotNetExecutable
}

if ([string]::IsNullOrWhiteSpace($GodotPath) -or -not (Test-Path -LiteralPath $GodotPath)) {
    Write-Host "Godot .NET executable was not found. Set tools/godot-path.json or pass -GodotPath <path-to-Godot.exe>."
    exit 2
}

# The report is printed by TestManagement with a stable prefix. Codex/CI can
# parse that line, while all normal Godot output remains available for diagnosis.
& $GodotPath --headless --path $projectRoot -- --run-gameplay-tests $Suite
exit $LASTEXITCODE
