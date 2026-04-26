# Test all WeatherForecast endpoints - Fixed
add-type @"
using System.Net;
using System.Security.Cryptography.X509Certificates;
public class TrustAll : ICertificatePolicy {
    public bool CheckValidationResult(
        ServicePoint srvPoint,
        X509Certificate certificate,
        WebRequest request,
        int certificateProblem) {
        return true;
    }
}
"@
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAll

$baseUrl = "https://localhost:44358"

Write-Host "Testing WeatherForecast endpoints..." -ForegroundColor Cyan

# GET all
Write-Host "`n1. GET /api/weatherforecast" -ForegroundColor Yellow
try {
    $r = Invoke-WebRequest -Uri "$baseUrl/api/weatherforecast" -Method GET
    Write-Host "   Status: $($r.StatusCode)" -ForegroundColor Green
    Write-Host "   Content: $($r.Content)"
} catch {
    Write-Host "   ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# DELETE any existing first
Write-Host "`nCleaning up..." -ForegroundColor Yellow
try { Invoke-WebRequest -Uri "$baseUrl/api/weatherforecast/1" -Method DELETE -ErrorAction SilentlyContinue } catch {}

# POST create
Write-Host "`n2. POST /api/weatherforecast" -ForegroundColor Yellow
try {
    $body = @{date="2026-04-15";temperatureC=30;summary="Hot"} | ConvertTo-Json
    $r = Invoke-WebRequest -Uri "$baseUrl/api/weatherforecast" -Method POST -Body $body -ContentType "application/json"
    Write-Host "   Status: $($r.StatusCode)" -ForegroundColor Green
    Write-Host "   Content: $($r.Content)"
} catch {
    Write-Host "   ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# GET by ID
Write-Host "`n3. GET /api/weatherforecast/1" -ForegroundColor Yellow
try {
    $r = Invoke-WebRequest -Uri "$baseUrl/api/weatherforecast/1" -Method GET
    Write-Host "   Status: $($r.StatusCode)" -ForegroundColor Green
    Write-Host "   Content: $($r.Content)"
} catch {
    Write-Host "   ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# PUT update
Write-Host "`n4. PUT /api/weatherforecast/1" -ForegroundColor Yellow
try {
    $body = @{date="2026-04-16";temperatureC=25;summary="Warm"} | ConvertTo-Json
    $r = Invoke-WebRequest -Uri "$baseUrl/api/weatherforecast/1" -Method PUT -Body $body -ContentType "application/json"
    Write-Host "   Status: $($r.StatusCode)" -ForegroundColor Green
    Write-Host "   Content: $($r.Content)"
} catch {
    Write-Host "   ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# DELETE
Write-Host "`n5. DELETE /api/weatherforecast/1" -ForegroundColor Yellow
try {
    $r = Invoke-WebRequest -Uri "$baseUrl/api/weatherforecast/1" -Method DELETE
    Write-Host "   Status: $($r.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "   ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# Auth Register
Write-Host "`n6. POST /api/auth/register" -ForegroundColor Yellow
try {
    $body = @{email="testapi@test.com";password="123456";firstName="Api";lastName="Test"} | ConvertTo-Json
    $r = Invoke-WebRequest -Uri "$baseUrl/api/auth/register" -Method POST -Body $body -ContentType "application/json"
    Write-Host "   Status: $($r.StatusCode)" -ForegroundColor Green
    Write-Host "   Content: $($r.Content)"
} catch {
    Write-Host "   ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

# Auth Login  
Write-Host "`n7. POST /api/auth/login" -ForegroundColor Yellow
try {
    $body = @{email="testapi@test.com";password="123456"} | ConvertTo-Json
    $r = Invoke-WebRequest -Uri "$baseUrl/api/auth/login" -Method POST -Body $body -ContentType "application/json"
    Write-Host "   Status: $($r.StatusCode)" -ForegroundColor Green
    Write-Host "   Content: $($r.Content)"
} catch {
    Write-Host "   ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nDone!" -ForegroundColor Cyan