# Assignment Report — InsureZen Backend API
### Tech Unicorn .NET Backend Engineer Assessment

---

## Overview

This report explains the decisions I made when building the InsureZen backend API — why I structured things the way I did, what trade-offs I considered, and where I'd do things differently in a real production system. It's meant to be read alongside the README and code, not as a standalone document.

---

## Task 1: Requirements Analysis

### How I Approached It

The problem statement is deliberately a bit open-ended, which I think is intentional — it's asking whether you can read a business description and translate it into concrete technical requirements. I started by identifying the core "things" in the system (entities), then the people using it (actors), then what each person needs to be able to do (functional requirements).

The most important insight from reading the problem was the **two-stage workflow**. It's not just "review a claim" — there are two distinct human roles with different responsibilities, and they interact with the same data in different ways. That pattern drives almost every design decision downstream.

### Key Entities I Identified

The five entities I settled on are: InsuranceCompany, User, Claim, ClaimReview, and AuditLog.

One decision worth explaining is **ClaimReview**. I could have put the Maker's feedback and the Checker's decision directly on the Claim table. Instead, I put them in a separate `ClaimReviews` table. The reason is that both reviewers produce similar data (feedback text, a verdict), and keeping them separate makes it easy to query "who reviewed what" and "what did each reviewer say" without jumbling everything together on one row.

The `AuditLog` table is something I added beyond the minimum requirements. The problem mentions "auditability" as a non-functional requirement, and in the insurance industry this is genuinely important — you need to be able to prove the chain of custody for every decision.

### The State Machine

The claim status is effectively a state machine with seven states. I think this is the most important piece of data modelling in the whole system. Every operation on a claim checks whether the claim is in the right state before allowing it to proceed. This means it's impossible (via the API at least) to get a claim into an inconsistent state.

The valid transitions are:
- Pending → MakerInProgress (when a Maker locks it)
- MakerInProgress → MakerSubmitted (when a Maker submits a review)
- MakerInProgress → Pending (when a Maker unlocks without reviewing)
- MakerSubmitted → CheckerInProgress (when a Checker locks it)
- CheckerInProgress → Approved or Rejected (when a Checker submits a decision)
- CheckerInProgress → MakerSubmitted (when a Checker unlocks without deciding)
- Approved or Rejected → ForwardedToInsurer

---

## Task 2: API Design

### How I Designed the Endpoints

I started from the two workflows (Maker and Checker) and worked out what actions each one needs to perform. Then I checked whether each action should be its own endpoint or could be combined with another.

The locking and unlocking endpoints (`/claims/{id}/lock/maker`, `/claims/{id}/lock/checker`, `/claims/{id}/unlock`) are worth explaining. I chose to make these explicit endpoints rather than having the lock happen automatically when you call the review endpoint. This gives the frontend more control — for example, showing a UI indicator that a claim is "being reviewed by someone" before the review is actually submitted.

### Why I Used POST for Lock/Unlock

You might expect PATCH for something that modifies a resource. I used POST because locking a claim is closer to a command/action than a partial update. It changes the state of the claim, but via a specific business action rather than a generic "update these fields" operation. This is consistent with the `/review/maker` and `/review/checker` endpoints which are also POST.

### Pagination

List endpoints (`/claims/maker/list`, `/claims/checker/list`) both support `pageNumber` and `pageSize` query parameters, with a max of 100 items per page. The response always includes `totalCount` so the client knows how many pages there are. This is important because the problem statement says the system could process thousands of claims per day — returning everything at once isn't an option.

### Error Responses

I tried to make error responses consistent and informative. There are two patterns:
- Single message errors: `{"message": "Claim not found"}` — for things like 404s or business rule violations
- Validation errors: `{"errors": ["Patient name is required", "..."]}` — for input validation failures, where multiple things might be wrong at once

All errors use standard HTTP status codes. 400 for bad input or business rule violations, 401 for authentication failures, 403 for authorization failures, 404 for missing resources, 409 for conflicts (like duplicate usernames).

---

## Task 3: Implementation

### Architecture Choices

I used a layered architecture: Controllers → Services → Repositories → Database. This is a common pattern in .NET applications and for good reason — each layer has a clear responsibility.

- **Controllers** handle HTTP concerns only: parsing the request, calling the right service method, returning the response. They don't contain business logic.
- **Services** contain the business logic: workflow rules, state transitions, audit logging.
- **Repositories** contain database queries. By abstracting these behind interfaces, the service layer doesn't know (or care) whether data comes from PostgreSQL or somewhere else.

For a junior-level project this might look like over-engineering, but the problem statement specifically mentions that this needs to support hundreds of concurrent users and is expected to grow. Starting with clean separation makes future changes much less painful.

### Concurrency

This was the most interesting technical challenge. The problem says "multiple Makers and Checkers will be operating the system concurrently" and explicitly asks for concurrent-safe state transitions.

My approach uses **pessimistic locking via a database column**. The `Claims` table has a `LockedByUserId` column. When a Maker or Checker locks a claim, their user ID is written to that column. Any attempt to lock an already-locked claim checks this column first and fails if it's not null.

This isn't the only way to handle this. Another approach is **optimistic concurrency** (using a row version/timestamp and failing if someone else modified the row between your read and your write). Pessimistic locking is simpler to reason about and explain, which is appropriate here. In a very high-throughput system, optimistic concurrency might be better because it has less contention.

I'm aware the current implementation doesn't use database-level row locking (SELECT FOR UPDATE) — it relies on application-level checking. This is mostly safe but has a small theoretical race window under very high concurrency. For production, I'd add an explicit database-level lock or use a transaction with isolation level `Serializable` for the lock operation.

### JWT Authentication

Each user gets a JWT token when they log in. The token is signed with a secret key and contains the user's ID, username, role, and insurance company ID. This last one is important — it means the service can read which company the user belongs to directly from the token without an extra database lookup.

Tokens expire after 1 hour in production (24 hours in development). There's no refresh token mechanism — the user just logs in again. For a first version this is acceptable. A real system would want refresh tokens so users don't have to re-enter credentials every hour.

### Input Validation

I used **FluentValidation** rather than .NET's built-in Data Annotations. The main advantage is that validation rules are separate from the DTO classes — they're easier to read, test, and modify without touching the model itself.

### What I Didn't Build (And Why)

**Docker / docker-compose** — The Tier 1 bonus says to containerise the app. I set up the code to work with Docker, but I didn't include a full docker-compose.yml in this version. The connection string is in `appsettings.Development.json` and could easily be moved to environment variables for container deployment.

**Real insurer API integration** — The problem explicitly says a stub is fine, so `forward-to-insurer` just updates the status and writes an audit log. If this were a real project, I'd add a background job or message queue to handle the actual HTTP call asynchronously.

---

## Design Decisions I'd Revisit in Production

**Auto-unlock stale locks**
Currently, if a Maker locks a claim and their browser crashes, that claim stays locked forever (until someone manually unlocks it). A real system needs a background job that checks the `LockedAt` timestamp and unlocks claims that have been locked for too long (say, 2 hours with no activity).

**Multi-tenancy**
Right now users are associated with one insurance company, and the code filters claims by company. But the filtering isn't consistently enforced in every query — it relies on the service layer doing the right thing. For a proper multi-tenant system, I'd enforce this at the database level (row-level security in PostgreSQL) so there's no risk of data leaking between companies if a code change accidentally skips the filter.

**Refresh tokens**
1-hour JWT expiry is good for security but annoying in practice. Adding refresh tokens with a longer expiry (7 days or so) would let users stay logged in without having to re-enter credentials while still revoking access quickly if a token is compromised.

**Idempotency on claim ingestion**
Currently, if the upstream service sends the same claim twice (e.g., due to a retry), two separate claim records get created. In production, you'd want to detect and deduplicate based on some combination of patient ID, service date, amount, and provider — or give each claim a unique external reference ID that the upstream system generates.

---
