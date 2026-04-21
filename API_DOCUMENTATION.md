# API Documentation Quick Reference

## Base URL
```
https://localhost:5001/api
```

## Common Headers
```
Content-Type: application/json
Authorization: Bearer <jwt_token>
```

---

## Authentication Endpoints

### 1. POST /auth/register

Register a new user

**Request:**
```json
{
  "username": "newuser",
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "role": "Maker",
  "insuranceCompanyId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response (201):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "username": "newuser",
  "email": "user@example.com",
  "role": "Maker",
  "isActive": true
}
```

**Error (409 Conflict):**
```json
{
  "message": "User already exists"
}
```

---

### 2. POST /auth/login

Authenticate user and get JWT token

**Request:**
```json
{
  "username": "newuser",
  "password": "SecurePassword123!"
}
```

**Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "username": "newuser",
    "email": "user@example.com",
    "role": "Maker",
    "isActive": true
  },
  "expiresIn": 3600
}
```

**Error (401):**
```json
{
  "message": "Invalid username or password"
}
```

---

## Claim Management Endpoints

### 3. POST /claims/ingest

Ingest a new claim (requires authentication)

**Authorization:** Required

**Request:**
```json
{
  "patientName": "John Doe",
  "patientId": "PAT-123456",
  "serviceDate": "2026-04-15T00:00:00Z",
  "claimAmount": 2500.50,
  "serviceDescription": "Surgical procedure",
  "providerName": "Medical Center ABC",
  "providerCode": "MED-001"
}
```

**Response (201):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "claimNumber": "CLM-20260421-A1B2C3D4",
  "patientName": "John Doe",
  "patientId": "PAT-123456",
  "serviceDate": "2026-04-15T00:00:00Z",
  "claimAmount": 2500.50,
  "serviceDescription": "Surgical procedure",
  "providerName": "Medical Center ABC",
  "providerCode": "MED-001",
  "status": "Pending",
  "submittedAt": "2026-04-21T10:30:00Z",
  "completedAt": null
}
```

**Error (400):**
```json
{
  "errors": [
    "Patient name is required",
    "Claim amount must be greater than 0"
  ]
}
```

---

### 4. GET /claims/{id}

Get claim details (requires authentication)

**Authorization:** Required

**Response (200):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "claimNumber": "CLM-20260421-A1B2C3D4",
  "patientName": "John Doe",
  "patientId": "PAT-123456",
  "serviceDate": "2026-04-15T00:00:00Z",
  "claimAmount": 2500.50,
  "serviceDescription": "Surgical procedure",
  "providerName": "Medical Center ABC",
  "providerCode": "MED-001",
  "status": "Approved",
  "submittedAt": "2026-04-21T10:30:00Z",
  "completedAt": "2026-04-21T14:45:00Z",
  "makerReview": {
    "id": "550e8400-e29b-41d4-a716-446655440003",
    "reviewedByUsername": "maker1",
    "feedback": "All documents verified",
    "makerRecommendation": "Approve",
    "checkerDecision": null,
    "reviewedAt": "2026-04-21T11:00:00Z"
  },
  "checkerReview": {
    "id": "550e8400-e29b-41d4-a716-446655440004",
    "reviewedByUsername": "checker1",
    "feedback": "Concur with maker",
    "makerRecommendation": null,
    "checkerDecision": "Approved",
    "reviewedAt": "2026-04-21T14:45:00Z"
  }
}
```

**Error (404):**
```json
{
  "message": "Claim not found"
}
```

---

### 5. GET /claims/maker/list

Get claims available for maker review (Maker only)

**Authorization:** Required (Maker role)

**Query Parameters:**
- `pageNumber` (int, default: 1)
- `pageSize` (int, default: 10, max: 100)
- `status` (string, optional): "Pending" or "MakerInProgress"
- `insuranceCompanyId` (guid, optional)
- `fromDate` (datetime, optional): "2026-04-01T00:00:00Z"
- `toDate` (datetime, optional): "2026-04-30T23:59:59Z"

**Example Request:**
```
GET /claims/maker/list?pageNumber=1&pageSize=10&status=Pending
```

**Response (200):**
```json
{
  "claims": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440002",
      "claimNumber": "CLM-20260421-A1B2C3D4",
      "patientName": "John Doe",
      "patientId": "PAT-123456",
      "serviceDate": "2026-04-15T00:00:00Z",
      "claimAmount": 2500.50,
      "serviceDescription": "Surgical procedure",
      "providerName": "Medical Center ABC",
      "providerCode": "MED-001",
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

**Error (403):**
```json
{
  "message": "Access denied"
}
```

---

### 6. GET /claims/checker/list

Get claims available for checker review (Checker only)

**Authorization:** Required (Checker role)

**Query Parameters:** Same as `/claims/maker/list`

**Response (200):** Same structure as `/claims/maker/list`

---

### 7. POST /claims/{id}/lock/maker

Lock claim for maker review

**Authorization:** Required (Maker role)

**Response (200):**
```json
{
  "message": "Claim locked successfully"
}
```

**Error (400):**
```json
{
  "message": "Cannot lock claim. It may be locked by another user or not in the correct status."
}
```

---

### 8. POST /claims/{id}/lock/checker

Lock claim for checker review

**Authorization:** Required (Checker role)

**Response (200):**
```json
{
  "message": "Claim locked successfully"
}
```

---

### 9. POST /claims/{id}/unlock

Unlock claim

**Authorization:** Required

**Response (200):**
```json
{
  "message": "Claim unlocked successfully"
}
```

**Error (400):**
```json
{
  "message": "Cannot unlock claim. You don't have permission."
}
```

---

### 10. POST /claims/{id}/review/maker

Submit maker review and recommendation

**Authorization:** Required (Maker role)

**Request:**
```json
{
  "feedback": "All documentation verified and complete. Claim is legitimate.",
  "recommendation": "Approve"
}
```

**Valid Recommendations:** "Approve" or "Reject"

**Response (200):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "claimNumber": "CLM-20260421-A1B2C3D4",
  "patientName": "John Doe",
  "patientId": "PAT-123456",
  "serviceDate": "2026-04-15T00:00:00Z",
  "claimAmount": 2500.50,
  "serviceDescription": "Surgical procedure",
  "providerName": "Medical Center ABC",
  "providerCode": "MED-001",
  "status": "MakerSubmitted",
  "submittedAt": "2026-04-21T10:30:00Z",
  "completedAt": null,
  "makerReview": {
    "id": "550e8400-e29b-41d4-a716-446655440003",
    "reviewedByUsername": "maker1",
    "feedback": "All documentation verified and complete. Claim is legitimate.",
    "makerRecommendation": "Approve",
    "checkerDecision": null,
    "reviewedAt": "2026-04-21T11:15:00Z"
  },
  "checkerReview": null
}
```

**Error (400):**
```json
{
  "message": "Cannot submit review. You don't have permission or claim is not locked by you."
}
```

---

### 11. POST /claims/{id}/review/checker

Submit checker final decision

**Authorization:** Required (Checker role)

**Request:**
```json
{
  "feedback": "Reviewed maker assessment. Concur with recommendation. Approve.",
  "decision": "Approved"
}
```

**Valid Decisions:** "Approved" or "Rejected"

**Response (200):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "claimNumber": "CLM-20260421-A1B2C3D4",
  "patientName": "John Doe",
  "patientId": "PAT-123456",
  "serviceDate": "2026-04-15T00:00:00Z",
  "claimAmount": 2500.50,
  "serviceDescription": "Surgical procedure",
  "providerName": "Medical Center ABC",
  "providerCode": "MED-001",
  "status": "Approved",
  "submittedAt": "2026-04-21T10:30:00Z",
  "completedAt": "2026-04-21T14:45:00Z",
  "makerReview": {
    "id": "550e8400-e29b-41d4-a716-446655440003",
    "reviewedByUsername": "maker1",
    "feedback": "All documentation verified and complete. Claim is legitimate.",
    "makerRecommendation": "Approve",
    "checkerDecision": null,
    "reviewedAt": "2026-04-21T11:15:00Z"
  },
  "checkerReview": {
    "id": "550e8400-e29b-41d4-a716-446655440004",
    "reviewedByUsername": "checker1",
    "feedback": "Reviewed maker assessment. Concur with recommendation. Approve.",
    "makerRecommendation": null,
    "checkerDecision": "Approved",
    "reviewedAt": "2026-04-21T14:45:00Z"
  }
}
```

---

### 12. POST /claims/{id}/forward-to-insurer

Forward completed claim to insurance company

**Authorization:** Required

**Response (200):**
```json
{
  "message": "Claim forwarded to insurer successfully"
}
```

**Error (400):**
```json
{
  "message": "Claim cannot be forwarded. It must be in Approved or Rejected status."
}
```

---

## HTTP Status Codes

| Status | Meaning | Example |
|--------|---------|---------|
| 200 | OK | Login successful, data retrieved |
| 201 | Created | Resource created (user, claim) |
| 400 | Bad Request | Validation error, invalid state |
| 401 | Unauthorized | Missing/invalid token, login failed |
| 403 | Forbidden | Insufficient permissions for role |
| 404 | Not Found | Claim/user not found |
| 409 | Conflict | Duplicate username/email |
| 500 | Server Error | Unexpected error |

---

## Authentication Flow Example

```powershell
# 1. Register
POST https://localhost:5001/api/auth/register
{
  "username": "maker1",
  "email": "maker@example.com",
  "password": "SecurePassword123!",
  "role": "Maker",
  "insuranceCompanyId": "550e8400-e29b-41d4-a716-446655440000"
}
→ Returns user details

# 2. Login
POST https://localhost:5001/api/auth/login
{
  "username": "maker1",
  "password": "SecurePassword123!"
}
→ Returns token: "eyJhbGc..."

# 3. Use token for claims
GET https://localhost:5001/api/claims/maker/list
Authorization: Bearer eyJhbGc...
→ Returns available claims
```

---

## Full Claim Workflow Example

```powershell
# 1. Ingest claim
POST /claims/ingest
{ claim data }
→ claim_id: ABC-123, status: Pending

# 2. Maker locks claim
POST /claims/ABC-123/lock/maker
→ status: MakerInProgress

# 3. Maker views details
GET /claims/ABC-123
→ claim details

# 4. Maker submits review
POST /claims/ABC-123/review/maker
{
  "feedback": "Verified",
  "recommendation": "Approve"
}
→ status: MakerSubmitted

# 5. Checker locks claim
POST /claims/ABC-123/lock/checker
→ status: CheckerInProgress

# 6. Checker views claim + maker review
GET /claims/ABC-123
→ includes makerReview object

# 7. Checker submits decision
POST /claims/ABC-123/review/checker
{
  "feedback": "Concur",
  "decision": "Approved"
}
→ status: Approved, completedAt: NOW

# 8. Forward to insurer
POST /claims/ABC-123/forward-to-insurer
→ status: ForwardedToInsurer
```

---

## Common Error Responses

### Validation Error
```json
{
  "errors": [
    "Patient name is required",
    "Claim amount must be greater than 0",
    "Service date cannot be in the future"
  ]
}
```

### Authentication Error
```json
{
  "message": "Invalid username or password"
}
```

### Authorization Error
```json
{
  "message": "Access denied. User does not have permission for this resource."
}
```

### State Error
```json
{
  "message": "Cannot lock claim. It may be locked by another user or not in the correct status."
}
```

### Concurrency Error
```json
{
  "message": "Claim is currently being reviewed by another user."
}
```

---

## Token Claims Example

Decoded JWT token:
```json
{
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "550e8400-e29b-41d4-a716-446655440001",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "maker1",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": "maker1@insurezen.com",
  "Role": "Maker",
  "InsuranceCompanyId": "550e8400-e29b-41d4-a716-446655440000",
  "iat": 1713700800,
  "exp": 1713704400,
  "iss": "InsureZen",
  "aud": "InsureZenAPI"
}
```

---

## Rate Limiting (Future Implementation)

Currently no rate limiting. When implemented:
```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1713704400
```

---

## Versioning (Future)

All endpoints currently v1 (implicit). When versioning added:
```
https://localhost:5001/api/v1/auth/login
https://localhost:5001/api/v2/claims/list
```

---

**Last Updated:** April 21, 2026  
**API Version:** 1.0
