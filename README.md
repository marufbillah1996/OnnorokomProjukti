# AssignmentHub — Assignment & Submission Management System

A role-based school/college web application for managing assignments and submissions across three
roles — **Admin**, **Teacher**, and **Student** — built for the OnnoRokom Projukti Limited
Assistant Software Engineer recruitment project.

Full architectural plan and design-decision rationale: [`DocsAndPlan/IMPLEMENTATION_PLAN.md`](DocsAndPlan/IMPLEMENTATION_PLAN.md).

## 1. Overview

Teachers create assignments for a specific class/subject, publish them, and grade student
submissions with marks and feedback. Students view assignments for their enrolled classes,
submit answers before the deadline, and track their grades. Admins manage users, classes,
subjects, teacher assignments, and application-wide settings, with full read-only oversight of
every assignment and submission in the system.

## 2. Main Features

**Admin**
- Manage users (create/update/deactivate Admin, Teacher, Student accounts)
- Manage Classes and Subjects; assign Teachers to Class+Subject pairs; enroll Students in Classes
- Read-only oversight of every assignment and submission platform-wide
- Manage generic key/value application settings

**Teacher**
- Create, update, delete, and publish assignments for classes/subjects they're assigned to
- View submissions for their own assignments; grade with marks + feedback
- Return a graded/pending submission to a student for correction
- Live in-app notification bell (no action needed — pushes automatically on grading)

**Student**
- View published assignments for enrolled classes; draft assignments are never visible
- Submit an answer; update it any time before the deadline
- View submission status, marks, and teacher feedback
- Live notification when an assignment is published or a submission is graded

**Cross-cutting**
- JWT authentication (15-min access token + rotating refresh token) with role-based **and**
  resource-based authorization (a Teacher cannot touch another Teacher's assignment)
- Real-time push via SignalR; RFC 7807 `ProblemDetails` error responses throughout
- Soft deletes, grading audit trail, request correlation IDs, rate-limited auth endpoints
- Architecture correctness enforced by CI: NetArchTest layering rules (backend) and ESLint
  import-boundary rules (frontend) — a layering violation fails the build, not just a review comment

## 3. Technology Stack

| Layer | Stack |
|---|---|
| Frontend | Next.js 16 (App Router), React 19, TypeScript, Tailwind CSS v4, TanStack Query, React Hook Form + Zod, Axios, SignalR client |
| Backend | ASP.NET Core Web API (.NET 8), C#, Clean Architecture (Domain/Application/Infrastructure/Api), FluentValidation, Serilog, Swagger/OpenAPI |
| Database | PostgreSQL 16, EF Core (Code-First migrations) |
| Auth | JWT bearer + refresh-token rotation, ASP.NET Core policy + resource-based authorization |
| Testing | xUnit, Moq, MockQueryable.Moq, FluentAssertions, NetArchTest (backend) · Jest, React Testing Library (frontend) |
| Infra | Docker Compose (Postgres + backend + frontend), GitHub Actions CI |

## 4. Project Structure

```text
/
├── backend/            # ASP.NET Core Web API — Clean Architecture (see backend/AssignmentHub.sln)
│   ├── src/
│   │   ├── AssignmentHub.Domain/          # Entities, enums, domain exceptions — zero dependencies
│   │   ├── AssignmentHub.Application/     # DTOs, service interfaces/implementations, validators
│   │   ├── AssignmentHub.Infrastructure/  # EF Core, repositories, JWT, SignalR, authorization, seeding
│   │   └── AssignmentHub.Api/             # Controllers, middleware, Program.cs composition root
│   └── tests/
│       ├── AssignmentHub.UnitTests/
│       ├── AssignmentHub.IntegrationTests/
│       └── AssignmentHub.ArchitectureTests/
├── frontend/           # Next.js — feature-layered architecture
│   └── src/
│       ├── app/         # Routing & composition only (admin/teacher/student/login route groups)
│       ├── features/    # Vertical slices: auth, users, academics, assignments, submissions, notifications
│       └── shared/      # Design-system UI primitives, hooks, api-client, types
├── .github/workflows/  # ci-backend.yml, ci-frontend.yml
├── docker-compose.yml  # One-command local setup
├── .env.example        # Root env template (Docker Compose)
└── DocsAndPlan/         # Original brief PDF + the full implementation plan
```

## 5. Prerequisites

- **Docker Desktop** (recommended path — no local .NET/Node/Postgres install needed), **or**
- .NET 8 SDK, Node.js 20+, PostgreSQL 16 (for running each part natively)

## 6. Quick Start (Docker — recommended)

```bash
cp .env.example .env
# Edit .env and set a real JWT_SECRET (e.g. `openssl rand -base64 48`)

docker compose up --build
```

That single command:
1. Starts PostgreSQL and waits for it to be healthy.
2. Builds and starts the backend, which **automatically applies EF Core migrations and seeds demo
   data on startup** — no manual database setup step is needed.
3. Builds and starts the frontend.

Once containers are up:

| Service | URL |
|---|---|
| Frontend | http://localhost:3000 |
| Backend API | http://localhost:5000/api |
| Swagger UI | http://localhost:5000/swagger |
| Health check | http://localhost:5000/health |

## 7. Demo Credentials

Seeded automatically on first backend startup (see
`backend/src/AssignmentHub.Infrastructure/Persistence/Seed/DatabaseSeeder.cs`):

| Role | Email | Password |
|---|---|---|
| Admin | `admin@assignmenthub.local` | `Admin@123` |
| Teacher | `teacher1@assignmenthub.local` | `Teacher@123` |
| Teacher | `teacher2@assignmenthub.local` | `Teacher@123` |
| Student | `student1@assignmenthub.local` | `Student@123` |
| Student | `student2@assignmenthub.local` | `Student@123` |
| Student | `student3@assignmenthub.local` | `Student@123` |

Seed data includes 2 classes, 3 subjects, teacher-subject assignments, student enrollments, and
assignments/submissions in every status (Draft, Published, Pending, Graded, Returned, plus one
past-deadline assignment) so every dashboard shows meaningful data immediately.

## 8. Manual (Non-Docker) Setup

### 8.1 Database

Run a local PostgreSQL 16 instance (or `docker run -e POSTGRES_PASSWORD=changeme -p 5432:5432 postgres:16-alpine`),
matching the connection string in `backend/src/AssignmentHub.Api/appsettings.Development.json`
(`Host=localhost;Port=5432;Database=assignmenthub;Username=postgres;Password=changeme` by default
— edit it, or an environment variable override, to match your local instance).

Migrations and seed data are applied automatically the first time you run the backend — there is
no separate "create database" step.

### 8.2 Backend

```bash
cd backend
dotnet tool restore          # installs dotnet-ef locally, pinned via .config/dotnet-tools.json
dotnet restore
dotnet build
dotnet run --project src/AssignmentHub.Api
```

The API listens on `http://localhost:5000` (see `src/AssignmentHub.Api/Properties/launchSettings.json`).
`appsettings.Development.json` already contains a working (non-production) JWT secret so this
runs out of the box with no extra configuration.

To generate a new migration after changing an entity:
```bash
dotnet tool run dotnet-ef migrations add <Name> \
  --project src/AssignmentHub.Infrastructure \
  --startup-project src/AssignmentHub.Api \
  --output-dir Persistence/Migrations
```

### 8.3 Frontend

```bash
cd frontend
cp .env.example .env.local   # NEXT_PUBLIC_API_BASE_URL=http://localhost:5000/api
npm install
npm run dev
```

The app runs on `http://localhost:3000`.

## 9. Running Tests

### Backend
```bash
cd backend
dotnet test
```
Runs all three test projects together:
- **UnitTests** — business rules (deadline enforcement, duplicate-submission prevention, marks
  validation, resource ownership) and JWT/password-hashing correctness, using Moq +
  MockQueryable.Moq (no database required).
- **ArchitectureTests** — NetArchTest rules asserting Domain/Application never depend on
  Infrastructure/Api, Application never references ASP.NET Core, and controllers never bypass
  the Application layer to touch the DbContext directly.
- **IntegrationTests** — full HTTP round trips through the real ASP.NET Core pipeline (auth,
  role + resource-based authorization, submission workflow) against a SQLite in-memory database
  seeded via the same `DatabaseSeeder` used in production.

### Frontend
```bash
cd frontend
npm run lint        # includes the ESLint architecture-boundary rules
npx tsc --noEmit
npm test
```

## 10. Design Decisions & Assumptions

- **Submission format:** text/Markdown only — no file/attachment upload, to keep the evaluation
  environment dependency-free (see Known Limitations).
- **No public self-registration:** per the role model, only an Admin creates Teacher/Student
  accounts. The very first Admin account exists solely via the startup seed routine.
- **Deadlines are UTC everywhere on the server**; the frontend (`shared/lib/date.ts`) converts to
  the viewer's local time only for display — the authoritative deadline check never happens
  client-side.
- **`education-theme` (in `DocsAndPlan/`) is a visual-language reference only** — its color
  palette and login-page layout informed the Tailwind theme and login screen. It has no
  dashboard/table/grading templates, so every data screen in this app is custom-built.
- **No AutoMapper** — entity↔DTO mapping is explicit static extension methods
  (`<Entity>MappingExtensions.ToDto(...)`) per bounded context. This avoids a runtime-reflection
  mapper, its commercial licensing terms for larger organizations in recent versions, and keeps
  every mapping plain and debuggable.
- **PostgreSQL concurrency** on `Assignment` uses the native `xmin` system column (mapped as a
  shadow row-version property) rather than a manually maintained column — the idiomatic Postgres
  equivalent of SQL Server's `ROWVERSION`.
- **Auth cookies are not httpOnly.** A true httpOnly refresh-token cookie would require proxying
  every API call through a Next.js Route Handler (a "BFF" pattern). Given this project's scope,
  the frontend accepts the same XSS-exposure tradeoff most SPAs already accept with
  localStorage-held JWTs, mitigated by a short-lived (15 min) access token and refresh-token
  rotation on every use. This is documented here rather than silently under-delivering on a
  "httpOnly" claim.
- **Caching (`IMemoryCache`)** for read-heavy Classes/Subjects data was scoped out as a
  deliberate simplification — it's explicitly listed as an *optional* extra in the project brief,
  and the dataset sizes here don't make it load-bearing. Everything else in the brief's
  "Advanced Enterprise Features" list (SignalR, soft deletes, CI/CD, rate limiting, audit trail,
  health checks, correlation IDs) is implemented.

## 11. Known Limitations

- Submissions are text/Markdown only; no file/attachment upload.
- No email/SMS delivery — notifications are in-app (SignalR) only.
- No self-service password reset flow (an Admin resets access via user management).
- Single-tenant design (one school/college instance per deployment).
- The frontend's auth cookies are not httpOnly (see Design Decisions above for the tradeoff and rationale).
