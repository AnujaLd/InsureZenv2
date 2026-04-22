# InsureZen Backend API - Complete Setup & Execution Guide

A comprehensive medical insurance claim management system with a two-stage review workflow (Maker and Checker) built with ASP.NET Core 10.0 and PostgreSQL.

> **⚡ NEW HERE?** Start with [SETUP_SUMMARY.md](SETUP_SUMMARY.md) for a quick 5-minute overview and fastest path to getting the app running!

## 📖 What This Project Is

This is a **complete, production-ready backend REST API** for medical insurance claim processing and management. It provides:

✅ **12 REST API Endpoints** - All fully implemented and working  
✅ **Two-Stage Review Workflow** - Maker → Checker approval process  
✅ **JWT Authentication** - Secure token-based authentication with role-based authorization  
✅ **Concurrent-Safe** - Pessimistic locking prevents simultaneous editing  
✅ **PostgreSQL Database** - Persistent data storage with Entity Framework Core ORM  
✅ **Complete Validation** - Input validation with FluentValidation  
✅ **Audit Logging** - Complete trail of all state changes  
✅ **Production Ready** - Error handling, logging, CORS, Swagger documentation  

**In ~15 minutes, you will have:**
- A fully functioning backend API running on your machine
- PostgreSQL database with all tables created
- 12 working API endpoints you can test via Swagger UI
- Complete ability to ingest, review, and manage insurance claims
- JWT authentication working with Maker and Checker roles

## Quick Navigation

- [🚀 Quick Start (5 minutes)](#quick-start--5-minutes)
- [📋 Complete A-to-Z Setup Guide](#complete-a-to-z-setup-guide)
- [🔧 Prerequisites & Installation](#prerequisites--installation)
- [💻 Running the Application](#running-the-application)
- [📡 API Endpoints - Complete List](#api-endpoints---complete-list)
- [🔄 Complete API Workflow Example](#complete-api-workflow-example)
- [🏗️ Project Structure](#project-structure)
- [📊 Requirements Analysis](#requirements-analysis)
- [🔐 Authentication & Authorization](#authentication--authorization)
- [📚 Database Schema](#database-schema)
- [✅ Assumptions](#assumptions)
- [🧪 Testing](#testing)
- [🐛 Troubleshooting](#troubleshooting)

## Quick Start (5 minutes)

For experienced developers who just want to run the app quickly:

```powershell
# 1. Install prerequisites (if not already installed)
winget install Microsoft.DotNet.SDK.10
winget install PostgreSQL.PostgreSQL

# 2. Clone and navigate
git clone https://github.com/AnujaLd/InsureZenv2
cd InsureZenv2

# 3. Create database
psql -h localhost -U postgres -c "CREATE DATABASE \"InsureZenDBv3\" ENCODING 'UTF8';"

# 4. Restore and migrate
dotnet restore
dotnet ef database update

# 5. Run the application
dotnet run

# 6. Access Swagger UI
# Open: https://localhost:5000/swagger/index.html
```

---

## Complete A-to-Z Setup Guide

This guide walks you through **every single step** needed to set up and run the application from scratch. Follow each section in order.

### Step 1: Install .NET SDK 10.0

**For Windows:**

```powershell
# Open PowerShell as Administrator
winget install Microsoft.DotNet.SDK.10

# Verify installation
dotnet --version

# Expected output: 10.0.x or higher
```

**For macOS:**
```bash
brew install dotnet
dotnet --version
```

**For Linux:**
```bash
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0
dotnet --version
```

### Step 2: Install PostgreSQL

**For Windows:**

```powershell
# Install PostgreSQL
winget install PostgreSQL.PostgreSQL

# During installation:
# - Choose installation directory (default: C:\Program Files\PostgreSQL\17)
# - Set postgres user password (default: postgres) - REMEMBER THIS
# - Port: 5432 (default)
# - Keep service running checkbox: YES
# - Set up Windows Firewall automatically: YES

# Verify installation after reboot
psql --version

# Expected output: psql (PostgreSQL) 17.x or higher
```

**For macOS:**
```bash
brew install postgresql@17
brew services start postgresql@17
psql --version
```

**For Linux:**
```bash
sudo apt-get update
sudo apt-get install -y postgresql postgresql-contrib

# Verify
psql --version
```

### Step 3: Clone the Repository

```powershell
# Navigate to where you want the project
cd C:\Projects  # or any location you prefer

# Clone the repository
git clone https://github.com/AnujaLd/InsureZenv2

# Navigate into project
cd InsureZenv2

# Verify you're in the right location
dir  # should show: Program.cs, src/, Pages/, appsettings.json, etc.
```

### Step 4: Verify Project Structure

After cloning, your directory structure should look like:

```
InsureZenv2/
├── src/
│   ├── Authentication/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Models/
│   ├── Repositories/
│   ├── Services/
│   └── Validators/
├── Migrations/
├── Pages/
├── Properties/
├── wwwroot/
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── InsureZenv2.csproj
└── README.md
```

### Step 5: Create the PostgreSQL Database

Open PowerShell and run:

```powershell
# Connect to PostgreSQL as the default postgres user
psql -h localhost -U postgres -c "CREATE DATABASE \"InsureZenDBv3\" ENCODING 'UTF8';"

# Verify the database was created
psql -h localhost -U postgres -c "\l"

# Expected output should show:
# InsureZenDBv3 | postgres | UTF8 | ...
```

### Step 6: Verify Connection String

Open `appsettings.Development.json` in your editor and verify:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=InsureZenDBv3;Username=postgres;Password=yourpassword"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-min-32-characters-long-change-in-production!",
    "Issuer": "InsureZen",
    "Audience": "InsureZenAPI",
    "ExpiresInMinutes": 1440
  }
}
```

**If your PostgreSQL password is different**, update the `Password=postgres` part accordingly.

### Step 7: Restore NuGet Packages

```powershell
# Navigate to project directory
cd d:\InsureZenv2

# Restore all NuGet packages
dotnet restore

# Expected output:
# Determining projects to restore...
# Restoring d:\InsureZenv2\InsureZenv2.csproj
# ...
# Restore completed in X.XXs
```

### Step 8: Run Database Migrations

```powershell
# This creates all tables in your database
dotnet ef database update

# Expected output:
# Applying migration '20260422044939_InitialCreate'.
# Done.
```

**If you see errors**, check:
- PostgreSQL service is running: `pg_isready -h localhost -p 5432`
- Database exists: `psql -h localhost -U postgres -l`
- Connection string in `appsettings.Development.json` is correct

### Step 9: Build the Application

```powershell
# Clean any previous builds
dotnet clean

# Build the application
dotnet build

# Expected output:
# Build succeeded.
# 0 Warning(s)
# 0 Error(s)
```

### Step 10: Run the Application

```powershell
# Start the application
dotnet run

# Expected output:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: http://localhost:5000
# info: Microsoft.Hosting.Lifetime[0]
#       Application started. Press Ctrl+C to exit
```

✅ **Application is now running!**

### Step 11: Test the Application (Web Browser)

Open your browser and navigate to:

```
http://localhost:5000/swagger/index.html
```

You should see the **Swagger UI** with all API endpoints listed.

**Note:** If you see a certificate warning, click "Continue" or add an exception (it's the self-signed development certificate).

---

## Prerequisites & Installation
### Required Software

| Software | Version | Download |
|----------|---------|----------|
| .NET SDK | 10.0+ | https://dotnet.microsoft.com/download |
| PostgreSQL | 13+ | https://www.postgresql.org/download/ |
| Git | Latest | https://git-scm.com/downloads |

### Installation Verification

After installing, verify everything works:

```powershell
# Check .NET
dotnet --version
# Expected: 10.0.x

# Check PostgreSQL
psql --version
# Expected: psql (PostgreSQL) 13.x or higher

# Check Git
git --version
# Expected: git version 2.x+
```

---

## Running the Application

### Development Environment

```powershell
# Terminal 1: Start the application
cd d:\InsureZenv2
dotnet run

### Stop the Application

Press `Ctrl+C` in the terminal where the application is running.

---

## Authentication Endpoints

### 1️⃣ POST /auth/register - Create New User Account

**Purpose**: Register a new Maker or Checker user

**Headers**:
```json
Content-Type: application/json
```

**Request Body**:
```json
{
  "username": "maker1",
  "email": "maker1@company.com",
  "password": "Password123!",
  "role": "Maker",
  "insuranceCompanyId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response (201 Created)**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "username": "maker1",
  "email": "maker1@company.com",
  "role": "Maker",
  "isActive": true
}
```

**Error Response (409 Conflict)**:
```json
{"message": "User already exists"}
```

**Validation Rules**:
- Username: Required, 3-50 characters, alphanumeric
- Email: Required, valid email format
- Password: Required, minimum 8 characters, must contain uppercase, lowercase, digit, special character
- Role: Must be "Maker" or "Checker"
- InsuranceCompanyId: Must be a valid UUID

---

### 2️⃣ POST /auth/login - Login & Get JWT Token

**Purpose**: Authenticate user and receive JWT token for subsequent requests

**Headers**:
```json
Content-Type: application/json
```

**Request Body**:
```json
{
  "username": "maker1",
  "password": "Password123!"
}
```

**Response (200 OK)**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJyb25ueSIsIkNvbXBhbnlJZCI6IjU1MGU4NDAwLWUyOWItNDFkNC1hNzE2LTQ0NjY1NTQ0MDAwMCIsIlJvbGUiOiJNYWtlciIsImlhdCI6MTcxMzc5NDgyNSwibmJmIjoxNzEzNzk0ODI1LCJleHAiOjE3MTM3OTg0MjUsImlzcyI6Ikluc3VyZVplbiIsImF1ZCI6Ikluc3VyZVplbkFQSSJ9.xxx",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "username": "maker1",
    "email": "maker1@company.com",
    "role": "Maker"
  },
  "expiresIn": 3600
}
```

**Error Response (401 Unauthorized)**:
```json
{"message": "Invalid username or password"}
```

---

## Claim Management Endpoints

### 3️⃣ POST /claims/ingest - Submit New Claim

**Purpose**: Ingest a new insurance claim into the system

**Headers**:
```json
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

**Request Body**:
```json
{
  "patientName": "John Doe",
  "patientId": "PAT-001",
  "serviceDate": "2026-04-15",
  "claimAmount": 1500.00,
  "serviceDescription": "Emergency Room",
  "providerName": "City Hospital",
  "providerCode": "PROV-001"
}
```

**Response (201 Created)**:
```json
{
  "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "claimNumber": "CLM-20260421-ABC123",
  "patientName": "John Doe",
  "patientId": "PAT-001",
  "serviceDate": "2026-04-15",
  "claimAmount": 1500.00,
  "serviceDescription": "Emergency Room",
  "providerName": "City Hospital",
  "providerCode": "PROV-001",
  "status": "Pending",
  "submittedAt": "2026-04-21T10:30:00Z",
  "completedAt": null
}
```

**Error Response (400 Bad Request)**:
```json
{
  "errors": [
    "Patient name is required",
    "Claim amount must be greater than 0"
  ]
}
```

**Validation Rules**:
- patientName: Required, max 100 characters
- patientId: Required, max 50 characters
- serviceDate: Required, must be valid date
- claimAmount: Required, must be > 0
- serviceDescription: Required, max 500 characters
- providerName: Required, max 100 characters
- providerCode: Required, max 50 characters

---

### 4️⃣ GET /claims/{id} - Get Claim Details

**Purpose**: Retrieve full details of a specific claim including all reviews

**Headers**:
```json
Authorization: Bearer {jwt_token}
```

**Path Parameters**:
- `id`: Claim UUID (e.g., `f47ac10b-58cc-4372-a567-0e02b2c3d479`)

**Response (200 OK)**:
```json
{
  "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "claimNumber": "CLM-20260421-ABC123",
  "patientName": "John Doe",
  "patientId": "PAT-001",
  "serviceDate": "2026-04-15",
  "claimAmount": 1500.00,
  "serviceDescription": "Emergency Room",
  "providerName": "City Hospital",
  "providerCode": "PROV-001",
  "status": "Approved",
  "submittedAt": "2026-04-21T10:30:00Z",
  "completedAt": "2026-04-21T14:45:00Z",
  "insuranceCompanyId": "550e8400-e29b-41d4-a716-446655440000",
  "makerReview": {
    "id": "550e8400-e29b-41d4-a716-446655440003",
    "reviewedByUsername": "maker1",
    "feedback": "All documents verified",
    "makerRecommendation": "Approve",
    "reviewedAt": "2026-04-21T11:00:00Z"
  },
  "checkerReview": {
    "id": "550e8400-e29b-41d4-a716-446655440004",
    "reviewedByUsername": "checker1",
    "feedback": "Concur with maker",
    "checkerDecision": "Approved",
    "reviewedAt": "2026-04-21T14:45:00Z"
  }
}
```

**Error Response (404 Not Found)**:
```json
{"message": "Claim not found"}
```

---

### 5️⃣ GET /claims/maker/list - Get Claims for Maker Review

**Purpose**: Retrieve paginated list of claims available for Maker review

**Headers**:
```json
Authorization: Bearer {jwt_token}
```

**Query Parameters**:
- `pageNumber` (optional): Default 1, minimum 1
- `pageSize` (optional): Default 10, maximum 100
- `status` (optional): "Pending" or "MakerInProgress"

**Response (200 OK)**:
```json
{
  "claims": [
    {
      "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
      "claimNumber": "CLM-20260421-ABC123",
      "patientName": "John Doe",
      "patientId": "PAT-001",
      "serviceDate": "2026-04-15",
      "claimAmount": 1500.00,
      "status": "Pending"
    },
    {
      "id": "a47ac10b-58cc-4372-a567-0e02b2c3d489",
      "claimNumber": "CLM-20260421-DEF456",
      "patientName": "Jane Smith",
      "patientId": "PAT-002",
      "serviceDate": "2026-04-16",
      "claimAmount": 2500.00,
      "status": "Pending"
    }
  ],
  "totalCount": 25,
  "pageNumber": 1,
  "pageSize": 10
}
```

**Error Response (403 Forbidden)**:
```json
{"message": "Access denied - Maker role required"}
```

---

### 6️⃣ GET /claims/checker/list - Get Claims for Checker Review

**Purpose**: Retrieve paginated list of claims available for Checker review

**Headers**:
```json
Authorization: Bearer {jwt_token}
```

**Query Parameters**:
- `pageNumber` (optional): Default 1, minimum 1
- `pageSize` (optional): Default 10, maximum 100
- `status` (optional): "MakerSubmitted" or "CheckerInProgress"

**Example Request**:

**Response (200 OK)**:
```json
{
  "claims": [
    {
      "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
      "claimNumber": "CLM-20260421-ABC123",
      "patientName": "John Doe",
      "patientId": "PAT-001",
      "serviceDate": "2026-04-15",
      "claimAmount": 1500.00,
      "status": "MakerSubmitted"
    }
  ],
  "totalCount": 5,
  "pageNumber": 1,
  "pageSize": 10
}
```

**Error Response (403 Forbidden)**:
```json
{"message": "Access denied - Checker role required"}
```

---

### 7️⃣ POST /claims/{id}/lock/maker - Lock Claim for Maker Review

**Purpose**: Lock a claim so only this Maker can review it

**Headers**:
```json
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

**Path Parameters**:
- `id`: Claim UUID

**Response (200 OK)**:
```json
{"message": "Claim locked successfully"}
```

**Error Response (400 Bad Request)**:
```json
{"message": "Cannot lock claim - already locked by another user"}
```

**Error Response (404 Not Found)**:
```json
{"message": "Claim not found"}
```

---

### 8️⃣ POST /claims/{id}/lock/checker - Lock Claim for Checker Review

**Purpose**: Lock a claim so only this Checker can review it

**Headers**:
```json
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

**Path Parameters**:
- `id`: Claim UUID

**Response (200 OK)**:
```json
{"message": "Claim locked successfully"}
```

**Error Response (400 Bad Request)**:
```json
{"message": "Cannot lock claim - must be in MakerSubmitted status"}
```

---

### 9️⃣ POST /claims/{id}/unlock - Unlock Claim

**Purpose**: Unlock a claim to release the lock

**Headers**:
```json
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

**Path Parameters**:
- `id`: Claim UUID

**Response (200 OK)**:
```json
{"message": "Claim unlocked successfully"}
```

**Error Response (404 Not Found)**:
```json
{"message": "Claim not found"}
```

---

### 🔟 POST /claims/{id}/review/maker - Submit Maker Review

**Purpose**: Submit Maker's review feedback and recommendation

**Headers**:
```json
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

**Path Parameters**:
- `id`: Claim UUID

**Request Body**:
```json
{
  "feedback": "All documents verified and complete",
  "recommendation": "Approve"
}
```

**Valid Recommendations**: `"Approve"` or `"Reject"`

**Response (200 OK)**:
```json
{
  "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "claimNumber": "CLM-20260421-ABC123",
  "status": "MakerSubmitted",
  "makerReview": {
    "id": "550e8400-e29b-41d4-a716-446655440003",
    "reviewedByUsername": "maker1",
    "feedback": "All documents verified and complete",
    "makerRecommendation": "Approve",
    "reviewedAt": "2026-04-21T11:00:00Z"
  }
}
```

**Error Response (403 Forbidden)**:
```json
{"message": "You do not have permission to review this claim"}
```

**Error Response (400 Bad Request)**:
```json
{
  "errors": [
    "Feedback is required",
    "Recommendation must be 'Approve' or 'Reject'"
  ]
}
```

---

### 1️⃣1️⃣ POST /claims/{id}/review/checker - Submit Checker Final Decision

**Purpose**: Submit Checker's final decision on the claim

**Headers**:
```json
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

**Path Parameters**:
- `id`: Claim UUID

**Request Body**:
```json
{
  "feedback": "Reviewed maker's recommendation and concur",
  "decision": "Approved"
}
```

**Valid Decisions**: `"Approved"` or `"Rejected"`

**Response (200 OK)**:
```json
{
  "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "claimNumber": "CLM-20260421-ABC123",
  "status": "Approved",
  "completedAt": "2026-04-21T14:45:00Z",
  "checkerReview": {
    "id": "550e8400-e29b-41d4-a716-446655440004",
    "reviewedByUsername": "checker1",
    "feedback": "Reviewed maker's recommendation and concur",
    "checkerDecision": "Approved",
    "reviewedAt": "2026-04-21T14:45:00Z"
  }
}
```

**Error Response (403 Forbidden)**:
```json
{"message": "You do not have permission to review this claim"}
```

**Error Response (400 Bad Request)**:
```json
{
  "errors": [
    "Feedback is required",
    "Decision must be 'Approved' or 'Rejected'"
  ]
}
```

---

### 1️⃣2️⃣ POST /claims/{id}/forward-to-insurer - Forward Claim to Insurance Company

**Purpose**: Forward completed claim to the insurance company

**Headers**:
```json
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

**Path Parameters**:
- `id`: Claim UUID

**Response (200 OK)**:
```json
{"message": "Claim forwarded to insurer successfully"}
```

**Error Response (400 Bad Request)**:
```json
{"message": "Claim must be in Approved or Rejected status before forwarding"}
```

**Error Response (404 Not Found)**:
```json
{"message": "Claim not found"}
```

---

## Complete API Workflow Example

Follow these steps **in order** to test the entire system end-to-end:

### Prerequisites
- PostgreSQL database is created and populated with an insurance company

### Step 1: Create Insurance Company in Database

```powershell
# Connect to PostgreSQL
psql -h localhost -U postgres -d InsureZenDBv3

# Run this SQL command in the psql prompt:
INSERT INTO "InsuranceCompanies" ("Id", "Name", "Code", "ContactEmail", "ContactPhone", "IsActive", "CreatedAt")
VALUES ('550e8400-e29b-41d4-a716-446655440000'::uuid, 'Test Insurance Co', 'TEST-001', 'test@company.com', '555-1234', true, NOW());

# Verify
SELECT * FROM "InsuranceCompanies";

# Exit psql
\q
```

## Requirements Analysis

### Problem Statement Summary

InsureZen digitizes medical insurance claim processing for multiple insurance companies. Claims are ingested as structured data, then flow through a two-stage human review process:

1. **Maker Phase**: InsureZen employee reviews extracted data and recommends approval/rejection
2. **Checker Phase**: Second employee reviews the claim and Maker recommendation, then issues final decision
3. **Forwarding**: Completed claim is forwarded to the insurance company

The system must support hundreds to thousands of concurrent claims with multiple Makers and Checkers working simultaneously.

### Domain Entities

#### 1. InsuranceCompany
- **Primary Key**: UUID `Id`
- **Fields**: Name, Code, ContactEmail, ContactPhone, IsActive, CreatedAt
- **Purpose**: Partner insurance companies that use the system
- **Relationships**: One insurance company has many users

#### 2. User
- **Primary Key**: UUID `Id`
- **Fields**: Username, Email, PasswordHash, Role, IsActive, InsuranceCompanyId, CreatedAt
- **Purpose**: InsureZen employees who review claims
- **Roles**: "Maker" or "Checker"
- **Constraints**: Username and Email must be unique
- **Relationships**: Belongs to one insurance company

#### 3. Claim
- **Primary Key**: UUID `Id`
- **Fields**: 
  - `ClaimNumber`: Auto-generated unique identifier (CLM-YYYYMMDD-XXXXXX)
  - `PatientName`, `PatientId`: Patient information
  - `ServiceDate`: Date service was provided
  - `ClaimAmount`: Requested claim amount (decimal, > 0)
  - `ServiceDescription`: Description of service provided
  - `ProviderName`, `ProviderCode`: Healthcare provider details
  - `Status`: Current state (Pending, MakerInProgress, MakerSubmitted, CheckerInProgress, Approved, Rejected, ForwardedToInsurer)
  - `LockedByUserId`: UUID of user currently reviewing (if locked)
  - `InsuranceCompanyId`: Which company this claim belongs to
  - `SubmittedAt`, `CompletedAt`: Timestamps
  - `CreatedAt`: Claim ingestion timestamp
- **Relationships**: One claim has one maker review and one checker review

#### 4. ClaimReview
- **Primary Key**: UUID `Id`
- **Fields**:
  - `ClaimId`: UUID reference to claim
  - `ReviewedByUserId`: UUID of reviewer (Maker or Checker)
  - `ReviewedByUsername`: Username for easy reference
  - `Feedback`: Review notes and comments
  - `MakerRecommendation`: "Approve" or "Reject" (Maker reviews only)
  - `CheckerDecision`: "Approved" or "Rejected" (Checker reviews only)
  - `ReviewedAt`: Timestamp of review submission
- **Type Differentiation**: Uses different fields for Maker vs Checker reviews
- **Relationships**: Belongs to one claim

#### 5. AuditLog
- **Primary Key**: UUID `Id`
- **Fields**:
  - `ClaimId`: UUID of related claim
  - `UserId`: UUID of user who performed action
  - `Action`: Type of action (e.g., "LockClaim", "SubmitReview", "ForwardToInsurer")
  - `Description`: Human-readable description
  - `Timestamp`: When action occurred
  - `OldValue`, `NewValue`: For tracking state changes
- **Purpose**: Complete audit trail for compliance and debugging

### Actors & Roles

#### Maker Role
- **Responsibilities**:
  - View list of pending claims needing review
  - Lock a claim for review
  - Review claim data and extracted information
  - Add feedback notes
  - Recommend approval or rejection
  - Submit recommendation
  - View own submitted reviews
- **Constraints**:
  - Can only review claims in "Pending" or "MakerInProgress" status
  - Can only submit review on claims locked by themselves
  - Cannot see Checker reviews before submission
- **API Access**: GET /claims/maker/list, POST /claims/{id}/lock/maker, POST /claims/{id}/review/maker

#### Checker Role
- **Responsibilities**:
  - View list of claims with Maker recommendations
  - Lock a claim for review
  - Review claim data AND Maker's recommendation/feedback
  - Add own feedback notes
  - Issue final decision (Approved/Rejected)
  - Submit final decision
  - View Maker reviews alongside claim
- **Constraints**:
  - Can only review claims in "MakerSubmitted" status
  - Can only submit review on claims locked by themselves
  - Must consider Maker recommendation
- **API Access**: GET /claims/checker/list, POST /claims/{id}/lock/checker, POST /claims/{id}/review/checker

#### System (Backend)
- **Responsibilities**:
  - Manage state transitions
  - Enforce validation rules
  - Lock/unlock claims
  - Forward completed claims
  - Log all actions
  - Prevent concurrent access conflicts

### Functional Requirements

| # | Requirement | Description | Priority |
|---|---|---|---|
| FR1 | Claim Ingestion | Accept structured claim data from upstream service | MUST |
| FR2 | Authentication | JWT-based login with username/password | MUST |
| FR3 | Authorization | Role-based access (Maker vs Checker) | MUST |
| FR4 | Claim Listing | View paginated claims based on role and status | MUST |
| FR5 | Claim Locking | Pessimistic lock to prevent concurrent reviews | MUST |
| FR6 | Maker Review | Submit recommendation with feedback | MUST |
| FR7 | Checker Review | Submit final decision with feedback | MUST |
| FR8 | Claim Unlocking | Release lock on claims | SHOULD |
| FR9 | Claim Details | Retrieve full claim with all reviews | MUST |
| FR10 | Claim History | View past claims with filters | SHOULD |
| FR11 | Forwarding | Mark claim as forwarded to insurer | MUST |
| FR12 | Audit Trail | Log all state changes | SHOULD |
| FR13 | Input Validation | Validate all inputs with meaningful errors | MUST |
| FR14 | Error Handling | Consistent error response format | MUST |

### Non-Functional Requirements

| Requirement | Description | Target |
|---|---|---|
| **Concurrency** | Multiple Makers/Checkers can work simultaneously | Pessimistic locking with database constraints |
| **Data Consistency** | State transitions must be atomic | Database transactions |
| **Auditability** | All actions tracked with user and timestamp | AuditLog table with indexes |
| **Performance** | Sub-second response times for typical queries | Database indexes on frequently queried fields |
| **Security** | Password hashing, JWT validation, HTTPS | PBKDF2, HS256, SSL/TLS |
| **Scalability** | Support hundreds of concurrent users | Stateless API design, connection pooling |
| **Availability** | Minimal downtime for deployments | Graceful shutdown handling |
| **Logging** | All errors and important events logged | Serilog with file and console sinks |

### Claim State Diagram

```
                    ┌─────────────┐
                    │   Pending   │ ← Claim ingested
                    └──────┬──────┘
                           │ Maker locks claim
                    ┌──────▼──────────────────┐
                    │ MakerInProgress        │
                    │ (Locked by Maker ID)   │
                    └──────┬──────────────────┘
                           │ Maker submits review
                    ┌──────▼──────────────┐
                    │  MakerSubmitted     │
                    │ (Unlock implicit)   │
                    └──────┬──────────────┘
                           │ Checker locks claim
                    ┌──────▼─────────────────────┐
                    │ CheckerInProgress         │
                    │ (Locked by Checker ID)    │
                    └──────┬─────────────────────┘
                           │ Checker submits decision
                    ┌──────▼──────────┐
                    │   Approved      │ ← Final decision
                    └────────┬────────┘
                             │ Forward to insurer
                    ┌────────▼──────────────────┐
                    │  ForwardedToInsurer      │
                    │ (End state)              │
                    └──────────────────────────┘
                           
        OR
                    ┌──────────────┐
                    │  Rejected    │ ← Alternative path
                    └────────┬─────┘
                             │ Forward to insurer
                    ┌────────▼──────────────────┐
                    │  ForwardedToInsurer      │
                    │ (End state)              │
                    └──────────────────────────┘
```

### Assumptions

1. **Upstream Service Responsibility**: A separate OCR/form parsing service extracts fields and provides structured JSON. This API does NOT perform OCR.

2. **User Roles**: Only two roles exist in the system - "Maker" and "Checker". No other roles like Admin.

3. **Concurrency Model**: Uses pessimistic locking (database row lock) via `LockedByUserId` field. When user locks a claim, only they can review it until they unlock or submit review.

4. **Timezone Convention**: All timestamps stored and returned as UTC. Client responsible for timezone conversion.

5. **Insurance Company Association**: 
   - Each user belongs to exactly one insurance company
   - Claims created in Maker's insurance company context
   - Makers/Checkers can only see claims from their company

6. **Token Expiration**: 
   - Development: 24 hours
   - Production: 1 hour
   - No refresh token mechanism

7. **User Activation**: 
   - Users become active immediately on registration
   - No email verification required
   - No manual admin approval needed

8. **Insurance Company Forwarding**: 
   - No actual external API call to insurance company
   - System logs the forwarding action to database
   - Third-party integration can read ForwardedToInsurer status

9. **Claim Immutability**: 
   - Original claim data cannot be edited after ingestion
   - Corrections must be tracked in review feedback
   - Only reviews and status can change

10. **Password Policy**:
    - Minimum 8 characters
    - Must contain uppercase, lowercase, digit, and special character
    - Stored as PBKDF2 hash, never in plain text

## API Design Reference

### Authentication

All endpoints except `/auth/login` and `/auth/register` require JWT Bearer token:

```
Authorization: Bearer <jwt_token>
```

### Common Headers

```json
{
  "Content-Type": "application/json",
  "Authorization": "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

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

## Project Structure

```
InsureZenv2/
├── src/
│   ├── Authentication/
│   │   ├── JwtTokenService.cs              # JWT token generation and validation
│   │   │   ├── GenerateToken(user)         # Creates JWT token from user claims
│   │   │   └── ValidateToken(token)        # Validates and decodes JWT
│   │   └── PasswordHasher.cs               # Password hashing with PBKDF2
│   │       ├── HashPassword(password)      # Hashes password securely
│   │       └── VerifyPassword(pass, hash)  # Verifies password against hash
│   │
│   ├── Controllers/
│   │   ├── AuthController.cs               # Authentication endpoints
│   │   │   ├── POST /auth/register         # Register new user
│   │   │   └── POST /auth/login            # Login and get JWT
│   │   │
│   │   └── ClaimsController.cs             # Claim management endpoints
│   │       ├── POST /claims/ingest         # Ingest new claim
│   │       ├── GET /claims/{id}            # Get claim details
│   │       ├── GET /claims/maker/list      # List claims for maker
│   │       ├── GET /claims/checker/list    # List claims for checker
│   │       ├── POST /claims/{id}/lock/*    # Lock claim
│   │       ├── POST /claims/{id}/unlock    # Unlock claim
│   │       ├── POST /claims/{id}/review/*  # Submit review
│   │       └── POST /claims/{id}/forward   # Forward to insurer
│   │
│   ├── Data/
│   │   └── ApplicationDbContext.cs         # EF Core DbContext
│   │       ├── DbSet<InsuranceCompany>
│   │       ├── DbSet<User>
│   │       ├── DbSet<Claim>
│   │       ├── DbSet<ClaimReview>
│   │       └── DbSet<AuditLog>
│   │
│   ├── DTOs/
│   │   ├── ClaimDtos.cs
│   │   │   ├── ClaimIngestDto               # Request for claim ingestion
│   │   │   ├── ClaimDetailDto              # Full claim response
│   │   │   └── ClaimListItemDto            # Claim list response item
│   │   ├── UserDtos.cs
│   │   │   ├── UserRegisterDto
│   │   │   ├── UserLoginDto
│   │   │   └── UserResponseDto
│   │   └── ReviewDtos.cs
│   │       ├── MakerReviewSubmitDto
│   │       └── CheckerReviewSubmitDto
│   │
│   ├── Models/ (Domain Models)
│   │   ├── InsuranceCompany.cs             # Insurance company entity
│   │   ├── User.cs                         # User/employee entity
│   │   ├── Claim.cs                        # Insurance claim entity
│   │   ├── ClaimReview.cs                  # Review entity (Maker/Checker)
│   │   ├── AuditLog.cs                     # Audit trail entity
│   │   └── Enums.cs                        # Enumerations
│   │       ├── ClaimStatus                 # Pending, MakerInProgress, etc.
│   │       ├── UserRole                    # Maker, Checker
│   │       └── ReviewDecision              # Approve, Reject
│   │
│   ├── Repositories/ (Data Access Layer)
│   │   ├── IClaimRepository                # Contract
│   │   │   ├── GetById(id)
│   │   │   ├── GetPendingForMaker()
│   │   │   ├── GetForCheckerReview()
│   │   │   └── SaveAsync(claim)
│   │   ├── ClaimRepository.cs              # Implementation
│   │   ├── UserRepository.cs
│   │   ├── ClaimReviewRepository.cs
│   │   └── AuditLogRepository.cs
│   │
│   ├── Services/ (Business Logic Layer)
│   │   ├── IClaimService
│   │   │   ├── IngestClaimAsync(dto)
│   │   │   ├── LockClaimAsync(id, userId)
│   │   │   ├── SubmitMakerReviewAsync()
│   │   │   ├── SubmitCheckerReviewAsync()
│   │   │   └── ForwardToInsurerAsync(id)
│   │   ├── ClaimService.cs
│   │   ├── IAuthenticationService
│   │   │   ├── RegisterAsync(dto)
│   │   │   └── LoginAsync(dto)
│   │   └── AuthenticationService.cs
│   │
│   ├── Validators/ (Input Validation)
│   │   ├── ClaimIngestDtoValidator.cs
│   │   ├── MakerReviewSubmitDtoValidator.cs
│   │   ├── CheckerReviewSubmitDtoValidator.cs
│   │   ├── UserLoginDtoValidator.cs
│   │   └── UserRegisterDtoValidator.cs
│   │
│   └── Mappers/
│       └── MappingProfile.cs               # AutoMapper DTO ↔ Model mappings
│
├── Migrations/                              # EF Core migrations (auto-generated)
│   ├── 20260422044939_InitialCreate.cs
│   ├── 20260422044939_InitialCreate.Designer.cs
│   └── ApplicationDbContextModelSnapshot.cs
│
├── Pages/                                   # Razor pages (frontend, not used in API)
│   ├── Index.cshtml
│   ├── Privacy.cshtml
│   └── Shared/
│       └── _Layout.cshtml
│
├── Properties/
│   └── launchSettings.json                 # Development launch configuration
│
├── logs/                                    # Application logs (auto-generated)
│   └── app-*.txt
│
├── bin/ & obj/                             # Build artifacts (auto-generated)
│
├── Program.cs                              # 🔑 Application startup
│                                           # - Service registration
│                                           # - Middleware pipeline
│                                           # - Database migration
│                                           # - Authentication setup
│                                           # - Swagger configuration
│
├── appsettings.json                        # Production configuration
├── appsettings.Development.json            # Development configuration
├── InsureZenv2.csproj                      # Project file with NuGet deps
├── .gitignore                              # Git ignore rules
└── README.md                               # This comprehensive guide
```

### Folder Explanations

| Folder | Purpose |
|--------|---------|
| `src/` | Main application source code |
| `src/Authentication/` | JWT and password handling |
| `src/Controllers/` | HTTP API endpoints |
| `src/Data/` | Database context and configuration |
| `src/DTOs/` | Data Transfer Objects for API contracts |
| `src/Models/` | Domain entities (database tables) |
| `src/Repositories/` | Data access abstraction layer |
| `src/Services/` | Business logic and workflows |
| `src/Validators/` | Input validation rules |
| `src/Mappers/` | Object-to-object mapping |
| `Migrations/` | Database schema version control |
| `Pages/` | Razor UI pages (not used for API) |
| `wwwroot/` | Static files (CSS, JS, images) |
| `logs/` | Application logs directory |

### Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                        HTTP Requests                         │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────────────────────────────────────────┐   │
│  │           Controllers (API Layer)                    │   │
│  │  • Parse HTTP requests                              │   │
│  │  • Validate inputs                                  │   │
│  │  • Route to business logic                          │   │
│  └────────────────────┬─────────────────────────────────┘   │
│                       │                                       │
│  ┌────────────────────▼─────────────────────────────────┐   │
│  │      Services (Business Logic Layer)                 │   │
│  │  • Orchestrate workflows                            │   │
│  │  • Enforce business rules                           │   │
│  │  • Manage state transitions                         │   │
│  └────────────────────┬─────────────────────────────────┘   │
│                       │                                       │
│  ┌────────────────────▼─────────────────────────────────┐   │
│  │    Repositories (Data Access Layer)                  │   │
│  │  • Abstract database operations                      │   │
│  │  • Query and save entities                          │   │
│  │  • Handle transactions                              │   │
│  └────────────────────┬─────────────────────────────────┘   │
│                       │                                       │
│  ┌────────────────────▼─────────────────────────────────┐   │
│  │    Entity Framework Core                             │   │
│  │  • ORM mapping                                       │   │
│  │  • Migrations                                        │   │
│  └────────────────────┬─────────────────────────────────┘   │
│                       │                                       │
│  ┌────────────────────▼─────────────────────────────────┐   │
│  │           PostgreSQL Database                        │   │
│  │  • InsuranceCompanies table                         │   │
│  │  • Users table                                       │   │
│  │  • Claims table                                      │   │
│  │  • ClaimReviews table                               │   │
│  │  • AuditLogs table                                  │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                               │
└─────────────────────────────────────────────────────────────┘
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
#       Now listening on: http://localhost:5000
```

### Production Mode

```powershell
dotnet publish -c Release
cd bin/Release/net10.0/publish
dotnet InsureZenv2.dll
```

### Access Application
- **Swagger UI**: http://localhost:5000/swagger/index.html

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