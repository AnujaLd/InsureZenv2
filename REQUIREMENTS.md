# Requirements Analysis — InsureZen

This document breaks down the requirements for the InsureZen claim processing system, including the entities involved, who does what, the rules the system must follow, and the decisions I made when things weren't spelled out.

---

## The Problem

Insurance companies receive medical claims on paper forms, all in different formats. InsureZen takes those forms, extracts the relevant data (via a separate service — not our job), and runs each claim through an internal review process before sending it back to the insurer.

The review process has two stages. First, a Maker employee looks at the claim data and says "looks good, approve it" or "something's wrong, reject it." Then a Checker employee looks at the same claim *plus* the Maker's feedback, and makes the final call. Once decided, the claim record goes back to the insurer.

Our job is to build the API that powers this workflow.

---

## 1. The Entities

### Insurance Company
This is the partner company that sent the claim.

Fields: ID, Name, Code, Contact Email, Contact Phone, Active/Inactive flag, Created date

### User (Maker or Checker)
An InsureZen employee who reviews claims. Every user belongs to one insurance company.

Fields: ID, Username, Email, Password (hashed), Role (Maker or Checker), Company ID, Active flag, Created date

### Claim
The central object — a single insurance claim submitted for review.

Fields: ID, Claim Number (auto-generated), Patient Name, Patient ID, Service Date, Claim Amount, Service Description, Provider Name, Provider Code, Status, Locked By (who's currently reviewing it), Company ID, Submitted date, Completed date

The status field is how we track where the claim is in the workflow. It can be:
- **Pending** — just arrived, nobody is looking at it yet
- **MakerInProgress** — a Maker has locked it and is reviewing
- **MakerSubmitted** — Maker is done, waiting for a Checker
- **CheckerInProgress** — a Checker has locked it and is reviewing
- **Approved** — Checker said yes
- **Rejected** — Checker said no
- **ForwardedToInsurer** — sent back to the insurance company

### Claim Review
A record of what a Maker or Checker said about a claim. Each claim gets one Maker review and one Checker review.

Fields: ID, Claim ID, Reviewer ID, Is Maker Review (true/false), Feedback text, Maker's recommendation (Approve/Reject), Checker's decision (Approved/Rejected), Review date

### Audit Log
A record of every action taken in the system — for compliance and debugging.

Fields: ID, Claim ID, User ID, Action type, Details, Timestamp

---

## 2. The Actors

### Maker
A Maker is the first human reviewer. Their job is to look at the raw claim data and decide whether it looks legitimate.

What a Maker can do:
- See all Pending claims (and ones they've already locked)
- Lock a claim so no one else can touch it
- Submit a review with feedback and a recommendation
- Unlock a claim if they decide not to review it

What a Maker cannot do:
- See the Checker's decision before it's final
- Submit the final approval or rejection (that's the Checker's job)
- Review claims that haven't been assigned to them

### Checker
A Checker is the second reviewer. They see the claim *and* the Maker's feedback, then make the final decision.

What a Checker can do:
- See all claims that have been through Maker review
- Lock a claim to review it
- Submit a final decision (Approved or Rejected)
- Unlock a claim

What a Checker cannot do:
- Edit the Maker's feedback
- Skip reviewing and just approve without feedback

### System (Automatic)
The system itself manages a few things automatically:
- Enforces which status transitions are allowed
- Generates unique claim numbers
- Logs all actions to the audit trail
- Prevents a claim from being locked by two people at once

---

## 3. Functional Requirements

These are the things the system must be able to do.

| # | Requirement | Priority |
|---|-------------|----------|
| 1 | Accept a new claim via API with all relevant fields | Must have |
| 2 | Register users with a Maker or Checker role | Must have |
| 3 | Allow users to log in and receive a JWT token | Must have |
| 4 | Enforce role-based access (Makers can't use Checker endpoints, and vice versa) | Must have |
| 5 | Let Makers see a paginated list of claims waiting for their review | Must have |
| 6 | Let Checkers see a paginated list of claims waiting for their review | Must have |
| 7 | Let a Maker lock a claim before reviewing it | Must have |
| 8 | Let a Maker submit feedback and a recommendation | Must have |
| 9 | Let a Checker lock a claim before reviewing it | Must have |
| 10 | Let a Checker submit a final decision | Must have |
| 11 | Prevent two users from locking the same claim at the same time | Must have |
| 12 | Allow claims to be forwarded to the insurer after a final decision | Must have |
| 13 | Validate all inputs with clear error messages | Must have |
| 14 | Return the full claim details including both reviews | Must have |
| 15 | Support filtering claims by status and date range | Should have |
| 16 | Log every action to an audit trail | Should have |

---

## 4. Non-Functional Requirements

These are about *how well* the system should work, not just *what* it does.

**Concurrency** — Multiple Makers and Checkers will work at the same time. The system must prevent two people from reviewing the same claim simultaneously without them knowing. We handle this with a locking mechanism (one person at a time can hold a claim).

**Data integrity** — A claim should never end up in an impossible state (for example: a claim that's been approved but has no Checker review). The system enforces valid state transitions only.

**Auditability** — Every action must be logged with who did it and when. This is important for compliance in the insurance industry.

**Security** — Passwords are hashed (not stored in plain text). All API calls require a valid JWT token. Sensitive endpoints are role-restricted.

**Performance** — List endpoints are paginated so the API stays fast even with thousands of claims. Database indexes are on the columns we filter and sort by most often.

**Reliability** — The API returns consistent, meaningful error messages. If something goes wrong, the error response explains what happened.

---

## 5. Edge Cases and How I Handled Them

**Two Makers try to lock the same claim at the same time**
The first one succeeds. The second gets a 400 error: "Claim is already locked by another user." This is enforced via the `LockedByUserId` database column — whoever sets it first wins.

**A Maker locks a claim but never submits a review**
The claim stays locked indefinitely until the Maker manually unlocks it (or an admin does). In a real production system I'd add an auto-unlock timeout, but that's outside scope here.

**A Checker tries to review a claim that doesn't have a Maker review yet**
Not allowed. The Checker's list only shows claims in `MakerSubmitted` status, so this can't happen through normal use. If someone sends the API request directly, the status check will reject it.

**Someone submits a review on a claim they don't have locked**
The system checks that the claim's `LockedByUserId` matches the authenticated user's ID. If it doesn't match, the request is rejected with a 403.

**Someone tries to forward a claim that isn't approved or rejected yet**
Only `Approved` or `Rejected` claims can be forwarded. Any other status returns a 400 error.

**A claim gets forwarded twice**
Once a claim is `ForwardedToInsurer`, it can't be forwarded again (that would be a duplicate). The status check prevents this.

**Service date is in the future**
Claims can't be for services that haven't happened yet. The validator rejects any `serviceDate` that's in the future.

**Duplicate username or email on registration**
The database has unique constraints on username and email. The API returns a 409 Conflict with a clear message.

---

## 6. Assumptions I Made

These are things the problem statement left ambiguous. I've documented my reasoning so it can be discussed in the review.

**1. No OCR or document parsing**
The problem statement says an upstream service already handles this. My API accepts clean, structured JSON. I designed the `ClaimIngestDto` as the "standardised input format" the problem mentions.

**2. Users belong to one insurance company**
I assume each Maker and Checker works for (and can only see claims from) one insurance company. This provides basic data isolation without needing full multi-tenancy infrastructure.

**3. Makers and Checkers from the same company can review each other's work**
The problem doesn't say they must be from the same company, but it makes operational sense. A Maker from Company A should have their work checked by a Checker also from Company A.

**4. No actual API call to the insurer**
The "forwarding" action creates an audit log entry and updates the claim status to `ForwardedToInsurer`. No real HTTP call is made — the problem statement says a stub is fine.

**5. Anyone with a valid JWT can ingest claims**
The problem says the upstream service delivers structured data. I allow both Makers and Checkers to call `/claims/ingest`, since it's unclear which role would be doing the ingesting in practice.

**6. JWT tokens expire after 1 hour (production) or 24 hours (development)**
These are reasonable defaults. Production is shorter for security; development is longer so you're not constantly re-logging in while testing.

**7. No email verification**
Users become active immediately after registration. Adding email verification would require an email service, which is outside scope.

**8. Passwords can't be changed through the API**
Password reset/change functionality is not part of the assignment. Users who forget their password would need admin intervention (or a future endpoint).

**9. Claim data is immutable after ingestion**
Once a claim is submitted, the core fields (patient name, amount, etc.) cannot be edited. If there's a data error, the Maker's feedback notes are the place to document it.

**10. Audit logs are never deleted**
The `AuditLogs` table is append-only. This is a deliberate choice for compliance — you need to be able to prove what happened and when.
