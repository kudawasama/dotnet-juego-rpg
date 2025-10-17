Param(
    [int]$Samples = 1000
)

$timestamp = Get-Date -Format "yyyy-MM-dd_HHmmss"
$logsDir = Join-Path -Path (Get-Location) -ChildPath "logs"
if (!(Test-Path $logsDir)) { New-Item -ItemType Directory -Path $logsDir | Out-Null }
$logFile = Join-Path $logsDir ("shadow_benchmark_{0}.txt" -f $timestamp)

Write-Host "Running shadow benchmark with $Samples samples..."
Write-Host "Log: $logFile"

dotnet run --project "MiJuegoRPG\MiJuegoRPG.csproj" --no-build -- --shadow-benchmark=$Samples > $logFile

if (Test-Path $logFile) {
    Write-Host "WROTE: $logFile"
} else {
    Write-Host "ERROR: Log file was not created: $logFile"
    exit 1
}
