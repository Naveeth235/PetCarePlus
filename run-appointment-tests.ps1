# Test Runner for PetCare Appointment Booking Feature
# This script runs all unit tests and integration tests for the appointment booking functionality

Write-Host "🧪 Running PetCare Appointment Booking Tests" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# Function to print colored output
function Write-Status {
    param(
        [int]$ExitCode,
        [string]$Message
    )
    
    if ($ExitCode -eq 0) {
        Write-Host "✅ $Message" -ForegroundColor Green
    } else {
        Write-Host "❌ $Message" -ForegroundColor Red
    }
}

function Write-Info {
    param([string]$Message)
    Write-Host "📋 $Message" -ForegroundColor Yellow
}

# Navigate to backend directory
$originalLocation = Get-Location
$backendPath = Join-Path $PSScriptRoot "backend"
Set-Location $backendPath

# Test counter
$totalTests = 0
$passedTests = 0

Write-Host ""
Write-Info "Running Domain Tests (Appointment Model and Status)..."
dotnet test "tests/PetCare.Domain.Tests/PetCare.Domain.Tests.csproj" --filter "Appointment" --verbosity quiet --nologo
$domainExitCode = $LASTEXITCODE
Write-Status $domainExitCode "Domain Tests"
$totalTests++
if ($domainExitCode -eq 0) { $passedTests++ }

Write-Host ""
Write-Info "Running Application Tests (DTOs and Validation)..."
dotnet test "tests/PetCare.Application.Tests/PetCare.Application.Tests.csproj" --filter "Appointment" --verbosity quiet --nologo
$appExitCode = $LASTEXITCODE
Write-Status $appExitCode "Application Tests"
$totalTests++
if ($appExitCode -eq 0) { $passedTests++ }

Write-Host ""
Write-Info "Running Infrastructure Tests (NotificationService)..."
dotnet test "tests/PetCare.Infrastructure.Tests/PetCare.Infrastructure.Tests.csproj" --filter "Notification" --verbosity quiet --nologo
$infraExitCode = $LASTEXITCODE
Write-Status $infraExitCode "Infrastructure Tests"
$totalTests++
if ($infraExitCode -eq 0) { $passedTests++ }

Write-Host ""
Write-Info "Running API Tests (AppointmentsController)..."
dotnet test "tests/PetCare.Api.Tests/PetCare.Api.Tests.csproj" --filter "Appointments" --verbosity quiet --nologo
$apiExitCode = $LASTEXITCODE
Write-Status $apiExitCode "API Controller Tests"
$totalTests++
if ($apiExitCode -eq 0) { $passedTests++ }

Write-Host ""
Write-Info "Running Integration Tests (End-to-End)..."
dotnet test "tests/PetCare.Integration.Tests/PetCare.Integration.Tests.csproj" --filter "Appointments" --verbosity quiet --nologo
$integrationExitCode = $LASTEXITCODE
Write-Status $integrationExitCode "Integration Tests"
$totalTests++
if ($integrationExitCode -eq 0) { $passedTests++ }

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "📊 TEST SUMMARY" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "Total Test Suites: $totalTests"
Write-Host "Passed: $passedTests" -ForegroundColor Green
Write-Host "Failed: $($totalTests - $passedTests)" -ForegroundColor Red

# Return to original location
Set-Location $originalLocation

if ($passedTests -eq $totalTests) {
    Write-Host "🎉 All appointment booking tests passed!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "💥 Some tests failed. Please review the output above." -ForegroundColor Red
    exit 1
}