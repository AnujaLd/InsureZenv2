# InsureZen - Complete Step-by-Step Execution Guide

This guide walks you through setting up and running the InsureZen backend application from scratch.

## Prerequisites Installation

### 1. Install .NET SDK 10.0

#### Option A: Using WinGet (Recommended for Windows)
```powershell
winget install Microsoft.DotNet.SDK.10
```

#### Option B: Using Chocolatey
```powershell
choco install dotnet-sdk-10.0
```

#### Option C: Manual Download
Visit: https://dotnet.microsoft.com/download/dotnet/10.0
Download .NET 10.0 SDK installer and run it

**Verify Installation:**
```powershell
dotnet --version
# Output should show: 10.0.202 or higher
```

### 2. Install PostgreSQL 18.3

#### Option A: Using WinGet
```powershell
winget install PostgreSQL.PostgreSQL
```

#### Option B: Using Chocolatey
```powershell
choco install postgresql
```

#### Option C: Manual Download
Visit: https://www.postgresql.org/download/windows/
Download PostgreSQL 18.3 installer and follow installation wizard

**During Installation:**
- Keep default port: **5432**
- Set password for `postgres` user (remember this password!)
- Default locale: English, UTF-8

**Verify Installation:**
```powershell
psql --version
# Output should show: psql (PostgreSQL) 18.3
```

### 3. Install pgAdmin 4 (Optional but Recommended)

```powershell
# Using WinGet
winget install ERP.pgAdmin

# Or download from: https://www.pgadmin.org/download/pgadmin-4-windows/
```

## Project Setup - Complete Commands

### Step 1: Navigate to Project Directory

```powershell
cd d:\InsureZenv2
```

### Step 2: Verify Project Structure

```powershell
# List directory structure
tree /F

# You should see:
# InsureZenv2.csproj
# Program.cs
# appsettings.json
# appsettings.Development.json
# .gitignore
# README.md
# src/ folder with subfolders
```

### Step 3: Restore NuGet Packages

```powershell
dotnet restore

# Output will show downloading packages:
# Determining projects to restore...
# Restoring d:\InsureZenv2\InsureZenv2.csproj ...
# Restored d:\InsureZenv2\InsureZenv2.csproj (xxx ms)
```

### Step 4: Update Database Configuration

#### Check Current Settings
```powershell
# View development appsettings
Get-Content appsettings.Development.json
```

#### Update Connection String

Edit `appsettings.Development.json` and change the connection string:

**Original:**
```json
"DefaultConnection": "Host=localhost;Port=5432;Database=InsureZenDBv3;Username=postgres;Password=postgres"
```

**If your PostgreSQL password is different, update it:**
```json
"DefaultConnection": "Host=localhost;Port=5432;Database=InsureZenDBv3;Username=postgres;Password=YOUR_PASSWORD_HERE"
```

**Using PowerShell to edit:**
```powershell
# Using a text editor
notepad appsettings.Development.json

# Or using PowerShell (if you know the new password):
(Get-Content appsettings.Development.json) -replace 'Password=postgres', 'Password=YourActualPassword' | Set-Content appsettings.Development.json
```

### Step 5: Verify PostgreSQL Connection

```powershell
# Test connection to PostgreSQL
psql -h localhost -U postgres -c "SELECT version();"

# If prompted for password, enter your PostgreSQL password
# Output should show PostgreSQL version information
```

### Step 6: Create Application Database

#### Using psql (Command Line)

```powershell
# Connect to PostgreSQL server
psql -h localhost -U postgres

# In psql prompt (psql=#):
CREATE DATABASE "InsureZenDBv3" ENCODING 'UTF8' LOCALE 'en_US.UTF-8';

# Verify database creation
\l

# You should see InsureZenDBv3 in the list

# Exit psql
\q
```

#### Using PowerShell One-Liner

```powershell
psql -h localhost -U postgres -c "CREATE DATABASE \"InsureZenDBv3\" ENCODING 'UTF8';"
```

#### Using pgAdmin GUI (if installed)

1. Open pgAdmin 4
2. Right-click on "Databases" in left sidebar
3. Select "Create" → "Database"
4. Name: `InsureZenDBv3`
5. Click "Save"

### Step 7: Run EF Core Migrations

```powershell
# The application will automatically migrate on first run, but you can do it manually:

dotnet ef database update --project .

# Or with verbose output:
dotnet ef database update --project . --verbose

# Output should show:
# Applying migration '....'
# Done.
```

### Step 8: Verify Database Tables

```powershell
# Connect to the database
psql -h localhost -U postgres -d InsureZenDBv3

# List all tables
\dt

# You should see:
# public | InsuranceCompanies    | table | postgres
# public | Users                 | table | postgres
# public | Claims                | table | postgres
# public | ClaimReviews          | table | postgres
# public | AuditLogs             | table | postgres

# Exit psql
\q
```

### Step 9: Build the Application

```powershell
# Build in Debug mode
dotnet build

# Or build in Release mode (optimized):
dotnet build -c Release

# Output should end with:
# Build succeeded.
```

### Step 10: Run the Application

#### First Time Run (Creates Logs Directory)

```powershell
dotnet run

# Or specify configuration:
dotnet run --configuration Debug

# Or run Release build:
dotnet run --configuration Release -f net10.0
```

**Expected Output:**
```
Building...
info: Serilog.AspNetCore.ReloadableWebHostBuilder[0]
      Configuring web host builder for Serilog
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to exit
```

### Step 11: Verify Application is Running

Open another PowerShell window and test the API:

```powershell
# Test if API is responding
curl -k https://localhost:5001/api/auth/login

# Or using Invoke-RestMethod:
Invoke-RestMethod -Uri "https://localhost:5001/swagger/index.html" -Method GET

# You should get a response (Swagger UI loads)
```

### Step 12: Access Swagger UI

Open your browser and navigate to:
```
https://localhost:5001/swagger/index.html
```

You should see the Swagger UI with all available endpoints.

## Testing the API

### 1. Create an Insurance Company (Via Database)

```powershell
# Connect to database
psql -h localhost -U postgres -d InsureZenDBv3

# Insert insurance company
INSERT INTO "InsuranceCompanies" ("Id", "Name", "Code", "ContactEmail", "ContactPhone", "IsActive", "CreatedAt")
VALUES ('550e8400-e29b-41d4-a716-446655440000'::uuid, 'Test Insurance Co', 'TEST-001', 'contact@test.com', '555-1234', true, NOW());

# Verify insertion
SELECT * FROM "InsuranceCompanies";

# Exit
\q
```

### 2. Register a Maker User

```powershell
# Using PowerShell with REST API
$body = @{
    username = "maker1"
    email = "maker1@insurezen.com"
    password = "SecurePassword123!"
    role = "Maker"
    insuranceCompanyId = "550e8400-e29b-41d4-a716-446655440000"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/auth/register" `
    -Method Post `
    -Body $body `
    -ContentType "application/json" `
    -SkipCertificateCheck

$response
```

### 3. Register a Checker User

```powershell
$body = @{
    username = "checker1"
    email = "checker1@insurezen.com"
    password = "SecurePassword123!"
    role = "Checker"
    insuranceCompanyId = "550e8400-e29b-41d4-a716-446655440000"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/auth/register" `
    -Method Post `
    -Body $body `
    -ContentType "application/json" `
    -SkipCertificateCheck

$response
```

### 4. Login and Get JWT Token

```powershell
$body = @{
    username = "maker1"
    password = "SecurePassword123!"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/auth/login" `
    -Method Post `
    -Body $body `
    -ContentType "application/json" `
    -SkipCertificateCheck

$token = $response.token
Write-Host "Token: $token"
Write-Host "User: $($response.user.username)"
Write-Host "Role: $($response.user.role)"
```

### 5. Ingest a Claim

```powershell
# Use the token from previous step
$token = "YOUR_JWT_TOKEN_HERE"

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$body = @{
    patientName = "John Doe"
    patientId = "PAT-001234"
    serviceDate = "2026-04-15T00:00:00Z"
    claimAmount = 1500.00
    serviceDescription = "Emergency Room Visit"
    providerName = "City Hospital"
    providerCode = "PROV-123"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/claims/ingest" `
    -Method Post `
    -Headers $headers `
    -Body $body `
    -SkipCertificateCheck

$claimId = $response.id
Write-Host "Claim created with ID: $claimId"
Write-Host "Claim Number: $($response.claimNumber)"
Write-Host "Status: $($response.status)"
```

### 6. View Claims Available for Maker

```powershell
$token = "YOUR_JWT_TOKEN_HERE"

$headers = @{
    "Authorization" = "Bearer $token"
}

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/claims/maker/list?pageNumber=1&pageSize=10" `
    -Method Get `
    -Headers $headers `
    -SkipCertificateCheck

$response.claims | ForEach-Object {
    Write-Host "Claim: $($_.claimNumber) - $($_.patientName) - $($_.status)"
}
```

### 7. Lock Claim for Maker Review

```powershell
$token = "YOUR_JWT_TOKEN_HERE"
$claimId = "CLAIM_ID_FROM_PREVIOUS_STEP"

$headers = @{
    "Authorization" = "Bearer $token"
}

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/claims/$claimId/lock/maker" `
    -Method Post `
    -Headers $headers `
    -SkipCertificateCheck

$response
```

### 8. Submit Maker Review

```powershell
$token = "YOUR_JWT_TOKEN_HERE"
$claimId = "CLAIM_ID_FROM_PREVIOUS_STEP"

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$body = @{
    feedback = "All documentation verified. Claim appears legitimate and complete."
    recommendation = "Approve"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/claims/$claimId/review/maker" `
    -Method Post `
    -Headers $headers `
    -Body $body `
    -SkipCertificateCheck

Write-Host "Review submitted. New status: $($response.status)"
```

### 9. Checker Views Claims

```powershell
# Get checker token
$body = @{
    username = "checker1"
    password = "SecurePassword123!"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/auth/login" `
    -Method Post `
    -Body $body `
    -ContentType "application/json" `
    -SkipCertificateCheck

$checkerToken = $response.token

# View available claims for checker
$headers = @{
    "Authorization" = "Bearer $checkerToken"
}

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/claims/checker/list?pageNumber=1&pageSize=10" `
    -Method Get `
    -Headers $headers `
    -SkipCertificateCheck

$response.claims | ForEach-Object {
    Write-Host "Claim: $($_.claimNumber) - Status: $($_.status)"
}
```

### 10. Lock and Review Claim as Checker

```powershell
$checkerToken = "CHECKER_JWT_TOKEN"
$claimId = "CLAIM_ID"

# Lock claim
$headers = @{
    "Authorization" = "Bearer $checkerToken"
}

Invoke-RestMethod `
    -Uri "https://localhost:5001/api/claims/$claimId/lock/checker" `
    -Method Post `
    -Headers $headers `
    -SkipCertificateCheck

# Submit decision
$headers["Content-Type"] = "application/json"
$body = @{
    feedback = "Agree with maker assessment. Recommend approval."
    decision = "Approved"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/claims/$claimId/review/checker" `
    -Method Post `
    -Headers $headers `
    -Body $body `
    -SkipCertificateCheck

Write-Host "Checker decision submitted. Final status: $($response.status)"
```

### 11. Forward to Insurer

```powershell
$token = "YOUR_JWT_TOKEN_HERE"
$claimId = "CLAIM_ID"

$headers = @{
    "Authorization" = "Bearer $token"
}

$response = Invoke-RestMethod `
    -Uri "https://localhost:5001/api/claims/$claimId/forward-to-insurer" `
    -Method Post `
    -Headers $headers `
    -SkipCertificateCheck

$response
```

## Stopping the Application

Press `Ctrl+C` in the PowerShell window running the application:

```powershell
# Application will gracefully shutdown
info: Microsoft.Hosting.Lifetime[0]
      Application is shutting down...
```

## Viewing Logs

```powershell
# View recent logs
Get-Content "logs/app-$(Get-Date -Format 'yyyy-MM-dd').txt" -Tail 50

# Or continuously follow logs
Get-Content "logs/app-$(Get-Date -Format 'yyyy-MM-dd').txt" -Wait

# List all log files
Get-ChildItem logs/
```

## Database Maintenance

### Backup Database

```powershell
# Create backup
pg_dump -h localhost -U postgres InsureZenDBv3 > backup_insurezen_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql

Write-Host "Backup created successfully"
```

### Restore Database

```powershell
# Drop existing database
psql -h localhost -U postgres -c "DROP DATABASE \"InsureZenDBv3\";"

# Create new database
psql -h localhost -U postgres -c "CREATE DATABASE \"InsureZenDBv3\";"

# Restore from backup
psql -h localhost -U postgres InsureZenDBv3 < backup_insurezen_20260421_120000.sql

Write-Host "Database restored successfully"
```

### Reset Database (Delete All Data)

```powershell
# Option 1: Drop and recreate (loses all data)
psql -h localhost -U postgres -c "DROP DATABASE \"InsureZenDBv3\";"
psql -h localhost -U postgres -c "CREATE DATABASE \"InsureZenDBv3\";"

# Option 2: Delete from tables (keeps schema)
psql -h localhost -U postgres -d InsureZenDBv3 << EOF
TRUNCATE "AuditLogs" CASCADE;
TRUNCATE "ClaimReviews" CASCADE;
TRUNCATE "Claims" CASCADE;
TRUNCATE "Users" CASCADE;
TRUNCATE "InsuranceCompanies" CASCADE;
EOF
```

## Troubleshooting Guide

### Issue: "Unable to connect to database"

**Cause:** PostgreSQL not running or connection string incorrect

**Solution:**
```powershell
# Check PostgreSQL service status
Get-Service postgresql-x64-18 | Select-Object Status

# Start PostgreSQL if stopped
Start-Service postgresql-x64-18

# Verify connection
psql -h localhost -U postgres -c "SELECT 1;"
```

### Issue: "Port 5001 already in use"

**Cause:** Another application using the port

**Solution:**
```powershell
# Find process using port
netstat -ano | findstr :5001

# Kill the process (replace PID with actual process ID)
taskkill /PID <PID> /F

# Or run on different port by modifying launchSettings.json
```

### Issue: "Access denied" when connecting to PostgreSQL

**Cause:** Wrong password or user

**Solution:**
```powershell
# Verify PostgreSQL is running
pg_isready -h localhost -p 5432

# Test connection with password
psql -h localhost -U postgres -W

# Update appsettings.Development.json with correct password
```

### Issue: "JWT Token has expired"

**Cause:** Token is older than 1 hour in production or 24 hours in development

**Solution:**
```powershell
# Request a new token by logging in again
# See "Login and Get JWT Token" section above
```

### Issue: "403 Forbidden" when accessing checker-only endpoint as Maker

**Cause:** Insufficient permissions

**Solution:**
```powershell
# Ensure you're using correct user's token
# Maker token won't work for /api/claims/checker/* endpoints
# Use checker's token instead
```

## Production Deployment

### Step 1: Build Release Version

```powershell
dotnet publish -c Release -o ./publish
```

### Step 2: Update Production Configuration

Edit `publish/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=prod-db-server;Port=5432;Database=InsureZenDBv3;Username=produser;Password=SecurePassword123"
  },
  "JwtSettings": {
    "SecretKey": "your-very-secure-long-random-key-min-32-chars",
    "ExpirationMinutes": 60
  }
}
```

### Step 3: Run on Production Server

```powershell
# Navigate to publish folder
cd publish

# Set environment to Production
$env:ASPNETCORE_ENVIRONMENT = "Production"

# Run application
dotnet InsureZenv2.dll
```

## Summary of Key Files Created

| File | Purpose |
|------|---------|
| `src/Models/*.cs` | Entity models for database |
| `src/Data/ApplicationDbContext.cs` | EF Core database context |
| `src/Repositories/*.cs` | Data access layer |
| `src/Services/*.cs` | Business logic |
| `src/Controllers/*.cs` | API endpoints |
| `src/DTOs/*.cs` | Request/response models |
| `src/Authentication/*.cs` | JWT and password hashing |
| `src/Validators/*.cs` | Input validation rules |
| `src/Mappers/MappingProfile.cs` | AutoMapper configurations |
| `Program.cs` | Application startup |
| `appsettings.json` | Production configuration |
| `appsettings.Development.json` | Development configuration |
| `InsureZenv2.csproj` | Project file with dependencies |
| `.gitignore` | Git ignore rules |
| `README.md` | Complete documentation |
| `EXECUTION_GUIDE.md` | This file |

## Cleanup and Reset

### Clean Build Artifacts

```powershell
# Remove build artifacts
Remove-Item -Recurse -Force bin/
Remove-Item -Recurse -Force obj/

# Restore packages and rebuild
dotnet clean
dotnet build
```

### Reset Everything (Complete Reset)

```powershell
# Stop application
# [Ctrl+C in running terminal]

# Delete database
psql -h localhost -U postgres -c "DROP DATABASE \"InsureZenDBv3\";"

# Clean build artifacts
dotnet clean

# Delete logs
Remove-Item -Recurse -Force logs/

# Start fresh: database creation happens on first run
dotnet run
```

---

**You're all set!** The application is now fully configured and running. Check the README.md for complete API documentation and use the test commands above to verify all functionality.
