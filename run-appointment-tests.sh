#!/bin/bash

# Test Runner for PetCare Appointment Booking Feature
# This script runs all unit tests and integration tests for the appointment booking functionality

echo "🧪 Running PetCare Appointment Booking Tests"
echo "============================================="

# Set colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✅ $2${NC}"
    else
        echo -e "${RED}❌ $2${NC}"
    fi
}

print_info() {
    echo -e "${YELLOW}📋 $1${NC}"
}

# Navigate to backend directory
cd "$(dirname "$0")/backend" || exit 1

# Test counter
total_tests=0
passed_tests=0

echo ""
print_info "Running Domain Tests (Appointment Model & Status)..."
dotnet test tests/PetCare.Domain.Tests/PetCare.Domain.Tests.csproj --filter "Appointment" --verbosity quiet
domain_result=$?
print_status $domain_result "Domain Tests"
total_tests=$((total_tests + 1))
if [ $domain_result -eq 0 ]; then passed_tests=$((passed_tests + 1)); fi

echo ""
print_info "Running Application Tests (DTOs & Validation)..."
dotnet test tests/PetCare.Application.Tests/PetCare.Application.Tests.csproj --filter "Appointment" --verbosity quiet
app_result=$?
print_status $app_result "Application Tests"
total_tests=$((total_tests + 1))
if [ $app_result -eq 0 ]; then passed_tests=$((passed_tests + 1)); fi

echo ""
print_info "Running Infrastructure Tests (NotificationService)..."
dotnet test tests/PetCare.Infrastructure.Tests/PetCare.Infrastructure.Tests.csproj --filter "Notification" --verbosity quiet
infra_result=$?
print_status $infra_result "Infrastructure Tests"
total_tests=$((total_tests + 1))
if [ $infra_result -eq 0 ]; then passed_tests=$((passed_tests + 1)); fi

echo ""
print_info "Running API Tests (AppointmentsController)..."
dotnet test tests/PetCare.Api.Tests/PetCare.Api.Tests.csproj --filter "Appointments" --verbosity quiet
api_result=$?
print_status $api_result "API Controller Tests"
total_tests=$((total_tests + 1))
if [ $api_result -eq 0 ]; then passed_tests=$((passed_tests + 1)); fi

echo ""
print_info "Running Integration Tests (End-to-End)..."
dotnet test tests/PetCare.Integration.Tests/PetCare.Integration.Tests.csproj --filter "Appointments" --verbosity quiet
integration_result=$?
print_status $integration_result "Integration Tests"
total_tests=$((total_tests + 1))
if [ $integration_result -eq 0 ]; then passed_tests=$((passed_tests + 1)); fi

echo ""
echo "============================================="
echo "📊 TEST SUMMARY"
echo "============================================="
echo "Total Test Suites: $total_tests"
echo "Passed: $passed_tests"
echo "Failed: $((total_tests - passed_tests))"

if [ $passed_tests -eq $total_tests ]; then
    echo -e "${GREEN}🎉 All appointment booking tests passed!${NC}"
    exit 0
else
    echo -e "${RED}💥 Some tests failed. Please review the output above.${NC}"
    exit 1
fi