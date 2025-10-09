# PetCare Test Report Generator - Simplified Version
param(
    [string]$OutputPath = "petcare-test-report.html",
    [switch]$OpenReport = $false
)

Write-Host "PetCare Test Report Generator" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan

# Test configuration
$testSuites = @(
    @{
        Name = "Domain Tests"
        Description = "Business logic, domain models, and entity validation"
        Path = "tests/PetCare.Domain.Tests/PetCare.Domain.Tests.csproj"
        Filter = "Appointment"
        Icon = ""
        Color = "#2196F3"
    },
    @{
        Name = "Application Tests"
        Description = "DTOs, validation rules, and application services"
        Path = "tests/PetCare.Application.Tests/PetCare.Application.Tests.csproj"
        Filter = "Appointment"
        Icon = ""
        Color = "#4CAF50"
    },
    @{
        Name = "Infrastructure Tests"
        Description = "External services and infrastructure components"
        Path = "tests/PetCare.Infrastructure.Tests/PetCare.Infrastructure.Tests.csproj"
        Filter = "Notification"
        Icon = ""
        Color = "#9C27B0"
    },
    @{
        Name = "API Tests"
        Description = "REST API controllers and HTTP endpoints"
        Path = "tests/PetCare.Api.Tests/PetCare.Api.Tests.csproj"
        Filter = "Appointments"
        Icon = ""
        Color = "#FF5722"
    },
    @{
        Name = "Integration Tests"
        Description = "End-to-end integration tests"
        Path = "tests/PetCare.Integration.Tests"
        Filter = ""
        Icon = ""
        Color = "#FF9800"
    }
)

# Initialize counters
$totalTests = 0
$passedTests = 0
$failedTests = 0
$skippedTests = 0
$suiteResults = @()

Write-Host ""
$startTime = Get-Date

# Run each test suite
foreach ($suite in $testSuites) {
    Write-Host "Running $($suite.Name)..." -ForegroundColor Yellow
    
    try {
        if ([string]::IsNullOrEmpty($suite.Filter)) {
            $testCmd = "dotnet test `"$($suite.Path)`" --verbosity minimal --logger `"console;verbosity=minimal`""
        } else {
            $testCmd = "dotnet test `"$($suite.Path)`" --filter `"$($suite.Filter)`" --verbosity minimal --logger `"console;verbosity=minimal`""
        }
        $output = Invoke-Expression $testCmd 2>&1 | Out-String
        

        
        # Initialize default values
        $passed = 0
        $failed = 0 
        $skipped = 0
        $duration = "0s"
        

        
        # Parse results - VSTest format (for failed tests)
        if ($output -match "Failed!\s+-\s+Failed:\s+(\d+),\s+Passed:\s+(\d+),\s+Skipped:\s+(\d+),\s+Total:\s+(\d+),\s+Duration:\s+([\d\.]+)\s*s") {
            $failed = [int]$matches[1]
            $passed = [int]$matches[2]
            $skipped = [int]$matches[3]
            $total = [int]$matches[4]
            $durationS = [float]$matches[5]
            $duration = "${durationS}s"
        }
        # Parse results - VSTest format (for passed tests)
        elseif ($output -match "Failed:\s+(\d+),\s+Passed:\s+(\d+),\s+Skipped:\s+(\d+),\s+Total:\s+(\d+),\s+Duration:\s+([\d\.]+)\s*ms") {
            $failed = [int]$matches[1]
            $passed = [int]$matches[2]
            $skipped = [int]$matches[3]
            $total = [int]$matches[4]
            $durationMs = [int]$matches[5]
            $duration = if ($durationMs -gt 1000) { "$([math]::Round($durationMs/1000, 1))s" } else { "${durationMs}ms" }
        }
        # Alternative format - Handle both success and failure cases
        elseif ($output -match "Test summary: total: (\d+), failed: (\d+), succeeded: (\d+), skipped: (\d+)") {
            $total = [int]$matches[1]
            $failed = [int]$matches[2]
            $passed = [int]$matches[3]
            $skipped = [int]$matches[4]
        }
        # Handle duration extraction from failed test output
        if ($output -match "Duration: ([\d\.]+)\s*s") {
            $durationS = [float]$matches[1]
            $duration = "${durationS}s"
        }
        elseif ($output -match "Duration: ([\d\.]+)\s*ms") {
            $durationMs = [float]$matches[1]
            $duration = if ($durationMs -gt 1000) { "$([math]::Round($durationMs/1000, 1))s" } else { "${durationMs}ms" }
        }
        
        $status = if ($failed -eq 0) { "PASSED" } else { "FAILED" }
        $statusIcon = if ($failed -eq 0) { "[PASS]" } else { "[FAIL]" }
        
        Write-Host "  $statusIcon $($suite.Name): $passed passed, $failed failed, $skipped skipped ($duration)" -ForegroundColor $(if($failed -eq 0){"Green"}else{"Red"})
        
        # Update totals
        $totalTests += ($passed + $failed + $skipped)
        $passedTests += $passed
        $failedTests += $failed
        $skippedTests += $skipped
        
        # Store results
        $suiteResults += @{
            Name = $suite.Name
            Description = $suite.Description
            Passed = $passed
            Failed = $failed
            Skipped = $skipped
            Total = ($passed + $failed + $skipped)
            Duration = $duration
            Status = $status
            StatusIcon = $statusIcon
        }
    }
    catch {
        Write-Host "  [ERROR] Error running $($suite.Name): $($_.Exception.Message)" -ForegroundColor Red
        $suiteResults += @{
            Name = $suite.Name
            Description = $suite.Description
            Passed = 0
            Failed = 1
            Skipped = 0
            Total = 1
            Duration = "0s"
            Status = "ERROR"
            StatusIcon = "[ERROR]"
        }
        $failedTests += 1
        $totalTests += 1
    }
}

$endTime = Get-Date
$totalDuration = [math]::Round(($endTime - $startTime).TotalSeconds, 1)

# Calculate summary
$overallStatus = if ($failedTests -eq 0) { "SUCCESS" } else { "FAILURE" }
$successRate = if ($totalTests -gt 0) { [math]::Round(($passedTests / $totalTests) * 100, 1) } else { 0 }
$statusClass = if ($overallStatus -eq "SUCCESS") { "status-success" } else { "status-failure" }
$statusIcon = "" # Status icon handled by CSS

Write-Host ""
Write-Host "Test Summary:" -ForegroundColor Cyan
Write-Host "  Total: $totalTests | Passed: $passedTests | Failed: $failedTests | Skipped: $skippedTests" -ForegroundColor White
Write-Host "  Duration: ${totalDuration}s | Success Rate: ${successRate}%" -ForegroundColor White
Write-Host "  Overall Status: $overallStatus" -ForegroundColor $(if($overallStatus -eq "SUCCESS"){"Green"}else{"Red"})

# Generate test suites HTML
$testSuitesHtml = ""
foreach ($result in $suiteResults) {
    $suiteSuccessRate = if ($result.Total -gt 0) { [math]::Round(($result.Passed / $result.Total) * 100, 1) } else { 0 }
    $passedWidth = if ($result.Total -gt 0) { ($result.Passed / $result.Total) * 100 } else { 0 }
    $failedWidth = if ($result.Total -gt 0) { ($result.Failed / $result.Total) * 100 } else { 0 }
    $skippedWidth = if ($result.Total -gt 0) { ($result.Skipped / $result.Total) * 100 } else { 0 }
    
    $statusClass = if ($result.Status -eq "PASSED") { "suite-passed" } else { "suite-failed" }
    $statusBadge = if ($result.Status -eq "PASSED") { "status-success" } else { "status-failure" }
    
    $testSuitesHtml += @"
            <div class="suite-item">
                <div class="suite-info">
                    <div class="suite-details">
                        <h4>$($result.Name)</h4>
                        <p>$($result.Description)</p>
                    </div>
                </div>
                <div class="suite-stats">
                    <div class="suite-stat $statusClass">
                        <div class="suite-stat-number">$($result.Total)</div>
                        <div class="suite-stat-label">Total</div>
                    </div>
                    <div class="suite-stat suite-passed">
                        <div class="suite-stat-number">$($result.Passed)</div>
                        <div class="suite-stat-label">Passed</div>
                    </div>
                    <div class="suite-stat suite-failed">
                        <div class="suite-stat-number">$($result.Failed)</div>
                        <div class="suite-stat-label">Failed</div>
                    </div>
                    <div class="suite-stat">
                        <div class="suite-stat-number">$($result.Duration)</div>
                        <div class="suite-stat-label">Duration</div>
                    </div>
                    <span class="suite-status $statusBadge">$($result.Status)</span>
                </div>
            </div>
"@
}

# Load template and replace placeholders
Write-Host ""
Write-Host "Generating HTML report..." -ForegroundColor Cyan

$templatePath = "test-report-template.html"
if (-not (Test-Path $templatePath)) {
    Write-Host "Template file not found: $templatePath" -ForegroundColor Red
    return
}

$template = Get-Content $templatePath -Raw
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

# Replace placeholders
$html = $template -replace "{{TIMESTAMP}}", $timestamp `
                 -replace "{{TOTAL_TESTS}}", $totalTests `
                 -replace "{{PASSED_TESTS}}", $passedTests `
                 -replace "{{FAILED_TESTS}}", $failedTests `
                 -replace "{{SKIPPED_TESTS}}", $skippedTests `
                 -replace "{{DURATION}}", "${totalDuration}s" `
                 -replace "{{SUCCESS_RATE}}", $successRate `
                 -replace "{{OVERALL_STATUS}}", $overallStatus `
                 -replace "{{STATUS_CLASS}}", $statusClass `
                 -replace "{{TEST_SUITES}}", $testSuitesHtml

# Write HTML file
$htmlPath = Join-Path $PWD $OutputPath
$html | Out-File -FilePath $htmlPath -Encoding UTF8

Write-Host "[SUCCESS] Report generated: $htmlPath" -ForegroundColor Green

# Open if requested
if ($OpenReport) {
    Write-Host "[BROWSER] Opening report..." -ForegroundColor Cyan
    Start-Process $htmlPath
}

# Also generate JSON report for CI/CD integration
$jsonReport = @{
    timestamp = $timestamp
    summary = @{
        total = $totalTests
        passed = $passedTests
        failed = $failedTests
        skipped = $skippedTests
        duration = "${totalDuration}s"
        successRate = $successRate
        status = $overallStatus
    }
    suites = $suiteResults
} | ConvertTo-Json -Depth 3

$jsonPath = $OutputPath -replace "\.html$", ".json"
$jsonReport | Out-File -FilePath $jsonPath -Encoding UTF8
Write-Host "[JSON] JSON report: $jsonPath" -ForegroundColor Green

Write-Host ""
Write-Host "=============================" -ForegroundColor Cyan
Write-Host "Report Complete! SUCCESS!" -ForegroundColor Green
Write-Host "=============================" -ForegroundColor Cyan