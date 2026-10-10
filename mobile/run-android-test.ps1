# Run MOVOQ on Android against the local API (Development).
# Usage: .\run-android-test.ps1
# Physical phone on Wi-Fi: .\run-android-test.ps1 -ApiHost 192.168.1.10

param(
    [string]$ApiHost = "",
    [string]$Emulator = "Pixel_6"
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

$apiUrl = if ($ApiHost) { "http://${ApiHost}:5014" } else { "http://10.0.2.2:5014" }

Write-Host "MOVOQ mobile — Android test run" -ForegroundColor Cyan
Write-Host "API target: $apiUrl"
Write-Host ""

Write-Host "Checking backend..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "http://localhost:5014/health" -TimeoutSec 5
    Write-Host "  API OK ($($health.status)) on http://localhost:5014" -ForegroundColor Green
} catch {
    Write-Host "  Start the API first:" -ForegroundColor Red
    Write-Host "  cd e:\MOVOQ\backend"
    Write-Host '  $env:ASPNETCORE_ENVIRONMENT="Development"'
    Write-Host "  dotnet run --project MOVOQ.Api --launch-profile http"
    exit 1
}

Write-Host "Installing Flutter packages..." -ForegroundColor Yellow
flutter pub get | Out-Null

$android = flutter devices 2>&1 | Select-String "android"
if (-not $android) {
    Write-Host "No Android device — launching $Emulator emulator..." -ForegroundColor Yellow
    flutter emulators --launch $Emulator
    Write-Host "Waiting for emulator (up to 90s)..." -ForegroundColor Yellow
    $deadline = (Get-Date).AddSeconds(90)
    do {
        Start-Sleep -Seconds 5
        $android = flutter devices 2>&1 | Select-String "emulator"
    } while (-not $android -and (Get-Date) -lt $deadline)
    if (-not $android) {
        Write-Host "Emulator did not appear. Open Android Studio > Device Manager and start one manually." -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "Sign in: tap Continue without signing in (local dev)." -ForegroundColor Cyan
Write-Host "Home: use +1,000 to simulate steps on emulator." -ForegroundColor Cyan
Write-Host "Profile: switch between seeded fake users." -ForegroundColor Cyan
Write-Host ""

$args = @("run", "-d", "android")
if ($ApiHost) {
    $args += @("--dart-define=API_BASE_URL=$apiUrl")
}

flutter @args
