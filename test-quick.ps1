try {
    $body = @{email="agente@test.com";password="Test1234";firstName="Agente";lastName="Test"} | ConvertTo-Json
    $r = Invoke-WebRequest -Uri "http://localhost:8081/api/auth/register" -Method POST -Body $body -ContentType "application/json" -ErrorAction Stop
    Write-Host "Status:" $r.StatusCode
    Write-Host "Content:" $r.Content
} catch {
    Write-Host "ERROR: $($_.Exception.Message)"
    Write-Host "Status code:" $_.Exception.Response.StatusCode.value__
}