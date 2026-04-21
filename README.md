# InsureZen Backend API

A comprehensive medical insurance claim management system with a two-stage review workflow (Maker and Checker) built with ASP.NET Core 10.0 and PostgreSQL.

## Table of Contents

- [Project Overview](#project-overview)
- [Requirements Analysis](#requirements-analysis)
- [API Design](#api-design)
- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Setup Instructions](#setup-instructions)
- [Database Setup](#database-setup)
- [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
- [Authentication & Authorization](#authentication--authorization)
- [Database Schema](#database-schema)
- [Assumptions](#assumptions)
- [Testing](#testing)
- [Troubleshooting](#troubleshooting)

## Project Overview

InsureZen digitizes and streamlines medical insurance claim processing on behalf of multiple insurance companies. This backend system provides REST APIs for:

1. **Claim Ingestion**: Uploading extracted claim data from forms
2. **Maker Review**: Insurance employees reviewing and recommending claim approval/rejection
3. **Checker Review**: Second-level review and final decision
4. **Claim History**: Searchable, paginated claim history with filtering
5. **Audit Trails**: Complete tracking of all actions and state transitions

## Requirements Analysis

### Entities

1. **InsuranceCompany** - Partner insurance companies using the system
2. **User** - InsureZen employees (Makers or Checkers)
3. **Claim** - Insurance claims submitted for processing
4. **ClaimReview** - Reviews submitted by Makers and Checkers
5. **AuditLog** - Complete audit trail of all actions

### Actors & Roles

- **Maker**: Reviews extracted claim data, adds feedback, and recommends approval or rejection
- **Checker**: Reviews Maker's recommendation alongside claim data and issues final decision
- **System**: Manages claim state transitions and forwards completed claims

### Functional Requirements

1. Ingestion of structured claim data
2. Concurrent-safe locking mechanism for claims
3. Maker workflow: view pending claims → lock → review → submit recommendation
4. Checker workflow: view submitted claims → lock → review recommendation → issue decision
5. Pagination and filtering of claim history
6. JWT-based authentication and role-based authorization
7. Complete audit logging of all transitions

### Non-Functional Requirements

1. **Concurrency Safety**: Only one user can review a claim at a time
2. **Auditability**: Every action is logged with timestamp and user
3. **Data Integrity**: State transitions follow strict rules
4. **Performance**: Efficient querying with appropriate indexing
5. **Security**: JWT authentication, password hashing, role-based access control

### State Diagram

```
Pending 
  ↓
MakerInProgress (Locked by Maker)
  ↓
MakerSubmitted
  ↓
CheckerInProgress (Locked by Checker)
  ↓
Approved/Rejected
  ↓
ForwardedToInsurer
```

### Assumptions

1. **Upstream Service**: Assumes a separate OCR/parsing service provides structured claim data
2. **User Roles**: Only two roles - "Maker" and "Checker"
3. **Concurrency Model**: Pessimistic locking via `LockedByUserId` field
4. **Timezone**: All timestamps are in UTC
5. **Insurance Company Association**: All users belong to one insurance company
6. **Token Expiration**: JWT tokens expire after 1 hour in production, 24 hours in development
7. **No Email Verification**: Registration creates active users immediately
8. **Insurer Forwarding**: A logged database entry serves as the "forwarding" to insurance company

## API Design

### Base URL
```
https://localhost:5001/api
```

### Authentication

All endpoints except `/auth/login` and `/auth/register` require JWT Bearer token in Authorization header:

```
Authorization: Bearer <jwt_token>
```

### Authentication Endpoints

#### POST /auth/login
Login with username and password

**Request:**
```json
{
  "username": "maker1",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGc...",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "username": "maker1",
    "email": "maker1@insurezen.com",
    "role": "Maker",
    "isActive": true
  },
  "expiresIn": 3600
}
```

**Error (401 Unauthorized):**
```json
{
  "message": "Invalid username or password"
}
```

#### POST /auth/register
Register a new user

**Request:**
```json
{
  "username": "newmaker",
  "email": "newmaker@insurezen.com",
  "password": "SecurePassword123!",
  "role": "Maker",
  "insuranceCompanyId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response (201 Created):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "username": "newmaker",
  "email": "newmaker@insurezen.com",
  "role": "Maker",
  "isActive": true
}
```

### Claim Management Endpoints

#### POST /claims/ingest
Ingest a new claim

**Authorization:** Required

**Request:**
```json
{
  "patientName": "John Doe",
  "patientId": "PAT-001234",
  "serviceDate": "2026-04-15T00:00:00Z",
  "claimAmount": 1500.00,
  "serviceDescription": "Emergency Room Visit",
  "providerName": "City Hospital",
  "providerCode": "PROV-123"
}
```

**Response (201 Created):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "claimNumber": "CLM-20260421-A1B2C3D4",
  "patientName": "John Doe",
  "patientId": "PAT-001234",
  "serviceDate": "2026-04-15T00:00:00Z",
  "claimAmount": 1500.00,
  "serviceDescription": "Emergency Room Visit",
  "providerName": "City Hospital",
  "providerCode": "PROV-123",
  "status": "Pending",
  "submittedAt": "2026-04-21T10:30:00Z",
  "completedAt": null
}
```

#### GET /claims/{id}
Get claim details

**Authorization:** Required

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "claimNumber": "CLM-20260421-A1B2C3D4",
  "patientName": "John Doe",
  "patientId": "PAT-001234",
  "serviceDate": "2026-04-15T00:00:00Z",
  "claimAmount": 1500.00,
  "serviceDescription": "Emergency Room Visit",
  "providerName": "City Hospital",
  "providerCode": "PROV-123",
  "status": "Approved",
  "submittedAt": "2026-04-21T10:30:00Z",
  "completedAt": "2026-04-21T14:45:00Z",
  "makerReview": {
    "id": "550e8400-e29b-41d4-a716-446655440003",
    "reviewedByUsername": "maker1",
    "feedback": "All documentation present and verified",
    "makerRecommendation": "Approve",
    "checkerDecision": null,
    "reviewedAt": "2026-04-21T11:00:00Z"
  },
  "checkerReview": {
    "id": "550e8400-e29b-41d4-a716-446655440004",
    "reviewedByUsername": "checker1",
    "feedback": "Agree with maker recommendation",
    "makerRecommendation": null,
    "checkerDecision": "Approved",
    "reviewedAt": "2026-04-21T14:45:00Z"
  }
}
```

#### GET /claims/maker/list
Get claims available for maker review (paginated)

**Authorization:** Required (Maker only)

**Query Parameters:**
- `pageNumber` (int, default: 1)
- `pageSize` (int, default: 10, max: 100)
- `status` (string, optional): "Pending" or "MakerInProgress"
- `insuranceCompanyId` (guid, optional)
- `fromDate` (datetime, optional)
- `toDate` (datetime, optional)

**Response (200 OK):**
```json
{
  "claims": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440002",
      "claimNumber": "CLM-20260421-A1B2C3D4",
      "patientName": "John Doe",
      "patientId": "PAT-001234",
      "serviceDate": "2026-04-15T00:00:00Z",
      "claimAmount": 1500.00,
      "serviceDescription": "Emergency Room Visit",
      "providerName": "City Hospital",
      "providerCode": "PROV-123",
      "status": "Pending",
      "submittedAt": "2026-04-21T10:30:00Z",
      "completedAt": null
    }
  ],
  "totalCount": 42,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

#### GET /claims/checker/list
Get claims available for checker review (paginated)

**Authorization:** Required (Checker only)

**Query Parameters:** Same as `/claims/maker/list`

**Response (200 OK):** Same structure as `/claims/maker/list`

#### POST /claims/{id}/lock/maker
Lock claim for maker review

**Authorization:** Required (Maker only)

**Response (200 OK):**
```json
{
  "message": "Claim locked successfully"
}
```

**Error (400 Bad Request):**
```json
{
  "message": "Cannot lock claim. It may be locked by another user or not in the correct status."
}
```

#### POST /claims/{id}/lock/checker
Lock claim for checker review

**Authorization:** Required (Checker only)

**Response (200 OK):**
```json
{
  "message": "Claim locked successfully"
}
```

#### POST /claims/{id}/unlock
Unlock claim

**Authorization:** Required

**Response (200 OK):**
```json
{
  "message": "Claim unlocked successfully"
}
```

**Error (400 Bad Request):**
```json
{
  "message": "Cannot unlock claim. You don't have permission."
}
```

#### POST /claims/{id}/review/maker
Submit maker review and recommendation

**Authorization:** Required (Maker only)

**Request:**
```json
{
  "feedback": "All documents verified. Claim appears legitimate.",
  "recommendation": "Approve"
}
```

**Response (200 OK):** Claim detail with updated status

**Error (400 Bad Request):**
```json
{
  "message": "Cannot submit review. You don't have permission or claim is not locked by you."
}
```

#### POST /claims/{id}/review/checker
Submit checker decision

**Authorization:** Required (Checker only)

**Request:**
```json
{
  "feedback": "Agree with maker assessment. Recommend approval.",
  "decision": "Approved"
}
```

**Response (200 OK):** Claim detail with final status

#### POST /claims/{id}/forward-to-insurer
Forward completed claim to insurance company

**Authorization:** Required

**Response (200 OK):**
```json
{
  "message": "Claim forwarded to insurer successfully"
}
```

**Error (400 Bad Request):**
```json
{
  "message": "Claim cannot be forwarded. It must be in Approved or Rejected status."
}
```

## Prerequisites

- **OS**: Windows 10/11 or Linux/macOS
- **.NET SDK**: Version 10.0.202 or higher
- **PostgreSQL**: Version 18.3 or higher
- **Git**: For version control

## Project Structure

```
InsureZenv2/
├── src/
│   ├── Authentication/
│   │   ├── JwtTokenService.cs       # JWT token generation and validation
│   │   └── PasswordHasher.cs        # Password hashing with PBKDF2
│   ├── Controllers/
│   │   ├── AuthController.cs        # Authentication endpoints
│   │   └── ClaimsController.cs      # Claim management endpoints
│   ├── Data/
│   │   └── ApplicationDbContext.cs  # EF Core database context
│   ├── DTOs/
│   │   ├── ClaimDtos.cs
│   │   ├── UserDtos.cs
│   │   └── InsuranceCompanyDtos.cs
│   ├── Models/
│   │   ├── InsuranceCompany.cs
│   │   ├── User.cs
│   │   ├── Claim.cs
│   │   ├── ClaimReview.cs
│   │   ├── AuditLog.cs
│   │   └── Enums.cs
│   ├── Repositories/
│   │   ├── ClaimRepository.cs
│   │   ├── UserRepository.cs
│   │   ├── ClaimReviewRepository.cs
│   │   └── AuditLogRepository.cs
│   ├── Services/
│   │   ├── ClaimService.cs
│   │   └── AuthenticationService.cs
│   ├── Validators/
│   │   ├── ClaimValidator.cs
│   │   └── UserValidator.cs
│   └── Mappers/
│       └── MappingProfile.cs        # AutoMapper configurations
├── Migrations/                      # EF Core migrations
├── Tests/                           # Unit and integration tests
├── Program.cs                       # Application startup configuration
├── appsettings.json                # Production settings
├── appsettings.Development.json    # Development settings
├── InsureZenv2.csproj              # Project file with NuGet dependencies
├── .gitignore                       # Git ignore rules
└── README.md                        # This file
```

## Setup Instructions

### Step 1: Install Prerequisites

#### Install .NET SDK 10.0
```powershell
# Check if already installed
dotnet --version

# Download from https://dotnet.microsoft.com/download
# Or use package manager:
# Windows (Chocolatey):
choco install dotnet-sdk-10.0

# Windows (WinGet):
winget install Microsoft.DotNet.SDK.10
```

#### Install PostgreSQL 18.3
```powershell
# Windows (Chocolatey):
choco install postgresql

# Windows (Direct Download):
# https://www.postgresql.org/download/windows/

# After installation, PostgreSQL runs on port 5432
# Default user: postgres
```

### Step 2: Clone or Extract Repository

```powershell
# If using git:
git clone <repository-url>
cd InsureZenv2

# Or navigate to existing folder:
cd d:\InsureZenv2
```

### Step 3: Restore NuGet Packages

```powershell
dotnet restore
```

This will download all required packages specified in `InsureZenv2.csproj`

### Step 4: Update Database Configuration

Edit `appsettings.Development.json` with your PostgreSQL credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=InsureZenDBv3;Username=postgres;Password=YOUR_POSTGRES_PASSWORD"
  }
}
```

For production, update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-production-host;Port=5432;Database=InsureZenDBv3;Username=produser;Password=SECURE_PASSWORD"
  },
  "JwtSettings": {
    "SecretKey": "your-very-long-secure-random-key-at-least-32-characters"
  }
}
```

## Database Setup

### Step 1: Create Database

Open pgAdmin or use psql to create the database:

```powershell
# Using psql
psql -U postgres -h localhost

# In psql terminal:
CREATE DATABASE "InsureZenDBv3";
\q
```

Or using pgAdmin GUI:
- Right-click on Databases → Create → Database
- Name: `InsureZenDBv3`
- Click Create

### Step 2: Run EF Core Migrations

The application automatically runs migrations on startup, but you can also run them manually:

```powershell
# Add a new migration (if you modify models)
dotnet ef migrations add InitialCreate

# Apply migrations
dotnet ef database update
```

### Step 3: Verify Database Creation

```powershell
# Connect to database
psql -U postgres -d InsureZenDBv3 -h localhost

# List tables
\dt

# You should see these tables:
# - AspNetUsers
# - InsuranceCompanies
# - Claims
# - ClaimReviews
# - AuditLogs
```

## Running the Application

### Development Mode

```powershell
# From project root
dotnet run --configuration Debug

# Or
dotnet run

# Output:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:5001
#       Now listening on: http://localhost:5000
```

### Production Mode

```powershell
dotnet publish -c Release
cd bin/Release/net10.0/publish
dotnet InsureZenv2.dll
```

### Access Application

- **API**: https://localhost:5001/api
- **Swagger UI**: https://localhost:5001/swagger/index.html
- **Health Check**: https://localhost:5001/health (if enabled)

## API Endpoints

### Authentication Flow

1. **Register a user**:
   ```bash
   POST /api/auth/register
   ```

2. **Login to get JWT token**:
   ```bash
   POST /api/auth/login
   ```

3. **Use token for subsequent requests**:
   ```bash
   Authorization: Bearer <jwt_token>
   ```

### Maker Workflow

1. **View pending claims**:
   ```bash
   GET /api/claims/maker/list
   ```

2. **Lock a claim**:
   ```bash
   POST /api/claims/{id}/lock/maker
   ```

3. **View claim details**:
   ```bash
   GET /api/claims/{id}
   ```

4. **Submit review and recommendation**:
   ```bash
   POST /api/claims/{id}/review/maker
   ```

### Checker Workflow

1. **View claims pending checker review**:
   ```bash
   GET /api/claims/checker/list
   ```

2. **Lock a claim**:
   ```bash
   POST /api/claims/{id}/lock/checker
   ```

3. **View claim details including maker review**:
   ```bash
   GET /api/claims/{id}
   ```

4. **Submit final decision**:
   ```bash
   POST /api/claims/{id}/review/checker
   ```

### Post-Review Actions

1. **Forward to insurance company**:
   ```bash
   POST /api/claims/{id}/forward-to-insurer
   ```

## Authentication & Authorization

### JWT Token Structure

```
Header: {
  "alg": "HS256",
  "typ": "JWT"
}

Payload: {
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "550e8400-e29b-41d4-a716-446655440000",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "maker1",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": "maker1@insurezen.com",
  "Role": "Maker",
  "InsuranceCompanyId": "550e8400-e29b-41d4-a716-446655440001",
  "exp": 1713693600,
  "iss": "InsureZen",
  "aud": "InsureZenAPI"
}
```

### Role-Based Authorization

- **Maker**: Can access maker-specific endpoints (`/claims/maker/*`)
- **Checker**: Can access checker-specific endpoints (`/claims/checker/*`)
- **Both Roles**: Can access view and ingest endpoints

### Policy Configuration

In `Program.cs`:
```csharp
options.AddPolicy("MakerOnly", policy =>
    policy.RequireClaim("Role", "Maker"));

options.AddPolicy("CheckerOnly", policy =>
    policy.RequireClaim("Role", "Checker"));
```

## Database Schema

### Tables

#### InsuranceCompanies
```sql
- Id (UUID, PK)
- Name (VARCHAR 255)
- Code (VARCHAR 50, UNIQUE)
- ContactEmail (VARCHAR 255)
- ContactPhone (VARCHAR 20)
- IsActive (BOOLEAN)
- CreatedAt (TIMESTAMP)
- UpdatedAt (TIMESTAMP, nullable)
```

#### Users
```sql
- Id (UUID, PK)
- Username (VARCHAR 255, UNIQUE)
- Email (VARCHAR 255, UNIQUE)
- PasswordHash (VARCHAR)
- Role (VARCHAR 50) - "Maker" or "Checker"
- InsuranceCompanyId (UUID, FK)
- IsActive (BOOLEAN)
- CreatedAt (TIMESTAMP)
- UpdatedAt (TIMESTAMP, nullable)
```

#### Claims
```sql
- Id (UUID, PK)
- ClaimNumber (VARCHAR 100, UNIQUE)
- InsuranceCompanyId (UUID, FK)
- PatientName (VARCHAR 255)
- PatientId (VARCHAR 100)
- ServiceDate (DATE)
- ClaimAmount (NUMERIC 18,2)
- ServiceDescription (TEXT)
- ProviderName (VARCHAR 255)
- ProviderCode (VARCHAR 100)
- Status (INT) - 0-6
- SubmittedAt (TIMESTAMP)
- CompletedAt (TIMESTAMP, nullable)
- LockedByUserId (UUID, FK, nullable)
- LockedAt (TIMESTAMP, nullable)
- CreatedAt (TIMESTAMP)
- UpdatedAt (TIMESTAMP, nullable)

Indexes:
- (Id, LockedByUserId)
- ClaimNumber (UNIQUE)
```

#### ClaimReviews
```sql
- Id (UUID, PK)
- ClaimId (UUID, FK)
- ReviewedByUserId (UUID, FK)
- IsMakerReview (BOOLEAN)
- Feedback (TEXT)
- MakerRecommendation (INT, nullable) - 0=Pending, 1=Approve, 2=Reject
- CheckerDecision (INT, nullable) - 0=Pending, 1=Approved, 2=Rejected
- ReviewedAt (TIMESTAMP)
- CreatedAt (TIMESTAMP)
- UpdatedAt (TIMESTAMP, nullable)

Indexes:
- (ClaimId, IsMakerReview)
```

#### AuditLogs
```sql
- Id (UUID, PK)
- ClaimId (UUID, FK)
- UserId (UUID, FK, nullable)
- Action (VARCHAR 255)
- Details (TEXT)
- CreatedAt (TIMESTAMP)

Indexes:
- ClaimId
- CreatedAt
```

## Assumptions

1. **Upstream Service Integration**: The application assumes claim data is already extracted and normalized by an upstream OCR/parsing service. The `ClaimIngestDto` represents the standardized input format.

2. **User Roles**: Only two user roles exist in the system:
   - `Maker`: Reviews claims and makes initial recommendations
   - `Checker`: Reviews maker recommendations and issues final decisions

3. **Single Insurance Company per User**: Each user is associated with exactly one insurance company for data isolation.

4. **Pessimistic Locking**: The system uses database-level pessimistic locking via `LockedByUserId` field to prevent concurrent review of the same claim.

5. **UTC Timestamps**: All timestamps are stored and returned in UTC timezone.

6. **JWT Expiration**:
   - Development: 24 hours
   - Production: 1 hour (configurable in appsettings)

7. **No Email Verification**: Users created via registration are immediately active. No email confirmation is required.

8. **Insurer Forwarding**: The `ForwardedToInsurer` status and audit log entry simulate forwarding to insurance company. No actual HTTP calls or message queue integration is implemented.

9. **Password Requirements**:
   - Minimum 8 characters
   - At least 1 uppercase letter
   - At least 1 lowercase letter
   - At least 1 digit
   - At least 1 special character (!@#$%^&*)

10. **Pagination Limits**:
    - Maximum page size: 100
    - Default page size: 10

11. **Concurrency Handling**: If a claim is already locked by another user, attempts to lock it fail with appropriate error message.

12. **State Transitions**: Only valid state transitions are allowed:
    ```
    Pending → MakerInProgress → MakerSubmitted → CheckerInProgress → Approved/Rejected → ForwardedToInsurer
    ```

## Testing

### Unit Tests

Create a test project:

```powershell
dotnet new xunit -n InsureZenv2.Tests
cd InsureZenv2.Tests
dotnet add reference ../InsureZenv2/InsureZenv2.csproj
```

### Example Test for Claim Service

```csharp
[Fact]
public async Task SubmitMakerReview_WithValidData_UpdatesClaimStatus()
{
    // Arrange
    var claim = new Claim 
    { 
        Id = Guid.NewGuid(),
        Status = ClaimStatus.MakerInProgress,
        LockedByUserId = userId
    };
    var dto = new MakerReviewSubmitDto 
    { 
        Feedback = "Valid",
        Recommendation = "Approve"
    };

    // Act
    var result = await _claimService.SubmitMakerReviewAsync(claim.Id, userId, dto);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(ClaimStatus.MakerSubmitted, claim.Status);
}
```

### Run Tests

```powershell
dotnet test
```

## Troubleshooting

### Connection String Errors

**Error**: `Exception: Unable to connect to database`

**Solution**:
1. Verify PostgreSQL is running: `pg_isready -h localhost -p 5432`
2. Check credentials in `appsettings.Development.json`
3. Ensure database `InsureZenDBv3` exists

```powershell
psql -U postgres -h localhost -c "SELECT datname FROM pg_database WHERE datname = 'InsureZenDBv3';"
```

### JWT Token Errors

**Error**: `401 Unauthorized`

**Solutions**:
1. Ensure token is included in Authorization header: `Authorization: Bearer <token>`
2. Verify token hasn't expired
3. Check JWT secret key matches in `appsettings.json`

### Migration Errors

**Error**: `Unable to create migrations folder`

**Solution**:
```powershell
# Ensure Migrations folder exists
mkdir Migrations

# Try again
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Port Already in Use

**Error**: `Address already in use 127.0.0.1:5001`

**Solution**:
```powershell
# Find process using port 5001
netstat -ano | findstr :5001

# Kill process
taskkill /PID <pid> /F

# Or use different port in launchSettings.json
```

### Password Hashing Errors

**Error**: `Invalid password hash format`

**Solution**: Ensure passwords are hashed using the `PasswordHasher` service before storing. Never store plain text passwords.

## Development Workflow

### Making Changes

1. **Create a feature branch**:
   ```powershell
   git checkout -b feature/your-feature-name
   ```

2. **Make changes** to appropriate files following the project structure

3. **Test locally**:
   ```powershell
   dotnet run
   dotnet test
   ```

4. **Commit and push**:
   ```powershell
   git add .
   git commit -m "feat: description of changes"
   git push origin feature/your-feature-name
   ```

5. **Create pull request** for review

### Adding Database Migrations

```powershell
# Add migration after changing models
dotnet ef migrations add DescriptionOfChange

# Remove last migration if needed
dotnet ef migrations remove

# Update database
dotnet ef database update
```

## Production Deployment

### Pre-Deployment Checklist

- [ ] Update `appsettings.json` with production database connection
- [ ] Update `appsettings.json` with secure JWT secret key
- [ ] Disable Swagger UI in production (`app.UseSwagger()` in Program.cs)
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Configure CORS appropriately for frontend URL
- [ ] Set up HTTPS certificates
- [ ] Configure application logging and monitoring

### Deployment Steps

```powershell
# Publish application
dotnet publish -c Release -o ./publish

# Copy to deployment server
# Set environment variables on server:
# ASPNETCORE_ENVIRONMENT=Production

# Run application
cd publish
dotnet InsureZenv2.dll
```

## Support & Questions

For issues or questions:
1. Check this README and Troubleshooting section
2. Review API response error messages
3. Check application logs in `logs/` directory
4. Review database connection and credentials

## License

Internal use only - InsureZen proprietary software

## Contributors

- Development Team: InsureZen
- Assessment Date: April 2026
