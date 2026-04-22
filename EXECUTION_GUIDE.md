# InsureZen - Quick Start Guide

## Prerequisites

### 1. Install .NET SDK 10.0
```powershell
winget install Microsoft.DotNet.SDK.10
# Verify: dotnet --version
```

### 2. Install PostgreSQL
```powershell
winget install PostgreSQL.PostgreSQL
# Default port: 5432, password: postgres
# Verify: psql --version
```

## Setup & Run

### 1. Navigate to Project Directory
```powershell
cd d:\InsureZenv2
```

### 2. Restore Packages
```powershell
dotnet restore
```

### 3. Create Database
```powershell
psql -h localhost -U postgres -c "CREATE DATABASE \"InsureZenDBv3\" ENCODING 'UTF8';"
```

### 4. Update Connection String (if needed)
Edit `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=InsureZenDBv3;Username=postgres;Password=postgres"
  }
}
```

### 5. Run Migrations
```powershell
dotnet ef database update
```

### 6. Build & Run Application
```powershell
dotnet run
```

**Expected Output:**
```
Now listening on: https://localhost:5001
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to exit
```

### 7. Access API
- **Swagger UI:** https://localhost:5001/swagger/index.html
- **API Base URL:** https://localhost:5001/api

---

## Testing the API

### 1. Create Insurance Company (Database)
```powershell
psql -h localhost -U postgres -d InsureZenDBv3

# In psql prompt:
INSERT INTO "InsuranceCompanies" ("Id", "Name", "Code", "ContactEmail", "ContactPhone", "IsActive", "CreatedAt")
VALUES ('550e8400-e29b-41d4-a716-446655440000'::uuid, 'Test Insurance', 'TEST-001', 'test@company.com', '555-1234', true, NOW());

\q
```

### 2. Register User
```powershell
$body = @{
    username = "maker1"
    email = "maker1@company.com"
    password = "Password123!"
    role = "Maker"
    insuranceCompanyId = "550e8400-e29b-41d4-a716-446655440000"
} | ConvertTo-Json

Invoke-RestMethod `
    -Uri "https://localhost:5001/api/auth/register" `
    -Method Post `
    -Body $body `
    -ContentType "application/json" `
    -SkipCertificateCheck
```

### 3. Login & Get Token
```powershell
$body = @{
    username = "maker1"
    password = "Password123!"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/auth/login" `
    -Method Post `
    -Body $body `
    -ContentType "application/json" `
    -SkipCertificateCheck

$token = $response.token
Write-Host "Token: $token"
```

### 4. Ingest Claim
```powershell
$token = "YOUR_TOKEN_HERE"

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$body = @{
    patientName = "John Doe"
    patientId = "PAT-001"
    serviceDate = "2026-04-15"
    claimAmount = 1500.00
    serviceDescription = "Emergency Room"
    providerName = "City Hospital"
    providerCode = "PROV-001"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/claims/ingest" `
    -Method Post `
    -Headers $headers `
    -Body $body `
    -SkipCertificateCheck

$claimId = $response.id
Write-Host "Claim ID: $claimId"
```

### 5. View Claims (Maker)
```powershell
$headers = @{
    "Authorization" = "Bearer $token"
}

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/claims/maker/list" `
    -Method Get `
    -Headers $headers `
    -SkipCertificateCheck

$response.claims
```

### 6. Lock Claim
```powershell
Invoke-RestMethod `
    -Uri "https://localhost:5001/api/claims/$claimId/lock/maker" `
    -Method Post `
    -Headers $headers `
    -SkipCertificateCheck
```

### 7. Submit Maker Review
```powershell
$body = @{
    feedback = "Documents verified"
    recommendation = "Approve"
} | ConvertTo-Json

$headers["Content-Type"] = "application/json"

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/claims/$claimId/review/maker" `
    -Method Post `
    -Headers $headers `
    -Body $body `
    -SkipCertificateCheck

$response
```

### 8. Checker Review (Same Process)
```powershell
# Register checker user
# Login with checker account
# Get claims from /claims/checker/list
# Lock and review claim
# Submit decision
```

### 9. Forward to Insurer
```powershell
Invoke-RestMethod `
    -Uri "https://localhost:5001/api/claims/$claimId/forward-to-insurer" `
    -Method Post `
    -Headers $headers `
    -SkipCertificateCheck
```

---

## Common Commands

### View Logs
```powershell
Get-Content "logs/app-$(Get-Date -Format 'yyyy-MM-dd').txt" -Tail 50
```

### Check PostgreSQL Status
```powershell
pg_isready -h localhost -p 5432
```

### Reset Database
```powershell
psql -h localhost -U postgres -c "DROP DATABASE \"InsureZenDBv3\";"
psql -h localhost -U postgres -c "CREATE DATABASE \"InsureZenDBv3\";"
dotnet ef database update
```

### Stop Application
Press `Ctrl+C` in the running terminal.

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| PostgreSQL connection failed | Start PostgreSQL service and verify password |
| Port 5001 already in use | Kill process or change port in launchSettings.json |
| Migrations fail | Ensure database exists and connection string is correct |
| JWT token expired | Login again to get a new token |

---

**Last Updated:** April 22, 2026
