# REQUIREMENTS ANALYSIS - InsureZen Backend

## Executive Summary

InsureZen is building a medical insurance claim management system with a two-stage human review workflow. This document provides a comprehensive analysis of functional and non-functional requirements, entities, workflows, edge cases, and architectural assumptions.

---

## 1. Domain Entities & Data Points

### 1.1 InsuranceCompany
Represents insurance companies using InsureZen services.

**Data Points:**
- `Id` (UUID) - Unique identifier
- `Name` (String) - Company name
- `Code` (String, UNIQUE) - Company code for reference
- `ContactEmail` (String) - Contact email address
- `ContactPhone` (String) - Contact phone number
- `IsActive` (Boolean) - Operational status
- `CreatedAt` (DateTime) - Creation timestamp
- `UpdatedAt` (DateTime) - Last update timestamp

**Relationships:**
- One-to-Many with Users
- One-to-Many with Claims

---

### 1.2 User (Employee)
Represents InsureZen employees who review and decide on claims.

**Data Points:**
- `Id` (UUID) - Unique identifier
- `Username` (String, UNIQUE) - Login username
- `Email` (String, UNIQUE) - Email address
- `PasswordHash` (String) - Securely hashed password (PBKDF2)
- `Role` (Enum: "Maker", "Checker") - User role
- `InsuranceCompanyId` (UUID, FK) - Associated insurance company
- `IsActive` (Boolean) - Account status
- `CreatedAt` (DateTime) - Account creation time
- `UpdatedAt` (DateTime) - Last update timestamp

**Relationships:**
- Many-to-One with InsuranceCompany
- One-to-Many with ClaimReviews (as reviewer)

**Notes:**
- Makers review claims and make initial recommendations
- Checkers review Maker recommendations and issue final decisions
- Both roles belong to same insurance company for data isolation

---

### 1.3 Claim
Represents an insurance claim submitted for processing.

**Data Points:**
- `Id` (UUID) - Unique identifier
- `ClaimNumber` (String, UNIQUE) - Human-readable claim identifier
- `InsuranceCompanyId` (UUID, FK) - Associated insurance company
- `PatientName` (String) - Patient name
- `PatientId` (String) - Patient identifier
- `ServiceDate` (DateTime) - Date of medical service
- `ClaimAmount` (Decimal) - Amount claimed (2 decimal places)
- `ServiceDescription` (String) - Description of service provided
- `ProviderName` (String) - Healthcare provider name
- `ProviderCode` (String) - Healthcare provider identifier
- `Status` (Enum) - Current workflow state
- `SubmittedAt` (DateTime) - Claim submission time
- `CompletedAt` (DateTime, nullable) - Completion time if finished
- `LockedByUserId` (UUID, nullable, FK) - User currently reviewing
- `LockedAt` (DateTime, nullable) - When lock was acquired
- `CreatedAt` (DateTime) - Record creation time
- `UpdatedAt` (DateTime) - Last update time

**Status Values:**
```
0 = Pending (Initial state)
1 = MakerInProgress (Maker is reviewing)
2 = MakerSubmitted (Maker completed, awaiting Checker)
3 = CheckerInProgress (Checker is reviewing)
4 = Approved (Checker approved)
5 = Rejected (Checker rejected)
6 = ForwardedToInsurer (Sent to insurance company)
```

**Relationships:**
- Many-to-One with InsuranceCompany
- One-to-One with ClaimReview (Maker review)
- One-to-One with ClaimReview (Checker review)
- Many-to-One with User (for lock)

**Notes:**
- Extracted data assumed to come from upstream OCR service
- Concurrency control via `LockedByUserId` field
- Claim can only be locked by one user at a time

---

### 1.4 ClaimReview
Represents a review (by Maker or Checker) of a claim.

**Data Points:**
- `Id` (UUID) - Unique identifier
- `ClaimId` (UUID, FK) - Associated claim
- `ReviewedByUserId` (UUID, FK) - User who conducted review
- `IsMakerReview` (Boolean) - True for Maker, False for Checker
- `Feedback` (String) - Review comments/feedback
- `MakerRecommendation` (Enum, nullable) - Maker's recommendation
  - 0 = Pending
  - 1 = Approve
  - 2 = Reject
- `CheckerDecision` (Enum, nullable) - Checker's final decision
  - 0 = Pending
  - 1 = Approved
  - 2 = Rejected
- `ReviewedAt` (DateTime) - When review was submitted
- `CreatedAt` (DateTime) - Record creation time
- `UpdatedAt` (DateTime) - Last update time

**Relationships:**
- Many-to-One with Claim
- Many-to-One with User (reviewer)

**Constraints:**
- At most one Maker review per claim
- At most one Checker review per claim
- Recommendations/decisions cannot be null when review is submitted

---

### 1.5 AuditLog
Complete audit trail of all actions on claims.

**Data Points:**
- `Id` (UUID) - Unique identifier
- `ClaimId` (UUID, FK) - Associated claim
- `UserId` (UUID, nullable, FK) - User performing action
- `Action` (String) - Action type (e.g., "CLAIM_LOCKED_BY_MAKER")
- `Details` (String) - Additional context/details
- `CreatedAt` (DateTime) - When action occurred

**Relationships:**
- Many-to-One with Claim
- Many-to-One with User

**Actions Logged:**
- CLAIM_INGESTED
- CLAIM_LOCKED_BY_MAKER
- CLAIM_LOCKED_BY_CHECKER
- CLAIM_UNLOCKED
- MAKER_REVIEW_SUBMITTED
- CHECKER_DECISION_SUBMITTED
- CLAIM_FORWARDED_TO_INSURER

---

## 2. Actors & Roles

### 2.1 Maker
**Responsibilities:**
- Review extracted claim data for accuracy and completeness
- Add feedback/comments on claims
- Recommend approval or rejection
- Cannot see Checker reviews or decisions

**Permissions:**
- View claims in `Pending` and `MakerInProgress` status
- Lock claims for review
- Submit review and recommendation
- Unlock claims
- View claim history with pagination

**Constraints:**
- Cannot access Checker-only endpoints
- Cannot see Checker decisions until claim is in final state
- Can only have one claim locked at a time (implicit via claim structure)

### 2.2 Checker
**Responsibilities:**
- Review Maker's recommendation and feedback
- Re-examine claim data independently
- Issue final approval or rejection decision
- Determine claim forwarding

**Permissions:**
- View claims in `MakerSubmitted` and `CheckerInProgress` status
- Lock claims for review
- View Maker's recommendation and feedback
- Submit final decision
- Unlock claims
- Forward approved/rejected claims to insurer

**Constraints:**
- Cannot access Maker-only endpoints
- Cannot modify Maker feedback
- Can only review claims that have Maker review

### 2.3 System (Automated)
**Responsibilities:**
- Manage claim state transitions
- Enforce business rules
- Create audit logs
- Prevent concurrent access

**Actions:**
- Auto-unlock claims if session expires (future enhancement)
- Generate claim numbers
- Record all state transitions

---

## 3. Functional Requirements

### 3.1 Authentication & Authorization

**FR-AUTH-1: User Registration**
- Users can register with username, email, password, role, and insurance company
- Passwords must meet complexity requirements:
  - Minimum 8 characters
  - At least 1 uppercase letter
  - At least 1 lowercase letter
  - At least 1 digit
  - At least 1 special character (!@#$%^&*)
- Usernames and emails must be unique
- Users are created as active immediately

**FR-AUTH-2: User Login**
- Users can login with username and password
- Successful login returns JWT token with 1-hour validity (configurable)
- Invalid credentials return 401 Unauthorized
- Token contains claims: UserId, Username, Email, Role, InsuranceCompanyId

**FR-AUTH-3: Role-Based Access Control**
- `/claims/maker/*` endpoints only accessible with `Role=Maker`
- `/claims/checker/*` endpoints only accessible with `Role=Checker`
- Unauthorized access returns 403 Forbidden

### 3.2 Claim Ingestion

**FR-INGEST-1: Ingest New Claim**
- Accept structured claim data from frontend/upstream service
- Validate all required fields present and valid
- Generate unique claim number in format: `CLM-YYYYMMDD-XXXXXXXX`
- Set initial status to `Pending`
- Record creation timestamp
- Return claim details with ID and claim number

**FR-INGEST-2: Input Validation**
- Patient name: Required, max 255 characters
- Patient ID: Required, max 100 characters
- Service date: Required, cannot be in future
- Claim amount: Required, must be > 0, max 2 decimal places
- Service description: Required
- Provider name: Required, max 255 characters
- Provider code: Required, max 100 characters

### 3.3 Maker Workflow

**FR-MAKER-1: View Available Claims**
- Makers view paginated list of `Pending` and `MakerInProgress` claims
- Default page size: 10, max: 100
- Support filtering by:
  - Status (Pending, MakerInProgress)
  - Insurance company
  - Date range (fromDate, toDate)
- Return total count for pagination

**FR-MAKER-2: Lock Claim**
- Maker locks claim before review
- Claim status changes to `MakerInProgress`
- Lock includes `LockedByUserId` and `LockedAt` timestamp
- Only one user can lock claim at a time
- Subsequent lock attempts fail with 400 error

**FR-MAKER-3: View Claim Details**
- Maker views full claim details
- Cannot see existing Checker reviews

**FR-MAKER-4: Submit Review**
- Maker provides feedback and recommendation (Approve/Reject)
- Creates `ClaimReview` record with `IsMakerReview=true`
- Sets claim status to `MakerSubmitted`
- Unlocks claim (clears `LockedByUserId`)
- Records audit log entry

**FR-MAKER-5: Unlock Claim**
- Maker can unlock claim they locked
- Resets `LockedByUserId` and `LockedAt`
- Fails if claim not locked or locked by different user

### 3.4 Checker Workflow

**FR-CHECKER-1: View Available Claims**
- Checkers view paginated list of `MakerSubmitted` and `CheckerInProgress` claims
- Same pagination and filtering options as Maker
- Can see associated Maker review and feedback

**FR-CHECKER-2: Lock Claim**
- Checker locks claim before review
- Claim status changes to `CheckerInProgress`
- Same lock mechanism as Maker

**FR-CHECKER-3: View Claim & Maker Review**
- Checker views full claim details
- Sees Maker's feedback and recommendation
- Reviews independently

**FR-CHECKER-4: Submit Decision**
- Checker provides feedback and final decision (Approved/Rejected)
- Creates `ClaimReview` record with `IsMakerReview=false`
- Sets claim status to `Approved` or `Rejected`
- Sets `CompletedAt` timestamp
- Unlocks claim
- Records audit log entry

### 3.5 Claim Forwarding

**FR-FORWARD-1: Forward to Insurer**
- Only `Approved` or `Rejected` claims can be forwarded
- Status changes to `ForwardedToInsurer`
- Records audit log: "Claim forwarded to insurance company"
- Simulates delivery to insurance company (stub/log entry)

### 3.6 Claim History

**FR-HISTORY-1: Paginated Claim History**
- All users view complete claim history (all statuses)
- Pagination with configurable page size (max 100)
- Filter by:
  - Status (any of 7 statuses)
  - Insurance company
  - Date range
- Return claims with:
  - Basic claim info
  - Current status
  - Submission and completion times
  - Maker and Checker review summaries

**FR-HISTORY-2: Claim Details View**
- Get full details of any claim
- Shows all reviews and audit trail
- Required for analytics dashboard

---

## 4. Non-Functional Requirements

### 4.1 Concurrency & Data Integrity

**NFR-CONC-1: Pessimistic Locking**
- Claims locked via `LockedByUserId` field (optimistic lock pattern)
- Prevents simultaneous review by multiple users
- Automatic validation on claim state changes

**NFR-CONC-2: State Consistency**
- Strict state transition rules enforced
- Invalid transitions fail with appropriate error
- Atomic operations via transactions

**NFR-CONC-3: Concurrent Users**
- System handles 100s-1000s of concurrent claims
- Multiple Makers and Checkers working simultaneously
- No race conditions or data corruption

### 4.2 Performance

**NFR-PERF-1: Query Efficiency**
- Indexes on frequently filtered columns:
  - Claims.Status
  - Claims.LockedByUserId
  - Claims.InsuranceCompanyId
  - Claims.SubmittedAt
  - ClaimReviews.(ClaimId, IsMakerReview)
  - AuditLogs.(ClaimId, CreatedAt)

**NFR-PERF-2: Pagination**
- All list endpoints paginated
- Max 100 items per page
- Efficient database queries using Skip/Take

**NFR-PERF-3: Response Times**
- Target: < 200ms for most endpoints
- Claim ingestion: < 500ms
- List queries: < 1000ms with 100+ claims

### 4.3 Auditability & Compliance

**NFR-AUDIT-1: Complete Audit Trail**
- Every action logged with:
  - Action type
  - User performing action
  - Timestamp (UTC)
  - Claim reference
  - Additional details

**NFR-AUDIT-2: Immutable Logs**
- Audit logs never updated or deleted
- Append-only pattern

**NFR-AUDIT-3: Data Retention**
- All logs retained for minimum 7 years
- Supports regulatory compliance requirements

### 4.4 Security

**NFR-SEC-1: Authentication**
- JWT tokens with:
  - 1-hour expiry (configurable per environment)
  - Symmetric signing (HS256)
  - Standard claims (iss, aud, iat, exp)
  - Custom claims (Role, InsuranceCompanyId)

**NFR-SEC-2: Password Security**
- PBKDF2 hashing with:
  - SHA-256 algorithm
  - 10,000 iterations
  - 64-byte salt
  - Constant-time comparison for verification

**NFR-SEC-3: Data Isolation**
- Users only see claims from their insurance company
- Role-based access prevents unauthorized operations
- No direct SQL injection vectors (EF Core parameterized queries)

**NFR-SEC-4: HTTPS**
- All API communication over HTTPS
- Self-signed certs for development
- Real CA certificates for production

### 4.5 Reliability

**NFR-REL-1: Error Handling**
- Graceful error responses with:
  - Appropriate HTTP status codes
  - Informative error messages
  - Request correlation IDs (future)

**NFR-REL-2: Logging**
- Structured logging with Serilog
- Log levels: Debug, Information, Warning, Error, Fatal
- Logs to console and rolling file

**NFR-REL-3: Database Transactions**
- EF Core handles transactions implicitly
- SaveChangesAsync() atomic operations

---

## 5. Edge Cases & Constraints

### 5.1 Concurrency Edge Cases

**EC-CONC-1: Double Locking**
- **Scenario**: User A locks claim, User B attempts to lock same claim
- **Handling**: Lock fails with 400 "Claim locked by another user"
- **Prevention**: Check `LockedByUserId` before allowing lock

**EC-CONC-2: Stale Lock**
- **Scenario**: Claim locked for extended time without action
- **Handling**: Manual unlock via API (future: auto-unlock after timeout)
- **Prevention**: Include lock timestamp for monitoring

**EC-CONC-3: Duplicate Submission**
- **Scenario**: User submits review twice (client retry)
- **Handling**: Idempotent via claim status check
- **Prevention**: Only allow submission if not already submitted

### 5.2 Validation Edge Cases

**EC-VAL-1: Missing Required Fields**
- **Scenario**: Claim ingestion without patient name
- **Handling**: 400 Bad Request with field-specific error messages
- **Prevention**: FluentValidation rules

**EC-VAL-2: Invalid Email Format**
- **Scenario**: User registers with invalid email
- **Handling**: 400 Bad Request with email format error
- **Prevention**: EmailAddress validator

**EC-VAL-3: Duplicate Username**
- **Scenario**: Two users with same username
- **Handling**: 409 Conflict on registration
- **Prevention**: Database UNIQUE constraint + validator

**EC-VAL-4: Invalid State Transition**
- **Scenario**: Checker tries to review Pending claim (no Maker review)
- **Handling**: 400 Bad Request "Claim not in correct status"
- **Prevention**: Status check before allowing operation

### 5.3 Authorization Edge Cases

**EC-AUTH-1: Token Expiry**
- **Scenario**: Request with expired JWT token
- **Handling**: 401 Unauthorized "Token expired"
- **Prevention**: Token validation on every request

**EC-AUTH-2: Wrong Role**
- **Scenario**: Maker tries to access /claims/checker/list
- **Handling**: 403 Forbidden "Insufficient permissions"
- **Prevention**: Policy-based authorization

**EC-AUTH-3: Inactive User**
- **Scenario**: Inactive user attempts login
- **Handling**: 401 Unauthorized "Invalid username or password"
- **Prevention**: IsActive check during login

**EC-AUTH-4: Cross-Insurance-Company Access**
- **Scenario**: User from Insurance Co A views claims from Co B
- **Handling**: Filtered at repository level (future multi-tenancy)
- **Prevention**: InsuranceCompanyId validation

### 5.4 Data Integrity Edge Cases

**EC-DATA-1: Orphaned Reviews**
- **Scenario**: Delete claim with associated reviews
- **Handling**: CASCADE delete enforced at database
- **Prevention**: Foreign key constraints

**EC-DATA-2: Incomplete Reviews**
- **Scenario**: Claim locked but review never submitted
- **Handling**: Claim stays locked; manual unlock via API
- **Prevention**: Proper error handling and user guidance

**EC-DATA-3: Negative Claim Amount**
- **Scenario**: Claim ingested with negative amount
- **Handling**: 400 Bad Request "Amount must be > 0"
- **Prevention**: Numeric validation rules

### 5.5 Business Logic Edge Cases

**EC-BUS-1: Maker/Checker Same Person**
- **Scenario**: Same user acts as both Maker and Checker (if role changes)
- **Handling**: Only review claims in appropriate workflow stage
- **Prevention**: Cannot change role after user creation (future)

**EC-BUS-2: Forward Already Forwarded**
- **Scenario**: Attempt to forward same claim twice
- **Handling**: 400 Bad Request "Already forwarded"
- **Prevention**: Status check (only `Approved`/`Rejected` can forward)

**EC-BUS-3: Service Date in Future**
- **Scenario**: Claim with service date after submission
- **Handling**: 400 Bad Request "Service date cannot be in future"
- **Prevention**: Temporal validation

**EC-BUS-4: Maker Changes Recommendation**
- **Scenario**: Maker submits second review with different recommendation
- **Handling**: 400 Bad Request "Review already submitted"
- **Prevention**: Status transitions prevent resubmission

---

## 6. System Assumptions

### 6.1 Data Assumptions

1. **Upstream OCR Service** - Claims data is already extracted and structured by separate service
2. **UTC Timestamps** - All times stored and returned in UTC
3. **Claim Uniqueness** - Claim number guaranteed unique
4. **Patient Data** - Patient info is de-identified or encrypted upstream
5. **Single Insurance Company per User** - No cross-company users

### 6.2 Operational Assumptions

1. **No Email Verification** - Users immediately active after registration
2. **No Password Reset** - Admin-only functionality (future)
3. **No Soft Deletes** - Deleted records are permanently removed
4. **No Multi-Tenancy** - Single InsureZen instance (future: multi-tenant)
5. **No Claim Resubmission** - Rejected claims don't return to workflow

### 6.3 Performance Assumptions

1. **Database Connections** - Connection pooling with default pool size 100
2. **Claim Volume** - Design supports 1000s of claims/day
3. **User Concurrency** - 100+ concurrent users
4. **Pagination Limit** - Max 100 items per page is acceptable

### 6.4 Integration Assumptions

1. **No Real Insurer API** - Forwarding is logged, not transmitted
2. **No Document Storage** - No S3/blob storage for original forms
3. **No Email Notifications** - No email alerts on status changes
4. **No Analytics Database** - No separate warehouse for reporting (future)
5. **No Real-Time Updates** - No WebSocket/SignalR (future)

### 6.5 Security Assumptions

1. **HTTPS Required** - All traffic encrypted in flight
2. **No Public API** - Only internal InsureZen employees access API
3. **No Anonymous Access** - All endpoints require authentication
4. **Single Secret Key** - All instances share same JWT secret (managed externally)
5. **No API Rate Limiting** - Trusts internal users (future: implement for robustness)

---

## 7. Workflow State Diagrams

### 7.1 Maker Workflow

```
┌─────────┐
│ Pending │
└────┬────┘
     │ [Maker locks]
     ▼
┌──────────────────┐
│ MakerInProgress  │
└────┬────────┬────┘
     │        │
     │        └─ [Unlock] → Pending
     │
     │ [Submit review + recommendation]
     ▼
┌────────────────┐
│ MakerSubmitted │
└────────────────┘
```

### 7.2 Checker Workflow

```
┌────────────────┐
│ MakerSubmitted │
└────┬───────────┘
     │ [Checker locks]
     ▼
┌──────────────────┐
│ CheckerInProgress│
└────┬────────┬────┘
     │        │
     │        └─ [Unlock] → MakerSubmitted
     │
     │ [Submit decision]
     ▼
┌─────────────────────────┐
│ Approved / Rejected     │
└────┬────────────────────┘
     │ [Forward to insurer]
     ▼
┌────────────────────┐
│ ForwardedToInsurer │
└────────────────────┘
```

### 7.3 Complete Workflow

```
Pending
  ↓
MakerInProgress (User: Maker, Lock: Yes)
  ↓
MakerSubmitted (ClaimReview created, Recommendation: Approve/Reject)
  ↓
CheckerInProgress (User: Checker, Lock: Yes)
  ↓
Approved/Rejected (ClaimReview created, Decision: Approved/Rejected)
  ↓
ForwardedToInsurer (Audit log: CLAIM_FORWARDED_TO_INSURER)
```

---

## 8. API Contract Summary

### Endpoints by Actor

| Endpoint | Method | Role | Status Checks |
|----------|--------|------|------------------|
| /auth/login | POST | Any | - |
| /auth/register | POST | Any | - |
| /claims/ingest | POST | Any | - |
| /claims/{id} | GET | Any | - |
| /claims/maker/list | GET | Maker | Filters for Pending, MakerInProgress |
| /claims/checker/list | GET | Checker | Filters for MakerSubmitted, CheckerInProgress |
| /claims/{id}/lock/maker | POST | Maker | Must be Pending or MakerInProgress |
| /claims/{id}/lock/checker | POST | Checker | Must be MakerSubmitted or CheckerInProgress |
| /claims/{id}/unlock | POST | Owner | Must be locked by user |
| /claims/{id}/review/maker | POST | Maker | Must be MakerInProgress and locked |
| /claims/{id}/review/checker | POST | Checker | Must be CheckerInProgress and locked |
| /claims/{id}/forward-to-insurer | POST | Any | Must be Approved or Rejected |

---

## 9. Testing Strategy

### Unit Tests
- Password hashing and verification
- JWT token generation and validation
- Validation rules (validators)
- Mapper configurations

### Integration Tests
- Database operations (CRUD)
- State transitions (Pending → MakerInProgress → etc.)
- Concurrency locks
- Authorization rules

### End-to-End Tests
- Complete Maker workflow
- Complete Checker workflow
- Concurrent user access
- Error handling

---

## 10. Future Enhancements

1. **Multi-Tenancy** - True tenant isolation at database/application level
2. **Audit Dashboard** - Admin view of all audit logs
3. **Notifications** - Email alerts on status changes
4. **Real Insurer Integration** - HTTP calls to insurance company APIs
5. **Role Management** - Admin role for user management
6. **Password Reset** - Self-service password reset
7. **Two-Factor Authentication** - Additional security layer
8. **Analytics** - Claims processing metrics and dashboards
9. **Document Storage** - Store original claim forms/images
10. **Real-Time Updates** - WebSocket notifications for status changes
11. **Rate Limiting** - API throttling per user/IP
12. **API Key Authentication** - Service-to-service authentication
13. **Approval Workflows** - Multi-level approval chains
14. **Claim Appeals** - Rejected claim resubmission process
15. **Batch Operations** - Bulk claim processing

---

**Document Version:** 1.0  
**Last Updated:** April 21, 2026  
**Status:** Final
