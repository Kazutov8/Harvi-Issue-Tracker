# MVP Milestone 1 Implementation Plan

## Status convention

- Use `[]` for a pending task.
- Use `[V]` for a completed task.
- When a task is completed, update the same line from `[]` to `[V]`.

## Stage name

`mvp-milestone-1`

## Goal

Implement the first working vertical slice of the AI issue tracker: auth, projects, issue creation, AI triage suggestion, manual application of triage, and basic issue discovery.

## Phase 1 - Solution skeleton

- [V] Create backend solution and four projects: `src/IssueTracker.Domain`, `src/IssueTracker.Application`, `src/IssueTracker.Infrastructure`, `src/IssueTracker.API`
- [V] Add project references to enforce architecture boundaries
- [V] Create frontend app in `frontend/`
- [V] Configure backend startup, dependency injection, and base configuration files
- [V] Configure SQLite connection and EF Core bootstrap in Infrastructure/API
- [V] Add initial health endpoint in API
- [V] Add base frontend API client module

### Files to touch

- `src/IssueTracker.Domain/*`
- `src/IssueTracker.Application/*`
- `src/IssueTracker.Infrastructure/*`
- `src/IssueTracker.API/*`
- `frontend/*`

## Phase 2 - Authentication vertical slice

- [V] Add `User` entity and related domain rules in `src/IssueTracker.Domain`
- [V] Add auth use cases in `src/IssueTracker.Application/Auth`
- [V] Add password hashing service in `src/IssueTracker.Infrastructure/Auth`
- [V] Add JWT issuing and validation in `src/IssueTracker.Infrastructure/Auth`
- [V] Add auth persistence mapping in `src/IssueTracker.Infrastructure/Persistence`
- [V] Add auth endpoints in `src/IssueTracker.API/Controllers`
- [V] Add request/response contracts for register, login, and me in `src/IssueTracker.API/Contracts`
- [V] Add frontend auth screens and auth state wiring in `frontend/src/auth` and `frontend/src/pages`
- [V] Add API token attachment in `frontend/src/api`
- [V] Add initial EF Core migration for auth schema
- [V] Verify local SQLite database can be created and used for auth flow

### Files to touch

- `src/IssueTracker.Domain/Entities/User.cs`
- `src/IssueTracker.Application/Auth/*`
- `src/IssueTracker.Infrastructure/Auth/*`
- `src/IssueTracker.Infrastructure/Persistence/*`
- `src/IssueTracker.API/Controllers/AuthController.cs`
- `src/IssueTracker.API/Contracts/Auth/*`
- `frontend/src/auth/*`
- `frontend/src/pages/*`
- `frontend/src/api/*`

## Phase 2.1 - Auth test baseline

- [V] Create backend integration test project in `tests/IssueTracker.API.IntegrationTests`
- [V] Add API test host setup with isolated test database configuration
- [V] Add integration tests for `POST /auth/register`
- [V] Add integration tests for `POST /auth/login`
- [V] Add integration tests for `GET /auth/me`
- [V] Ensure test database is recreated or isolated for each test run
- [V] Add documented command or script convention for running backend tests locally

### Files to touch

- `tests/IssueTracker.API.IntegrationTests/*`
- `IssueTracker.sln`
- `src/IssueTracker.API/*`
- `src/IssueTracker.Infrastructure/Persistence/*`

## Phase 3 - Projects vertical slice

- [V] Add `Project` entity in `src/IssueTracker.Domain/Entities`
- [V] Add project repository abstraction in `src/IssueTracker.Application/Abstractions`
- [V] Add `CreateProject` and `ListProjects` use cases in `src/IssueTracker.Application/Projects`
- [V] Add EF Core project mapping and repository implementation in `src/IssueTracker.Infrastructure/Persistence`
- [V] Add project endpoints in `src/IssueTracker.API/Controllers`
- [V] Add project contracts in `src/IssueTracker.API/Contracts/Projects`
- [V] Add projects list/create UI in `frontend/src/pages` and `frontend/src/projects`
- [V] Add integration tests for project creation and project listing
- [V] Configure local frontend-to-backend dev integration via `https://localhost:7017` with explicit backend CORS allowance for the Vite origin

### Files to touch

- `src/IssueTracker.Domain/Entities/Project.cs`
- `src/IssueTracker.Application/Abstractions/IProjectRepository.cs`
- `src/IssueTracker.Application/Projects/*`
- `src/IssueTracker.Infrastructure/Persistence/*`
- `src/IssueTracker.API/Controllers/ProjectsController.cs`
- `src/IssueTracker.API/Contracts/Projects/*`
- `frontend/src/projects/*`
- `frontend/src/pages/*`
- `tests/IssueTracker.API.IntegrationTests/*`
- `frontend/src/api/client.js`
- `frontend/.env.local`
- `src/IssueTracker.API/Program.cs`

## Phase 4 - Labels and issue foundation

- Execution order for current codebase:
  1. Extend the domain model first: add `Label`, `Issue`, and issue status/priority enums while preserving the existing static-factory style used by `User` and `Project`.
  2. Introduce application contracts and use cases next: repository abstractions, issue DTOs, and label-query support needed by the issue read/write flows.
  3. Expand persistence in one pass: update `IssueTrackerDbContext`, add EF configurations and repositories, then create a migration that folds labels and issues into the existing SQLite schema.
  4. Add the API slice after the application layer is stable: contracts and an `IssuesController` for create, details, and list-by-project.
  5. Add the frontend vertical slice last: project issue list page, issue creation form, and issue details page wired to the current auth/token flow and router.
  6. Cover the slice with tests in two layers: pure domain/application tests for invariants and integration tests for the new HTTP endpoints.

- Scope notes for implementation:
  - Do not build full label CRUD in this phase. Only add the minimum label support required so projects have an available label set for upcoming AI triage work.
  - The current frontend has no project-details route yet, so Phase 4 should explicitly introduce navigation from the projects list into a per-project issues page rather than trying to overload `ProjectsPage`.
  - `IApplicationDbContext` is intentionally minimal today. Unless a concrete use case truly needs direct set access, prefer repositories over widening that abstraction prematurely.
  - The current API returns mostly simple controller-level validation responses. Keep Phase 4 consistent with that style instead of introducing a broader validation/error framework before Phase 8.

- [V] Add `Label` entity and normalized-name rule in `src/IssueTracker.Domain/Entities`
- [V] Add `Issue` entity, enums, and invariants in `src/IssueTracker.Domain`
- [V] Add issue and label repository abstractions in `src/IssueTracker.Application/Abstractions`
- [V] Add `CreateIssue`, `GetIssueDetails`, and `ListProjectIssues` use cases in `src/IssueTracker.Application/Issues`
- [V] Add label management support needed for project triage context in `src/IssueTracker.Application/Labels`
- [V] Add EF Core mappings for issues, labels, and join table in `src/IssueTracker.Infrastructure/Persistence`
- [V] Create migration for users, projects, labels, issues, and issue-labels
- [V] Add issue endpoints in `src/IssueTracker.API/Controllers`
- [V] Add issue and label contracts in `src/IssueTracker.API/Contracts`
- [V] Add frontend issue creation form, issue list, and issue details page
- [V] Add domain or application tests for issue defaults and label normalization rules where pure logic can be tested without HTTP
- [V] Add integration tests for create issue, get issue details, and list project issues

### Files to touch

- `src/IssueTracker.Domain/Entities/Label.cs`
- `src/IssueTracker.Domain/Entities/Issue.cs`
- `src/IssueTracker.Domain/Enums/*`
- `src/IssueTracker.Application/Abstractions/IIssueRepository.cs`
- `src/IssueTracker.Application/Abstractions/ILabelRepository.cs`
- `src/IssueTracker.Application/Issues/*`
- `src/IssueTracker.Application/Labels/*`
- `src/IssueTracker.Infrastructure/Persistence/*`
- `src/IssueTracker.API/Controllers/IssuesController.cs`
- `src/IssueTracker.API/Contracts/Issues/*`
- `src/IssueTracker.API/Contracts/Labels/*`
- `frontend/src/issues/*`
- `frontend/src/pages/*`
- `tests/IssueTracker.API.IntegrationTests/*`
- `tests/*`

## Phase 5 - AI triage vertical slice

- [V] Add `ITriageAgent` abstraction in `src/IssueTracker.Application/Abstractions`
- [V] Add triage suggestion DTOs in `src/IssueTracker.Application/Issues` or dedicated triage folder
- [V] Implement `SuggestIssueTriage` use case with validation of AI output
- [V] Implement concrete LLM-backed triage agent in `src/IssueTracker.Infrastructure/AI` using an OpenAI-compatible chat completions API
- [V] Add AI-related configuration in API/Infrastructure settings for local LM Studio integration (`BaseUrl`, `Model`, `Timeout`)
- [V] Add `POST /issues/{id}/ai-suggest` endpoint in `src/IssueTracker.API/Controllers`
- [V] Add AI suggestion contracts in `src/IssueTracker.API/Contracts/Issues`
- [V] Add frontend AI triage panel on issue details page
- [V] Add UI states for loading, success, invalid suggestion, and provider failure
- [V] Add application tests for AI suggestion validation and invalid label or priority handling
- [V] Add integration tests for successful AI triage suggestion and provider failure path

### Files to touch

- `src/IssueTracker.Application/Abstractions/ITriageAgent.cs`
- `src/IssueTracker.Application/Issues/*`
- `src/IssueTracker.Infrastructure/AI/*`
- `src/IssueTracker.API/Controllers/IssuesController.cs`
- `src/IssueTracker.API/Contracts/Issues/*`
- `src/IssueTracker.API/appsettings*.json`
- `frontend/src/issues/*`
- `frontend/src/pages/IssueDetailsPage.*`
- `tests/IssueTracker.API.IntegrationTests/*`
- `tests/*`

## Phase 6 - Apply triage, assign, and transition

- [V] Add `ApplyIssueTriage` use case in `src/IssueTracker.Application/Issues`
- [V] Add `AssignIssue` use case in `src/IssueTracker.Application/Issues`
- [V] Add `TransitionIssueStatus` use case with `closed_at` logic in `src/IssueTracker.Application/Issues`
- [V] Add validation that assignee exists and status values are valid
- [V] Add `POST /issues/{id}/apply-triage-suggestion` endpoint
- [V] Add `POST /issues/{id}/assign` endpoint
- [V] Add `POST /issues/{id}/transition` endpoint
- [V] Add frontend controls for apply suggestion, manual edits, assignee selection, and status transition
- [V] Add application tests for triage application rules, assignee validation, and `closed_at` transitions
- [V] Add integration tests for apply triage suggestion, assign issue, and transition issue status

### Files to touch

- `src/IssueTracker.Application/Issues/*`
- `src/IssueTracker.Infrastructure/Persistence/*`
- `src/IssueTracker.API/Controllers/IssuesController.cs`
- `src/IssueTracker.API/Contracts/Issues/*`
- `frontend/src/issues/*`
- `frontend/src/pages/IssueDetailsPage.*`
- `tests/IssueTracker.API.IntegrationTests/*`
- `tests/*`

## Phase 7 - Filters and search

- [] Add issue list query model supporting status, assignee, labels, text query, page, and page size
- [] Implement filtering and search persistence queries in `src/IssueTracker.Infrastructure/Persistence`
- [] Hide `done` issues by default unless explicitly requested
- [] Add filter query parameters to `GET /projects/{projectSlug}/issues`
- [] Add frontend filter bar and issue search UI
- [] Add integration tests for status, assignee, label, and text search filters including hidden-by-default `done` behavior

### Files to touch

- `src/IssueTracker.Application/Issues/*`
- `src/IssueTracker.Infrastructure/Persistence/*`
- `src/IssueTracker.API/Controllers/IssuesController.cs`
- `src/IssueTracker.API/Contracts/Issues/*`
- `frontend/src/issues/*`
- `frontend/src/pages/ProjectIssuesPage.*`
- `tests/IssueTracker.API.IntegrationTests/*`

## Phase 8 - Hardening and demo readiness

- [] Add request validation for auth, project, issue, and triage endpoints
- [] Add consistent API error responses
- [] Extend integration coverage for validation errors, consistent error responses, and critical end-to-end API paths
- [] Add a small set of reference issues for manual AI triage evaluation
- [] Add demo seed data for local setup
- [] Review naming and remove dead code introduced during implementation
- [] Verify the full milestone scenario end-to-end

### Files to touch

- `src/IssueTracker.API/*`
- `src/IssueTracker.Application/*`
- `src/IssueTracker.Infrastructure/*`
- `tests/*`
- `frontend/src/*`

## End-to-end acceptance checklist

- [V] User can register and log in
- [V] User can create a project
- [V] User can create an issue with title only
- [V] New issue starts in `backlog` with `medium` priority and no assignee
- [V] User can request AI triage for the issue
- [V] User receives suggested priority, labels, and acceptance criteria
- [V] User can edit or accept the suggestion
- [V] User can assign the issue
- [V] User can move the issue to `todo`
- [] User can find the issue again using list/search/filter UI

## Notes

- Keep tasks atomic. If a task becomes broad, split it into smaller checklist items before implementation.
- Update status in place using `[]` and `[V]` only.
- Do not mark a task as `[V]` until the change is actually implemented and verified to a reasonable degree.
- For MVP, SQLite is the default local database. Migrations are the source of truth; the `.db` file is a local runtime artifact and should not be relied on as shared project state.
- Before marking any phase as completed, run all existing automated tests relevant to the solution and resolve failures.
- Each new backend vertical slice should add or update automated tests when practical instead of deferring all coverage to the end.
