# 📚 InsureZen - Complete Documentation Index

## Welcome! Start Here

---

### "Just make it run in 5 minutes"
→ Read: [SETUP_SUMMARY.md](SETUP_SUMMARY.md)  
→ Commands: Copy the 4-command quick start  
→ Result: API running on your machine in 5 minutes  

---
→ Read: [README.md](README.md) - Chapter 2 "Complete A-to-Z Setup Guide"  
→ Follow: All 11 numbered steps  
→ Result: Full understanding of what's happening, API running in 15 minutes  

---

→ Read: [API_DOCUMENTATION.md](API_DOCUMENTATION.md)  
→ Reference: All 12 endpoints with request/response examples  
→ Test: Use Swagger UI at `http://localhost:5000/swagger/index.html`  

---
→ Read: [README.md](README.md) - Chapter 4 "Project Structure"  
→ Read: [README.md](README.md) - Chapter 5 "Requirements Analysis"  
→ Understand: Entities, actors, workflows, and constraints  

---
→ Read: [DOCUMENTATION_COMPLETE.md](DOCUMENTATION_COMPLETE.md)  
→ Review: Checklist of all deliverables  
→ Confirm: Everything is included and working  

---

## 📄 Documentation Files Overview

**The comprehensive guide for everything**

Contents:
- What this project is and what it does
- 5-minute quick start
- Complete A-to-Z setup (11 detailed steps)
- Prerequisites and installation for all platforms
- Running the application
- All 12 API endpoints fully documented
- Complete step-by-step workflow (15 steps with commands)
- Project structure with architecture diagram
- Comprehensive requirements analysis
- Database schema documentation
- Authentication and authorization
- Testing guidance
- Troubleshooting for 12+ scenarios
- Production deployment notes

**Best for**: Complete reference guide for everything

---

### SETUP_SUMMARY.md (300+ lines) ⚡ QUICK START
**Fast path to getting running**

Contents:
- What the project is (30-second summary)
- Fastest setup (10 minutes)
- API endpoints summary (all 12 listed)
- Step-by-step workflow explanation
- Database tables overview
- Claim status flow diagram
- Roles and permissions
- Common testing commands
- Troubleshooting quick reference
- What each file does
- Key design decisions

**Best for**: Quick setup and refresher on how to test

---

### API_DOCUMENTATION.md (400+ lines) 📡 API REFERENCE
**Detailed API endpoint documentation**

Contents:
- Base URL and authentication
- All 12 endpoints with:
  - Purpose statement
  - HTTP method and URL
  - Headers required
  - Request body format with example
  - Response format with example
  - Error responses with examples
  - Validation rules
  - Query parameters
- HTTP status codes reference
- Complete workflow example in API calls
- Pagination details

**Best for**: Calling the API, understanding contracts, testing endpoints

---

### REQUIREMENTS.md (included in README) 📊 REQUIREMENTS
**Complete requirements breakdown**

Contents:
- Problem statement summary
- Domain entities (5 entities explained)
- Actors and roles (Maker, Checker, System)
- Functional requirements (14 listed with priority)
- Non-functional requirements (7 listed with targets)
- Claim state diagram
- Constraints and edge cases (9 documented)
- 10 documented assumptions

**Best for**: Understanding what the system does and why

---

### DOCUMENTATION_COMPLETE.md ✅ VERIFICATION
**Verification that all deliverables are complete**

Contents:
- Confirmation of all deliverables
- Overview of each documentation file
- What's included in each section
- Verification checklist
- How to use these documents
- What you can do after setup

**Best for**: Confirming everything is included and understanding the scope

---

## 🚀 Quick Command Reference

### Install Everything (Windows)
```powershell
winget install Microsoft.DotNet.SDK.10
winget install PostgreSQL.PostgreSQL
```

### Create Database
```powershell
psql -h localhost -U postgres -c "CREATE DATABASE \"InsureZenDBv3\" ENCODING 'UTF8';"
```

### Setup & Run (4 commands)
```powershell
cd d:\InsureZenv2
dotnet restore
dotnet ef database update
dotnet run
```

### Test the API
```
Open: http://localhost:5000/swagger/index.html
```

---

## 📋 All 12 API Endpoints

| # | Method | Endpoint | Purpose |
|---|--------|----------|---------|
| 1 | POST | `/auth/register` | Create new user (Maker/Checker) |
| 2 | POST | `/auth/login` | Login and get JWT token |
| 3 | POST | `/claims/ingest` | Submit new claim |
| 4 | GET | `/claims/{id}` | Get claim details |
| 5 | GET | `/claims/maker/list` | List claims for maker review |
| 6 | GET | `/claims/checker/list` | List claims for checker review |
| 7 | POST | `/claims/{id}/lock/maker` | Lock claim as maker |
| 8 | POST | `/claims/{id}/lock/checker` | Lock claim as checker |
| 9 | POST | `/claims/{id}/unlock` | Release claim lock |
| 10 | POST | `/claims/{id}/review/maker` | Submit maker review & recommendation |
| 11 | POST | `/claims/{id}/review/checker` | Submit checker final decision |
| 12 | POST | `/claims/{id}/forward-to-insurer` | Mark claim as forwarded |

---

## 🔄 The Workflow

```
1. Create insurance company (database)
2. Register maker user
3. Register checker user
4. Maker logs in
5. Ingest claim → Status: Pending
6. Maker locks claim → Status: MakerInProgress
7. Maker submits review → Status: MakerSubmitted
8. Checker locks claim → Status: CheckerInProgress
9. Checker submits decision → Status: Approved/Rejected
10. Forward to insurer → Status: ForwardedToInsurer
✅ Complete!
```

---

## 🏗️ Architecture

```
Browser/Client
    ↓
REST API (Controllers)
    ↓ (HTTP Requests)
    │
    ├→ Authentication (JWT validation)
    ├→ Authorization (Role-based access)
    ├→ Input Validation (FluentValidation)
    │
    ↓
Services (Business Logic)
    ├→ Claim workflows
    ├→ User authentication
    ├→ State transitions
    │
    ↓
Repositories (Data Access)
    ├→ ClaimRepository
    ├→ UserRepository
    ├→ ClaimReviewRepository
    ├→ AuditLogRepository
    │
    ↓
Entity Framework Core (ORM)
    ↓
PostgreSQL Database
    ├→ InsuranceCompanies
    ├→ Users
    ├→ Claims
    ├→ ClaimReviews
    ├→ AuditLogs
```

---

## 🗄️ Database Tables

1. **InsuranceCompanies** - Partner companies
2. **Users** - Maker/Checker employees
3. **Claims** - Insurance claims
4. **ClaimReviews** - Maker and Checker reviews
5. **AuditLogs** - Complete audit trail

---

## 👥 Roles & Permissions

### Maker Role
- Can see: Pending claims
- Can do: Lock, review, recommend (Approve/Reject)
- Endpoints: `/claims/maker/*`

### Checker Role
- Can see: MakerSubmitted claims
- Can do: Lock, review, decide (Approved/Rejected)
- Endpoints: `/claims/checker/*`

### Both Roles
- Can do: View claims, ingest claims, forward to insurer
- Endpoints: `/claims/ingest`, `/claims/{id}`, `/claims/{id}/forward-to-insurer`

---

## 📊 Key Features

✅ **12 REST API Endpoints** - Fully working  
✅ **JWT Authentication** - Secure token-based auth  
✅ **Role-Based Authorization** - Maker vs Checker  
✅ **Pessimistic Locking** - Prevent concurrent edits  
✅ **Two-Stage Workflow** - Maker review → Checker decision  
✅ **Input Validation** - FluentValidation with meaningful errors  
✅ **Database Migrations** - Automatic schema creation  
✅ **Audit Logging** - Complete action trail  
✅ **Swagger UI** - Interactive API documentation  

---

## 🚦 Getting Started Steps

### Step 1: Choose Your Path
- 😃 5-minute setup? → Go to [SETUP_SUMMARY.md](SETUP_SUMMARY.md)
- 📖 Detailed setup? → Go to [README.md](README.md)
- 📡 API details? → Go to [API_DOCUMENTATION.md](API_DOCUMENTATION.md)

### Step 2: Install Prerequisites
- .NET SDK 10.0
- PostgreSQL 17

### Step 3: Run Commands
```powershell
cd d:\InsureZenv2
dotnet restore
dotnet ef database update
dotnet run
```

### Step 4: Test API
```
http://localhost:5000/swagger/index.html
```

### Step 5: Follow Workflow
Use the step-by-step workflow in [SETUP_SUMMARY.md](SETUP_SUMMARY.md)

---

## 📖 Reading Order Recommendations

### For Setup (New to Project)
1. This file (INDEX.md) - Get oriented
2. [SETUP_SUMMARY.md](SETUP_SUMMARY.md) - Quick overview
3. [README.md](README.md) - Chapter 2 "Complete A-to-Z Setup"
4. [README.md](README.md) - Chapter 3 "Running the Application"
5. [API_DOCUMENTATION.md](API_DOCUMENTATION.md) - Test endpoints

### For Integration (Need to Call API)
1. [API_DOCUMENTATION.md](API_DOCUMENTATION.md) - Endpoint reference
2. [README.md](README.md) - Chapter 6 "Complete API Workflow Example"
3. [SETUP_SUMMARY.md](SETUP_SUMMARY.md) - Common testing commands
4. [README.md](README.md) - Chapter 9 "Troubleshooting"

### For Understanding (Want to Learn)
1. [README.md](README.md) - Chapter 1 "What This Project Is"
2. [README.md](README.md) - Chapter 5 "Requirements Analysis"
3. [README.md](README.md) - Chapter 4 "Project Structure"
4. [API_DOCUMENTATION.md](API_DOCUMENTATION.md) - See endpoints in action

### For Deployment (Production Use)
1. [README.md](README.md) - Chapter 10 "Authentication & Authorization"
2. [README.md](README.md) - Chapter 11 "Database Schema"
3. [README.md](README.md) - Chapter 13 "Production Deployment"
4. [README.md](README.md) - Chapter 9 "Troubleshooting"

---


**Start with [SETUP_SUMMARY.md](SETUP_SUMMARY.md) now! →**

---
Status: ✅ Complete and Ready to Use
