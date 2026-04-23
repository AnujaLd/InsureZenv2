# InsureZen Backend API

A REST API for managing medical insurance claims with a two-stage review process. Built with ASP.NET Core 10 and PostgreSQL.

---

## What This Does

InsureZen processes insurance claims through a simple workflow:

1. A claim is submitted into the system
2. A **Maker** reviews the claim and recommends approve or reject
3. A **Checker** reviews the Maker's recommendation and makes the final call
4. The completed claim is forwarded to the insurance company

---

## Before You Start

You need two things installed on your machine:

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) (version 13 or newer)

Check they're working:
```
dotnet --version
psql --version
```

---

## Setup (Follow These Steps in Order)

### Step 1 — Create the database

Open a terminal and run:
```
psql -h localhost -U postgres -c "CREATE DATABASE \"InsureZenDBv3\" ENCODING 'UTF8';"
```

If your PostgreSQL password isn't `postgres`, you'll be prompted to enter it.

### Step 2 — Check the connection string

Open `appsettings.Development.json` and make sure the password matches yours:
```json
"DefaultConnection": "Host=localhost;Port=5432;Database=InsureZenDBv3;Username=postgres;Password=yourpassword"
```

Change `Password=postgres` to whatever password you set during PostgreSQL installation.

### Step 3 — Install packages

```
dotnet restore
```

### Step 4 — Set up the database tables

```
dotnet ef database update
```

This creates all five tables automatically. You should see "Done." when it finishes.

### Step 5 — Run the app

```
dotnet run
```

You should see:
```
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to exit
```

### Step 6 — Open the API explorer

Go to this URL in your browser:
```
http://localhost:5000/swagger/index.html
```

You'll see all 12 endpoints listed and ready to test.

---

## API Endpoints

### Authentication

| Method | Endpoint | What it does |
|--------|----------|--------------|
| POST | /auth/register | Create a new user account |
| POST | /auth/login | Log in and get a token |

After logging in, copy the token from the response. You'll need to include it in all other requests as:
```
Authorization: Bearer YOUR_TOKEN_HERE
```

### Claims

| Method | Endpoint | What it does |
|--------|----------|--------------|
| POST | /claims/ingest | Submit a new claim |
| GET | /claims/{id} | Get details of one claim |
| GET | /claims/maker/list | List claims waiting for Maker review |
| GET | /claims/checker/list | List claims waiting for Checker review |
| POST | /claims/{id}/lock/maker | Maker takes ownership of a claim |
| POST | /claims/{id}/lock/checker | Checker takes ownership of a claim |
| POST | /claims/{id}/unlock | Release a claim |
| POST | /claims/{id}/review/maker | Maker submits feedback and recommendation |
| POST | /claims/{id}/review/checker | Checker submits final decision |
| POST | /claims/{id}/forward-to-insurer | Send completed claim to insurer |

---

## How a Claim Moves Through the System

```
Submitted → Pending
Maker locks it → MakerInProgress
Maker submits review → MakerSubmitted
Checker locks it → CheckerInProgress
Checker decides → Approved  (or Rejected)
Forwarded → ForwardedToInsurer
```

A claim can only move forward. You cannot skip steps or go backwards.

---

## Testing a Full Workflow

Here's how to test end-to-end using Swagger or PowerShell:

**1. Add an insurance company to the database (one-time setup):**
```sql
psql -h localhost -U postgres -d InsureZenDBv3

INSERT INTO "InsuranceCompanies" ("Id", "Name", "Code", "ContactEmail", "ContactPhone", "IsActive", "CreatedAt")
VALUES ('550e8400-e29b-41d4-a716-446655440000'::uuid, 'Test Insurance', 'TEST-001', 'test@example.com', '555-0000', true, NOW());
```

**2. Register a Maker and a Checker:**
```json
POST /auth/register
{
  "username": "maker1",
  "email": "maker1@example.com",
  "password": "Password123!",
  "role": "Maker",
  "insuranceCompanyId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**3. Log in as Maker, get token, submit a claim:**
```json
POST /claims/ingest
{
  "patientName": "Jane Smith",
  "patientId": "PAT-001",
  "serviceDate": "2026-04-01",
  "claimAmount": 1200.00,
  "serviceDescription": "Emergency consultation",
  "providerName": "City Hospital",
  "providerCode": "HOSP-01"
}
```

**4. Lock the claim, submit a review:**
```json
POST /claims/{id}/lock/maker

POST /claims/{id}/review/maker
{
  "feedback": "All documents look good",
  "recommendation": "Approve"
}
```

**5. Log in as Checker, lock and decide:**
```json
POST /claims/{id}/lock/checker

POST /claims/{id}/review/checker
{
  "feedback": "Agree with Maker",
  "decision": "Approved"
}
```

**6. Forward to insurer:**
```json
POST /claims/{id}/forward-to-insurer
```

---

## Password Requirements

When registering users, passwords must have:
- At least 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character (e.g. `!@#$%`)

Example of a valid password: `Password123!`

---

## Common Problems

**"Connection refused" when connecting to PostgreSQL**
Make sure PostgreSQL is running. On Windows, open Services and check "postgresql". On Mac, run `brew services start postgresql@17`.

**"Port 5000 is already in use"**
Something else is using that port. Find it with `netstat -ano | findstr :5000` and stop that process, or change the port in `Properties/launchSettings.json`.

**Migrations fail**
Make sure the database was created first (Step 1). If you need to reset: drop and recreate the database, then run `dotnet ef database update` again.

**"Access denied" on an endpoint**
Check that you're using the right role. Makers can only use `/claims/maker/*` endpoints. Checkers can only use `/claims/checker/*` endpoints.

**JWT token errors**
Tokens expire after 1 hour in production (24 hours in development). Log in again to get a fresh token.

---

## Project Layout

```
InsureZenv2/
├── src/
│   ├── Controllers/      ← The 12 API endpoints live here
│   ├── Services/         ← Business logic (workflows, rules)
│   ├── Repositories/     ← Database queries
│   ├── Models/           ← Database table definitions
│   ├── DTOs/             ← What the API accepts and returns
│   ├── Validators/       ← Input validation rules
│   └── Authentication/   ← JWT tokens and password hashing
├── Migrations/           ← Database version history
├── Program.cs            ← App startup and configuration
├── appsettings.json      ← Production config
└── appsettings.Development.json  ← Dev config (edit this one)
```

---

## Database Tables

| Table | Purpose |
|-------|---------|
| InsuranceCompanies | The partner companies using the system |
| Users | Maker and Checker employees |
| Claims | The insurance claims |
| ClaimReviews | Maker and Checker reviews for each claim |
| AuditLogs | Record of every action taken |

---

## Stopping the App

Press `Ctrl+C` in the terminal where it's running.
