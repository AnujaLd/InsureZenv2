# InsureZen - API Documentation

## Base URL
```
https://localhost:5001/api
```

## Authentication

All endpoints except `/auth/register` and `/auth/login` require JWT token in header:
```
Authorization: Bearer {jwt_token}
```

---

## Authentication Endpoints

### 1. POST /auth/register

Register a new user account.

**Request:**
```json
{
  "username": "maker1",
  "email": "maker1@company.com",
  "password": "Password123!",
  "role": "Maker",
  "insuranceCompanyId": "123e4567-e89b-12d3-a456-426614174000"
}
```

**Response (201):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "username": "maker1",
  "email": "maker1@company.com",
  "role": "Maker",
  "isActive": true
}
```

**Error (409):**
```json
{"message": "User already exists"}
```

---

### 2. POST /auth/login

Login and get JWT token.

**Request:**
```json
{
  "username": "maker1",
  "password": "Password123!"
}
```

**Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0...",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "username": "maker1",
    "email": "maker1@company.com",
    "role": "Maker"
  },
  "expiresIn": 3600
}
```

**Error (401):**
```json
{"message": "Invalid username or password"}
```

---

## Claim Management Endpoints

### 3. POST /claims/ingest

Submit a new insurance claim.

**Request:**
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

**Response (201):**
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

**Error (400):**
```json
{
  "errors": ["Patient name is required", "Claim amount must be greater than 0"]
}
```

---

### 4. GET /claims/{id}

Get claim details by ID.

**Response (200):**
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

**Error (404):**
```json
{"message": "Claim not found"}
```

---

### 5. GET /claims/maker/list

Get claims for maker review (Maker role only).

**Query Parameters:**
- `pageNumber` (default: 1)
- `pageSize` (default: 10, max: 100)
- `status` (optional): "Pending" or "MakerInProgress"

**Response (200):**
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
    }
  ],
  "totalCount": 5,
  "pageNumber": 1,
  "pageSize": 10
}
```

**Error (403):**
```json
{"message": "Access denied"}
```

---

### 6. GET /claims/checker/list

Get claims for checker review (Checker role only).

**Same query parameters as /claims/maker/list**

**Response (200):** Same as /claims/maker/list

**Error (403):**
```json
{"message": "Access denied"}
```

---

### 7. POST /claims/{id}/lock/maker

Lock claim for maker review.

**Response (200):**
```json
{"message": "Claim locked successfully"}
```

**Error (400):**
```json
{"message": "Cannot lock claim"}
```

---

### 8. POST /claims/{id}/lock/checker

Lock claim for checker review.

**Response (200):**
```json
{"message": "Claim locked successfully"}
```

---

### 9. POST /claims/{id}/unlock

Unlock claim.

**Response (200):**
```json
{"message": "Claim unlocked successfully"}
```

---

### 10. POST /claims/{id}/review/maker

Submit maker review and recommendation.

**Request:**
```json
{
  "feedback": "Documents verified",
  "recommendation": "Approve"
}
```

**Valid Recommendations:** "Approve" or "Reject"

**Response (200):**
```json
{
  "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "claimNumber": "CLM-20260421-ABC123",
  "status": "MakerSubmitted",
  "makerReview": {
    "id": "550e8400-e29b-41d4-a716-446655440003",
    "reviewedByUsername": "maker1",
    "feedback": "Documents verified",
    "makerRecommendation": "Approve",
    "reviewedAt": "2026-04-21T11:00:00Z"
  }
}
```

---

### 11. POST /claims/{id}/review/checker

Submit checker final decision.

**Request:**
```json
{
  "feedback": "Approve claim",
  "decision": "Approved"
}
```

**Valid Decisions:** "Approved" or "Rejected"

**Response (200):**
```json
{
  "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "claimNumber": "CLM-20260421-ABC123",
  "status": "Approved",
  "completedAt": "2026-04-21T14:45:00Z",
  "checkerReview": {
    "id": "550e8400-e29b-41d4-a716-446655440004",
    "reviewedByUsername": "checker1",
    "feedback": "Approve claim",
    "checkerDecision": "Approved",
    "reviewedAt": "2026-04-21T14:45:00Z"
  }
}
```

---

### 12. POST /claims/{id}/forward-to-insurer

Forward completed claim to insurance company.

**Response (200):**
```json
{"message": "Claim forwarded to insurer successfully"}
```

**Error (400):**
```json
{"message": "Claim must be in Approved or Rejected status"}
```

---

## HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 201 | Created |
| 400 | Bad Request |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 409 | Conflict |
| 500 | Server Error |

---

## Workflow Example

```
1. POST /auth/register         → Create user
2. POST /auth/login            → Get JWT token
3. POST /claims/ingest         → Submit claim
4. GET /claims/maker/list      → View claims
5. POST /claims/{id}/lock/maker    → Lock for review
6. POST /claims/{id}/review/maker  → Submit review
7. POST /claims/{id}/lock/checker  → Checker locks
8. POST /claims/{id}/review/checker → Checker decides
9. POST /claims/{id}/forward-to-insurer → Forward
```

---

**Last Updated:** April 22, 2026
