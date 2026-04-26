add-type -AssemblyName System.Net
[System.Net.ServicePointManager]::CertificatePolicy = New-Object Object # dummy - will be overridden

# Test Register
Write-Host "=== Testing AUTH REGISTER ===" -ForegroundColor Cyan
$body = @{email="testfin@test.com";password="123456";firstName="Final";lastName="Test"} | ConvertTo-Json
try {
    $r = Invoke-WebRequest -Uri "https://localhost:44358/api/auth/register" -Method POST -Body $body -ContentType "application/json"
    Write-Host "REGISTER Status:" $r.StatusCode -ForegroundColor Green
    Write-Host "Content:" $r.Content
} catch {
    Write-Host "ERROR:" $_.Exception.Message -ForegroundColor Red
}

# Test Login
Write-Host "`n=== Testing AUTH LOGIN ===" -ForegroundColor Cyan
$body = @{email="testfin@test.com";password="123456"} | ConvertTo-Json
try {
    $r = Invoke-WebRequest -Uri "https://localhost:44358/api/auth/login" -Method POST -Body $body -ContentType "application/json"
    Write-Host "LOGIN Status:" $r.StatusCode -ForegroundColor Green
    Write-Host "Content:" $r.Content
} catch {
    Write-Host "ERROR:" $_.Exception.Message -ForegroundColor Red
}

# Test GET weathers
Write-Host "`n=== Testing WEATHER GET ===" -ForegroundColor Cyan
try {
    $r = Invoke-WebRequest -Uri "https://localhost:44358/api/weatherforecast" -Method GET
    Write-Host "GET Status:" $r.StatusCode -ForegroundColor Green
    Write-Host "Content:" $r.Content
} catch {
    Write-Host "ERROR:" $_.Exception.Message -ForegroundColor Red
}

Write-Host "`nDone!" -ForegroundColor Cyan