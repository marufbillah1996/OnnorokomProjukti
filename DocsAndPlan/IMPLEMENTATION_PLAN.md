# Assignment & Submission Management System — Implementation Plan

## 1. Project Overview
A role-based school/college web application for managing assignments and submissions. The system supports three roles — Admin, Teacher, and Student — with strict role-based access control, resource-level ownership enforcement, and streamlined academic workflows.

**Architectural commitment:** both the backend and frontend follow a strictly layered architecture with an enforced, one-directional dependency rule — not just as prose convention, but as something CI actively fails on if violated (NetArchTest assertions on the backend, ESLint import-boundary rules on the frontend). §3 and §4 define the layers; §8's testing/CI pipeline is where the rule is mechanically enforced.

## 2. Technology Stack
* **Frontend:** Next.js (App Router), React, TypeScript.
  * *Styling & UI:* **Tailwind CSS** as the primary styling engine. The provided `education-theme` package only contains marketing/auth pages (`index`, `login`, `register`, `contact`, `blank-page`, `404`) — it has **no dashboard, data-table, or grading-panel templates**. It is used strictly as a **visual language reference** (color palette, typography, spacing, button/card styles, and the login/register page layout), not as literal templates. All dashboard, table, and workflow screens (admin console, teacher grading UI, student submission UI) are custom-built in Tailwind following that visual language, as reusable design-system primitives (§4).
  * *Forms & Validation:* React Hook Form integrated with Zod (shared validation shape mirrors backend FluentValidation rules where practical).
  * *API Integration:* Axios with a typed API client + TanStack React Query for caching, retries, and mutation state.
  * *Auth/session:* HttpOnly cookie or memory-held JWT (see §10.2) with a React context/hook (`useAuth`) plus **Next.js middleware** for edge-level route guarding by role, in addition to backend enforcement.
* **Backend:** ASP.NET Core Web API (.NET 8 LTS), C#.
  * *Architecture:* Clean Architecture (see §3 for concrete solution layout and enforcement), Repository + Unit of Work pattern behind the Application layer's service interfaces.
  * *Validation:* FluentValidation for request DTOs, executed via a validation pipeline/filter so controllers stay thin.
  * *Error Handling & Logging:* Global Exception Handling Middleware returning RFC 7807 `ProblemDetails`, Serilog (Console + rolling file sinks, structured JSON), request correlation IDs.
  * *Documentation:* Swagger/OpenAPI with JWT Bearer auth wired into the Swagger UI "Authorize" button and XML doc comments surfaced on operations.
* **Database:** PostgreSQL (chosen over MongoDB — the domain is strongly relational: users↔classes↔subjects↔assignments↔submissions with several many-to-many and referential-integrity rules, e.g. "one submission per student per assignment," which PostgreSQL enforces natively via constraints).
  * *ORM:* Entity Framework Core (Code-First, migrations committed to the repo).
* **Authentication & Authorization:** JWT-based authentication with refresh tokens, ASP.NET Core policy-based authorization for roles **and** a custom resource-based authorization handler for ownership rules (see §10.1).
* **Testing:**
  * Backend: xUnit + Moq (+ FluentAssertions) for unit tests; WebApplicationFactory-based integration tests for the auth/authorization pipeline; **NetArchTest** architecture fitness tests (§3.2).
  * Frontend: Jest + React Testing Library for the deadline-gated submission form and role-guarded routing logic; **ESLint boundary rules** enforced in CI (§4.2).

## 3. Backend Architecture (Clean Architecture)

### 3.1 Solution Structure
```text
backend/
├── src/
│   ├── AssignmentHub.Domain/          # Entities, enums, value objects, domain exceptions — ZERO project references
│   ├── AssignmentHub.Application/     # DTOs, interfaces (IAssignmentRepository, ICurrentUser...), services,
│   │                                  # FluentValidation validators, business-rule logic (deadline checks,
│   │                                  # ownership checks), AutoMapper profiles — references Domain only
│   ├── AssignmentHub.Infrastructure/  # EF Core DbContext, migrations, repository implementations,
│   │                                  # JWT token generation, SignalR hub, Serilog config, IMemoryCache
│   │                                  # — references Application + Domain, implements Application's interfaces
│   └── AssignmentHub.Api/             # Controllers, middleware, DI composition root, Swagger config
│                                      # — references Application (for use-case calls) + Infrastructure (DI wiring only)
└── tests/
    ├── AssignmentHub.UnitTests/            # Application-layer business rule tests (Moq-based)
    ├── AssignmentHub.IntegrationTests/     # WebApplicationFactory: auth, authorization, submission workflow
    └── AssignmentHub.ArchitectureTests/    # NetArchTest fitness functions (§3.2)
```

**Dependency rule:** `Api → Infrastructure → Application → Domain`. Domain never references anything. Application never references Infrastructure or Api — it depends only on interfaces it defines itself, which Infrastructure implements (Dependency Inversion). Controllers in Api call Application services/use-cases; they never call `DbContext` or any Infrastructure type directly. This is what keeps business rules unit-testable without a database and keeps persistence swappable.

### 3.2 Enforcing the Rule (Architecture Fitness Tests)
A dedicated `AssignmentHub.ArchitectureTests` project uses **NetArchTest.Rules** to assert the dependency rule as an executable, CI-gated test rather than a convention developers can silently drift from:
```csharp
Types.InAssembly(DomainAssembly)
    .Should().NotHaveDependencyOnAny("AssignmentHub.Application", "AssignmentHub.Infrastructure", "AssignmentHub.Api")
    .GetResult().IsSuccessful.Should().BeTrue();

Types.InAssembly(ApplicationAssembly)
    .Should().NotHaveDependencyOnAny("AssignmentHub.Infrastructure", "AssignmentHub.Api")
    .GetResult().IsSuccessful.Should().BeTrue();

Types.InAssembly(ApiAssembly)
    .That().ResideInNamespace("AssignmentHub.Api.Controllers")
    .Should().NotHaveDependencyOn("AssignmentHub.Infrastructure.Persistence") // no direct DbContext access from controllers
    .GetResult().IsSuccessful.Should().BeTrue();
```
This runs as part of the standard `dotnet test` step in `ci-backend.yml` (§9) — a layering violation fails the build, the same way a broken unit test would.

## 4. Frontend Architecture (Feature-Layered)

### 4.1 Folder Structure
```text
frontend/
└── src/
    ├── app/                     # Next.js App Router — ROUTING & COMPOSITION ONLY.
    │   │                        # Pages import from features/ and shared/; no business logic, no direct axios calls.
    │   ├── (auth)/login/
    │   ├── admin/...
    │   ├── teacher/...
    │   ├── student/...
    │   └── middleware.ts        # role-based route guarding (§7)
    ├── features/                # One folder per vertical feature slice
    │   ├── auth/                 # api.ts, hooks.ts (useLogin, useAuth), components/, schema.ts (zod), types.ts
    │   ├── assignments/
    │   ├── submissions/
    │   ├── academics/            # classes/subjects/teacher-assignment (admin)
    │   ├── users/
    │   └── notifications/
    ├── shared/
    │   ├── components/ui/        # design-system primitives (Button, Card, DataTable, Modal...) — theme-derived (§2)
    │   ├── hooks/                 # cross-feature hooks (usePagination, useDebounce)
    │   ├── lib/                   # api-client.ts (axios instance + interceptors), query-client.ts,
    │   │                          # date.ts (UTC↔local conversion, §10.4), auth-storage.ts
    │   └── types/                  # cross-cutting types (Role, ApiError, Paginated<T>)
    └── styles/                     # Tailwind config/tokens derived from the education-theme palette
```

### 4.2 Dependency Rule & Enforcement
Mirrors the backend's inward-only dependency rule:
* `app/**` may import from `features/**` and `shared/**` — never the reverse.
* `features/<X>/**` may import from `shared/**` only — never directly from another `features/<Y>/**` slice (cross-feature reuse is promoted into `shared/` first) and never from `app/**`.
* `shared/**` imports nothing from `features/**` or `app/**`.

Enforced via `eslint-plugin-boundaries` (or `import/no-restricted-paths`) configured in `.eslintrc`, with element types mapped to `app`, `features`, `shared`. `ci-frontend.yml` (§9) runs `npm run lint` as a required check, so a cross-feature or feature→app import is a **CI failure**, not a code-review nitpick.

## 5. Database Data Model (PostgreSQL)

* **Users:** `Id`, `Name`, `Email` (unique index), `PasswordHash`, `Role` (enum: Admin, Teacher, Student), `IsActive`, `CreatedAt`, `UpdatedAt`
* **RefreshTokens:** `Id`, `UserId`, `TokenHash`, `ExpiresAt`, `RevokedAt`, `CreatedAt` — supports refresh/rotation and logout-everywhere.
* **Classes:** `Id`, `Name`, `Description`, `IsDeleted` (soft delete)
* **Subjects:** `Id`, `Name`, `Code`, `IsDeleted`
* **ClassSubjects (join):** `Id`, `ClassId`, `SubjectId`, `TeacherId`, unique index on `(ClassId, SubjectId)` — one teacher per class/subject combination at a time.
* **StudentClasses (join):** `StudentId`, `ClassId`, composite PK — enrollment.
* **Assignments:** `Id`, `Title`, `Description`, `DeadlineUtc` (`timestamptz`, always stored/compared in UTC), `MaxMarks` (int, checked > 0), `Status` (enum: Draft, Published), `ClassSubjectId`, `CreatedByTeacherId`, `IsDeleted`, `CreatedAt`, `UpdatedAt`, `RowVersion` (optimistic concurrency token).
  * Index on `(ClassSubjectId, Status)` for the student "view published assignments" query; index on `DeadlineUtc` for dashboard/"upcoming deadline" queries.
* **Submissions:** `Id`, `AssignmentId`, `StudentId`, `Content` (text/markdown), `SubmittedAt`, `UpdatedAt`, `Status` (enum: Pending, Graded, Returned), `MarksAwarded` (int?, checked ≤ `Assignment.MaxMarks`), `Feedback` (string?), `IsDeleted`.
  * **Unique index on `(AssignmentId, StudentId)`** — enforces exactly one submission row per student per assignment (updates modify the existing row; prevents race-condition duplicates on double-click submit).
* **GradeAuditLogs:** `Id`, `SubmissionId`, `GradedByTeacherId`, `PreviousStatus`, `NewStatus`, `PreviousMarks`, `NewMarks`, `GradedAt` — lightweight audit trail over grading actions.
* **Notifications:** `Id`, `UserId`, `Type` (AssignmentPublished, SubmissionGraded), `Payload` (jsonb), `IsRead`, `CreatedAt` — persisted backing store for the SignalR real-time push (§9.1), so notifications survive refresh/offline and aren't purely transient.
* **Settings:** `Id`, `SettingKey` (unique), `SettingValue` — generic KV store for admin-managed app settings (e.g. `LateSubmissionPolicy`, `MaintenanceMode`).

**Soft-delete policy:** `IsDeleted` + EF Core Global Query Filters apply to `Users`, `Classes`, `Subjects`, `Assignments`, `Submissions`. Deactivating a Teacher does not delete their historical `Assignments`/`ClassSubjects` rows — `IsActive=false` on the user prevents login while preserving referential history.

## 6. RESTful API Endpoints Design

All list endpoints support a common convention: `?page=&pageSize=&sortBy=&sortDir=&search=`. All error responses use RFC 7807 `ProblemDetails` with an `errors` dictionary for field-level validation failures, so the frontend has one shape to parse everywhere.

### Authentication
* `POST /api/auth/login` — authenticate, return access token (short-lived, ~15 min) + refresh token (long-lived, httpOnly cookie).
* `POST /api/auth/refresh` — exchange a valid refresh token for a new access token; rotates the refresh token.
* `POST /api/auth/logout` — revokes the current refresh token.

### Admin Endpoints (`Authorize(Roles = "Admin")`)
* `GET/POST/PUT/DELETE /api/users` — manage all users (paginated/filterable by role, active status).
* `GET/POST/PUT/DELETE /api/classes`, `/api/subjects` — manage classes/subjects.
* `POST /api/classes/{classId}/subjects/{subjectId}/teachers/{teacherId}` — assign teacher to subject/class.
* `POST /api/classes/{classId}/students/{studentId}` — enroll student in a class.
* `GET /api/admin/assignments`, `GET /api/admin/submissions` — global view, paginated/filterable.
* `GET/PUT /api/settings` — manage application settings.

### Teacher Endpoints (`Authorize(Roles = "Teacher")` + resource-ownership check)
* `GET/POST/PUT/DELETE /api/assignments` — CRUD, restricted by the ownership authorization handler (§10.1) to assignments the authenticated teacher created for a `ClassSubject` they are assigned to.
* `PUT /api/assignments/{id}/publish` — Draft → Published.
* `GET /api/assignments/{id}/submissions` — paginated submissions for a specific assignment.
* `PUT /api/submissions/{id}/grade` — sets `MarksAwarded`, `Feedback`, and transitions `Status` (Pending → Graded); writes a `GradeAuditLogs` row.
* `PUT /api/submissions/{id}/status` — explicit manual status override (e.g. **Returned**, to reopen a submission for correction) — kept separate from grading per the PDF's distinct "change submission status when necessary" responsibility.

### Student Endpoints (`Authorize(Roles = "Student")`)
* `GET /api/student/assignments` — published assignments for the student's enrolled classes.
* `GET /api/student/assignments/{id}` — assignment detail + deadline (in UTC + client renders local time).
* `POST /api/student/assignments/{id}/submit` — create/upsert the student's submission (blocked once `Status != Pending`, unless reopened via the teacher's status override).
* `PUT /api/student/submissions/{id}` — update, validated server-side against `DeadlineUtc >= DateTime.UtcNow`.
* `GET /api/student/submissions` — submission history with statuses, marks, feedback.

### Shared / Infrastructure
* `GET /health` — liveness/readiness endpoint for Docker healthchecks and CI smoke tests.
* SignalR hub at `/hubs/notifications` (JWT-authenticated) — pushes `AssignmentPublished` / `SubmissionGraded` events.

## 7. Frontend UI Structure & Workflows

### General
* `/login` — secure login page (styling adapted from `education-theme/login.html`), role-based redirect after auth.
* Next.js **middleware** (`middleware.ts`) inspects the session/role claim and redirects unauthenticated or wrong-role users before a protected page renders — client-side defense-in-depth on top of backend enforcement (which remains the authoritative check).
* **Shared components** (from `shared/components/ui`, §4.1): navigation sidebar, responsive header, loading skeletons, toast notifications, a live notification bell wired to the SignalR hub.

### Admin Dashboard (`/admin/*`)
* `/admin/dashboard` — platform metrics.
* `/admin/users` — data table for user management (search/filter/paginate).
* `/admin/academics` — manage Classes, Subjects, and Teacher↔ClassSubject mapping.
* `/admin/monitoring` — global read-only view of all assignments and submissions.

### Teacher Dashboard (`/teacher/*`)
* `/teacher/dashboard` — active assignments + submissions pending grading.
* `/teacher/assignments` — assignment management grid; multi-step "Create Assignment" form.
* `/teacher/assignments/[id]/submissions` — grading interface: submission content side-by-side with a grading panel (marks, feedback, status control).

### Student Dashboard (`/student/*`)
* `/student/dashboard` — "To-Do" widget highlighting upcoming deadlines (computed client-side from UTC deadlines, displayed in local time).
* `/student/assignments` — grid of class assignments.
* `/student/assignments/[id]` — detail view with the submission form (rich text editor), disabled/read-only once the deadline has passed or the submission is graded; displays feedback once graded.

## 8. Testing Strategy

### Backend (xUnit + Moq)
* JWT generation and claims correctness (role, user id, expiry).
* **Resource-based authorization handler**: teacher A cannot grade/edit/publish an assignment owned by teacher B, even with a valid Teacher-role token.
* Deadline rule: student submission/update is rejected once `DeadlineUtc` has passed (boundary-tested at exactly the deadline).
* Draft assignments are excluded from every student-facing query.
* Duplicate submission attempts hit the unique `(AssignmentId, StudentId)` constraint and return a clean 409/validation error, not a 500.
* Grading validation: `MarksAwarded` cannot exceed `Assignment.MaxMarks`.
* Role-based endpoint protection across all three roles (integration tests via `WebApplicationFactory`).
* **Architecture fitness tests** (§3.2): Domain/Application layer isolation, controllers never bypass Application to reach Infrastructure directly.

### Frontend (Jest + React Testing Library)
* Deadline-gated submission form: submit button disabled/hidden after deadline; countdown/label reflects server-provided UTC time correctly across the local browser timezone.
* Role-guarded route/middleware logic: wrong-role access attempts redirect as expected.
* **Layering static analysis** (§4.2): ESLint boundary rules run as a required CI check, not a Jest test, but is the frontend's equivalent fitness function to the backend's NetArchTest suite.

## 9. Delivery & Repository Structure
```text
/
├── backend/            # ASP.NET Core Web API solution (Clean Architecture, see §3)
├── frontend/           # Next.js application (feature-layered architecture, see §4)
├── database/           # EF Core migrations bundled in backend; this folder holds the seed SQL/backup
│                       # and any standalone DB scripts for non-Docker setups
├── docker-compose.yml  # One-command full local setup: postgres + backend (auto-runs EF migrations
│                       # and seed data on startup) + frontend
├── .github/workflows/  # ci-backend.yml (build + xUnit + NetArchTest) and
│                       # ci-frontend.yml (lint incl. boundary rules + typecheck + build + Jest)
├── README.md           # See §12 for required structure
└── .env.example        # Environment configuration template (see §10.3)
```
`docker-compose up` is the **committed, primary** local-setup path (not merely optional) — the backend's `Program.cs` calls `context.Database.Migrate()` and runs the seed routine (§13) on startup, satisfying the PDF's "evaluator should be able to set up the database without manually creating tables" requirement unconditionally. A non-Docker fallback (local Postgres + `dotnet ef database update`) is documented as a secondary path.

### Final Checklist for Evaluator Compliance
- [x] Complete source code (Frontend, Backend, DB, Unit Tests).
- [x] Database migrations and seed/sample data scripts (Admin, Teacher, Student pre-loaded — see §13 for concrete seed spec).
- [x] Comprehensive `README.md` (see §12 for the exact required structure, including Known Limitations).
- [x] Demo credentials provided clearly, backed by real seeded data (not just empty accounts).
- [x] Clean environment configuration (`.env.example`).
- [x] Docker containerization as the default one-command setup; Swagger UI enabled with Bearer-auth support.
- [x] Layered architecture mechanically enforced in CI on both stacks (NetArchTest + ESLint boundaries) — not just documented intent.

## 10. Cross-Cutting Design Decisions

### 10.1 Authorization Model
Two layers, both enforced server-side (client-side guarding in §7 is UX-only):
1. **Role-based:** `[Authorize(Roles = "Admin|Teacher|Student")]` on controllers/actions — coarse gate.
2. **Resource-based:** a custom `IAuthorizationHandler` (`AssignmentOwnershipRequirement`) checks that `assignment.CreatedByTeacherId == currentUser.Id` (or that the teacher is currently assigned to the assignment's `ClassSubject`) before allowing edit/publish/grade actions. This is what actually implements "Teacher can only manage their own created assignments" — a plain role check cannot express it.

### 10.2 Authentication Details
* Passwords hashed with ASP.NET Core's `PasswordHasher<T>` (PBKDF2) — never a custom hash.
* Access token: short-lived JWT (~15 min), signed with an HMAC secret from configuration/env, containing `sub`, `role`, `email` claims.
* Refresh token: opaque random value, hashed at rest in `RefreshTokens`, delivered as an httpOnly cookie, rotated on each use, revocable (supports logout and "logout everywhere").
* **No public self-registration** — per the PDF's role model, only Admin creates Teacher/Student accounts via `/api/users`. The very first Admin account is created exclusively by the database seed routine (§13); this is stated explicitly in the README so it isn't mistaken for a missing feature.
* **CORS:** the Api project configures an explicit allowed-origins policy (from `Cors:AllowedOrigins` in config/env) scoped to the frontend's origin, since frontend and backend run on different ports/origins in local dev and in Docker Compose.
* **Rate limiting:** .NET 8 built-in rate limiter applied to `/api/auth/login` and `/api/auth/refresh` to blunt brute-force attempts.

### 10.3 Environment Configuration (`.env.example` contents)
```
# Database
POSTGRES_DB=assignmenthub
POSTGRES_USER=postgres
POSTGRES_PASSWORD=changeme
ConnectionStrings__Default=Host=localhost;Port=5432;Database=assignmenthub;Username=postgres;Password=changeme

# JWT
Jwt__Secret=replace-with-a-strong-secret
Jwt__Issuer=AssignmentHub
Jwt__Audience=AssignmentHub.Client
Jwt__AccessTokenMinutes=15
Jwt__RefreshTokenDays=7

# CORS
Cors__AllowedOrigins=http://localhost:3000

# Frontend
NEXT_PUBLIC_API_BASE_URL=http://localhost:5000/api
```

### 10.4 Timezone Handling
Deadlines are stored and compared in UTC exclusively on the server (`DeadlineUtc`, `DateTime.UtcNow`); `shared/lib/date.ts` on the frontend is the single conversion point to the viewer's local timezone for display — no client ever performs the authoritative deadline comparison.

## 11. Advanced / Enterprise Features

1. **Real-Time Notifications (SignalR):** hub at `/hubs/notifications`, backed by the persisted `Notifications` table (§5) so history survives disconnects; client subscribes on login and shows a live badge/toast for `AssignmentPublished` and `SubmissionGraded` events.
2. **CI/CD (GitHub Actions):** `ci-backend.yml` (build + xUnit + NetArchTest on every push/PR) **and** `ci-frontend.yml` (lint incl. boundary rules + typecheck + build + Jest) — both stacks covered, including architectural enforcement.
3. **Soft Deletes:** EF Core Global Query Filters on Users/Classes/Subjects/Assignments/Submissions (§5).
4. **API Rate Limiting:** .NET 8 built-in middleware on auth endpoints (§10.2).
5. **Caching:** `IMemoryCache` for read-heavy, low-churn data (`Classes`, `Subjects`), invalidated on admin writes.
6. **Grading Audit Trail:** `GradeAuditLogs` (§5) — who graded what, when, and the before/after values.
7. **Health Check Endpoint:** `/health` for Docker Compose healthchecks and CI smoke verification.
8. **Correlation IDs:** a lightweight middleware stamps each request with a correlation ID propagated into Serilog's structured log output, enabling end-to-end request tracing.

## 12. README.md Required Structure
1. Project overview
2. Main features (by role)
3. Technology stack
4. Architecture / project structure (link §3 backend, §4 frontend — including the enforced dependency rules)
5. Setup instructions (Docker-first, manual fallback)
6. Database setup instructions (migrations + seeding, §13)
7. Running the frontend / backend individually
8. Running the tests (backend xUnit + NetArchTest, frontend Jest + ESLint boundaries)
9. Demo credentials table (Admin/Teacher/Student — matching the seeded accounts in §13)
10. Design decisions & assumptions (including the `education-theme` scope note from §2, timezone handling, no-file-upload decision)
11. **Known limitations** — stated up front rather than discovered by the evaluator:
    * Submissions are text/markdown only; no file/attachment upload.
    * No email/SMS delivery — notifications are in-app (SignalR) only.
    * No self-service password reset flow (Admin resets via user management).
    * Single-tenant design (one school/college instance per deployment).

## 13. Stated Assumptions
1. **Submission Format:** text-based (Markdown/Rich Text); no external file storage, to keep the evaluation environment dependency-free.
2. **Teacher Assignment:** a Teacher must be explicitly assigned to a `ClassSubject` by an Admin before creating assignments for that cohort.
3. **Application Settings:** implemented as a generic Key-Value store, admin-managed.
4. **User Provisioning:** no public registration; Admin provisions Teacher/Student accounts; the first Admin account exists only via database seeding.
5. **Deadlines:** stored and compared in UTC server-side (§10.4); the frontend converts to the viewer's local timezone for display only.

## 14. Seed Data Specification
The Docker/startup seed routine (idempotent — checks for existing data before inserting) creates, at minimum:
* **Users:** 1 Admin, 2 Teachers, 3 Students (credentials documented in README §12.9, never committed as real secrets — generated from `.env` values at seed time in non-demo environments, fixed well-known demo values acceptable for the local/demo Docker profile only).
* **Academics:** 2 Classes, 3 Subjects, teacher-to-class-subject assignments covering both teachers.
* **Enrollments:** all 3 students enrolled across the 2 classes.
* **Assignments:** at least one in `Draft` and several in `Published` status, with a mix of past and future deadlines (to exercise the deadline-lock UI/logic on first login).
* **Submissions:** a mix of `Pending`, `Graded`, and `Returned` statuses so every dashboard view (Admin monitoring, Teacher grading queue, Student history) shows non-empty, representative data immediately after `docker-compose up`.
