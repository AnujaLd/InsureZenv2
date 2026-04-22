# InsureZen - Setup & API Workflow Summary

This is a **medical insurance claim processing backend** built with asp .NET  and PostgreSQL. It has:
- **12 REST API endpoints** (all working)
- **JWT authentication** with role-based access (Maker vs Checker)
- **Complete workflow**: Claims → Maker Review → Checker Review → Forward to Insurer
- **Concurrent safety**: Multiple users can work simultaneously without conflicts

---

## FASTEST PATH TO SUCCESS (10 minutes)

### 1. Install Two Things
```powershell
# If not already installed on your machine:
winget install Microsoft.DotNet.SDK.10
winget install PostgreSQL.PostgreSQL
```

### 2. Create Database
```powershell
# When PostgreSQL prompts, default password is "postgres"
psql -h localhost -U postgres -c "CREATE DATABASE \"InsureZenDBv3\" ENCODING 'UTF8';"
```

### 3. Navigate to Project
```powershell
cd d:\InsureZenv2
```

### 4. Setup & Run
```powershell
dotnet restore
dotnet ef database update
dotnet run
```

### 5. Test It
Open in browser: `https://localhost:5000/swagger/index.html`

✅ **Done!** You can now call all 12 API endpoints.

---

## API ENDPOINTS SUMMARY

### Authentication (2 endpoints - everyone)
1. `POST /auth/register` - Create new user (Maker or Checker)
2. `POST /auth/login` - Login and get JWT token

### Claims Core (3 endpoints - everyone)
3. `POST /claims/ingest` - Add new claim to system
4. `GET /claims/{id}` - Get full claim details
5. `GET /claims/maker/list` - View claims for maker review
6. `GET /claims/checker/list` - View claims for checker review

### Claim Locking (3 endpoints - both roles)
7. `POST /claims/{id}/lock/maker` - Lock claim as maker
8. `POST /claims/{id}/lock/checker` - Lock claim as checker
9. `POST /claims/{id}/unlock` - Release lock

### Reviews (2 endpoints - each role)
10. `POST /claims/{id}/review/maker` - Submit maker review & recommendation
11. `POST /claims/{id}/review/checker` - Submit checker final decision

### Completion (1 endpoint - both roles)
12. `POST /claims/{id}/forward-to-insurer` - Mark claim as sent to insurer

---

## STEP-BY-STEP WORKFLOW IN API CALLS

### The Happy Path (What a Real Workflow Looks Like)

```
1. Create Insurance Company (database directly, one-time)
   INSERT INTO "InsuranceCompanies" VALUES (...)

2. Register Maker User
   POST /auth/register → maker1 (Maker role)

3. Register Checker User
   POST /auth/register → checker1 (Checker role)

4. Maker Logs In
   POST /auth/login → Get JWT token for maker1

5. Submit a Claim
   POST /claims/ingest → Claim created with Status: Pending

6. Maker Lists Available Claims
   GET /claims/maker/list → Sees Pending claims

7. Maker Locks Claim
   POST /claims/{id}/lock/maker → Status: MakerInProgress

8. Maker Reviews Claim
   POST /claims/{id}/review/maker → Status: MakerSubmitted
   Recommendation: "Approve" or "Reject"

9. Checker Logs In
   POST /auth/login → Get JWT token for checker1

10. Checker Lists Available Claims
    GET /claims/checker/list → Sees MakerSubmitted claims

11. Checker Locks Claim
    POST /claims/{id}/lock/checker → Status: CheckerInProgress

12. Checker Views Claim (Including Maker Review)
    GET /claims/{id} → See maker review + feedback

13. Checker Issues Decision
    POST /claims/{id}/review/checker → Status: Approved/Rejected
    Decision: "Approved" or "Rejected"

14. Forward to Insurance Company
    POST /claims/{id}/forward-to-insurer → Status: ForwardedToInsurer
    ✅ Claim workflow complete!
```

---

## DATABASE TABLES

The system creates 5 tables:

```
InsuranceCompanies
├─ Id, Name, Code, ContactEmail, ContactPhone, IsActive, CreatedAt

Users
├─ Id, Username, Email, PasswordHash, Role, InsuranceCompanyId
├─ IsActive, CreatedAt

Claims
├─ Id, ClaimNumber, PatientName, PatientId, ServiceDate
├─ ClaimAmount, ServiceDescription, ProviderName, ProviderCode
├─ Status, LockedByUserId, InsuranceCompanyId
├─ SubmittedAt, CompletedAt, CreatedAt

ClaimReviews
├─ Id, ClaimId, ReviewedByUserId, ReviewedByUsername
├─ Feedback, MakerRecommendation, CheckerDecision, ReviewedAt

AuditLogs
├─ Id, ClaimId, UserId, Action, Details, CreatedAt
```

---

## CLAIM STATUS FLOW

Every claim goes through these statuses:

```
Pending
  ↓ (Maker locks)
MakerInProgress
  ↓ (Maker submits review)
MakerSubmitted
  ↓ (Checker locks)
CheckerInProgress
  ↓ (Checker submits decision)
Approved          OR          Rejected
  ↓                             ↓
ForwardedToInsurer     ForwardedToInsurer
  ✅ Done!                       ✅ Done!
```

---

## ROLES & PERMISSIONS

### Maker Role
- Can see: Pending and MakerInProgress claims
- Can do: Lock, review, recommend (Approve/Reject)
- Cannot: See Checker reviews, make final decisions

### Checker Role
- Can see: MakerSubmitted and CheckerInProgress claims
- Can do: Lock, review, issue final decision (Approved/Rejected)
- Cannot: Make initial recommendations, change claims after review
- CAN: See Maker's feedback and recommendation

---

## COMMON TESTING COMMANDS

### Create Insurance Company (SQL - one time)
```powershell
psql -h localhost -U postgres -d InsureZenDBv3
INSERT INTO "InsuranceCompanies" ("Id", "Name", "Code", "ContactEmail", "ContactPhone", "IsActive", "CreatedAt")
VALUES ('550e8400-e29b-41d4-a716-446655440000'::uuid, 'Test Insurance', 'TEST-001', 'test@company.com', '555-1234', true, NOW());
```

### Register Maker
```powershell
$body = @{
    username = "maker1"
    email = "maker1@company.com"
    password = "Password123!"
    role = "Maker"
    insuranceCompanyId = "550e8400-e29b-41d4-a716-446655440000"
} | ConvertTo-Json


### Login & Get Token
```powershell
$body = @{
    username = "maker1"
    password = "Password123!"
} | ConvertTo-Json

```

### Ingest Claim
```powershell
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$body = @{
    patientName = "John Doe"
    patientId = "PAT-001"
    serviceDate = "2026-04-15"
    claimAmount = 1500
    serviceDescription = "Emergency Room"
    providerName = "City Hospital"
    providerCode = "PROV-001"
} | ConvertTo-Json

```

---

## TROUBLESHOOTING QUICK REFERENCE

| Problem | Solution |
|---------|----------|
| Can't connect to PostgreSQL | Check service running: `pg_isready -h localhost` |
| Port 5001 already in use | `netstat -ano \| findstr :5001` then kill process |
| Migration fails | Delete database: `psql -h localhost -U postgres -c "DROP DATABASE \"InsureZenDBv3\";"` and recreate |
| Can't call API endpoints | Verify JWT token not expired, token included in Authorization header |
| Claim appears locked | Only Maker/Checker who locked it can unlock or submit review |
| Can't register user | Username might already exist, check duplicate error |
| Invalid password errors | Password must have uppercase, lowercase, digit, special character |

---

## WHAT EACH FILE DOES

| File | Purpose |
|------|---------|
| `Program.cs` | Startup: database, auth, services, middleware setup |
| `src/Controllers/AuthController.cs` | Login/register endpoints |
| `src/Controllers/ClaimsController.cs` | All 10 claim endpoints |
| `src/Services/AuthenticationService.cs` | Register/login logic |
| `src/Services/ClaimService.cs` | Claim workflows, locking, reviews |
| `src/Data/ApplicationDbContext.cs` | Database context, table mappings |
| `src/Models/*.cs` | Database entity definitions |
| `src/DTOs/*.cs` | API request/response formats |
| `src/Repositories/*.cs` | Database queries |
| `appsettings.Development.json` | Dev database connection, JWT secret |
| `Migrations/` | Database schema versions |

---

## WHAT TO TELL SOMEONE ELSE TO DO

Give them these 4 commands to copy-paste:

```powershell
cd d:\InsureZenv2
dotnet restore
dotnet ef database update
dotnet run
```

That's it. They now have a working backend API.

---

## KEY DESIGN DECISIONS

1. **Pessimistic Locking** - When Maker locks a claim, ONLY they can review it (via `LockedByUserId` database field)
2. **JWT Authentication** - No session state, scales to thousands of concurrent users
3. **Automatic Migration** - Database tables created automatically on app startup
4. **Swagger UI** - Try all 12 endpoints without writing any code
5. **Separate Reviews** - Maker and Checker reviews stored separately, both visible in final claim
6. **Audit Trail** - Every action logged for compliance and debugging

---

## NEXT STEPS AFTER SETUP

1. **View API docs**: Open `https://localhost:5000/swagger`
2. **Test a workflow**: Follow "STEP-BY-STEP WORKFLOW" section above
3. **View database**: Use `psql` to query tables
4. **Check logs**: Look in `logs/` folder
5. **Read full README**: For deeper technical details and edge cases

---

**You're all set!** 🎉

For detailed setup, see `README.md`.  
For API documentation, see `API_DOCUMENTATION.md`.
