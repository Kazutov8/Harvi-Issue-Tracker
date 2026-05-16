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

## Phase 3 - Projects vertical slice

- [] Add `Project` entity in `src/IssueTracker.Domain/Entities`
- [] Add project repository abstraction in `src/IssueTracker.Application/Abstractions`
- [] Add `CreateProject` and `ListProjects` use cases in `src/IssueTracker.Application/Projects`
- [] Add EF Core project mapping and repository implementation in `src/IssueTracker.Infrastructure/Persistence`
- [] Add project endpoints in `src/IssueTracker.API/Controllers`
- [] Add project contracts in `src/IssueTracker.API/Contracts/Projects`
- [] Add projects list/create UI in `frontend/src/pages` and `frontend/src/projects`

### Files to touch

- `src/IssueTracker.Domain/Entities/Project.cs`
- `src/IssueTracker.Application/Abstractions/IProjectRepository.cs`
- `src/IssueTracker.Application/Projects/*`
- `src/IssueTracker.Infrastructure/Persistence/*`
- `src/IssueTracker.API/Controllers/ProjectsController.cs`
- `src/IssueTracker.API/Contracts/Projects/*`
- `frontend/src/projects/*`
- `frontend/src/pages/*`

## Phase 4 - Labels and issue foundation

- [] Add `Label` entity and normalized-name rule in `src/IssueTracker.Domain/Entities`
- [] Add `Issue` entity, enums, and invariants in `src/IssueTracker.Domain`
- [] Add issue and label repository abstractions in `src/IssueTracker.Application/Abstractions`
- [] Add `CreateIssue`, `GetIssueDetails`, and `ListProjectIssues` use cases in `src/IssueTracker.Application/Issues`
- [] Add label management support needed for project triage context in `src/IssueTracker.Application/Labels`
- [] Add EF Core mappings for issues, labels, and join table in `src/IssueTracker.Infrastructure/Persistence`
- [] Create migration for users, projects, labels, issues, and issue-labels
- [] Add issue endpoints in `src/IssueTracker.API/Controllers`
- [] Add issue and label contracts in `src/IssueTracker.API/Contracts`
- [] Add frontend issue creation form, issue list, and issue details page

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

## Phase 5 - AI triage vertical slice

- [] Add `ITriageAgent` abstraction in `src/IssueTracker.Application/Abstractions`
- [] Add triage suggestion DTOs in `src/IssueTracker.Application/Issues` or dedicated triage folder
- [] Implement `SuggestIssueTriage` use case with validation of AI output
- [] Implement concrete LLM-backed triage agent in `src/IssueTracker.Infrastructure/AI`
- [] Add AI-related configuration in API/Infrastructure settings
- [] Add `POST /issues/{id}/ai-suggest` endpoint in `src/IssueTracker.API/Controllers`
- [] Add AI suggestion contracts in `src/IssueTracker.API/Contracts/Issues`
- [] Add frontend AI triage panel on issue details page
- [] Add UI states for loading, success, invalid suggestion, and provider failure

### Files to touch

- `src/IssueTracker.Application/Abstractions/ITriageAgent.cs`
- `src/IssueTracker.Application/Issues/*`
- `src/IssueTracker.Infrastructure/AI/*`
- `src/IssueTracker.API/Controllers/IssuesController.cs`
- `src/IssueTracker.API/Contracts/Issues/*`
- `src/IssueTracker.API/appsettings*.json`
- `frontend/src/issues/*`
- `frontend/src/pages/IssueDetailsPage.*`

## Phase 6 - Apply triage, assign, and transition

- [] Add `ApplyIssueTriage` use case in `src/IssueTracker.Application/Issues`
- [] Add `AssignIssue` use case in `src/IssueTracker.Application/Issues`
- [] Add `TransitionIssueStatus` use case with `closed_at` logic in `src/IssueTracker.Application/Issues`
- [] Add validation that assignee exists and status values are valid
- [] Add `POST /issues/{id}/apply-triage-suggestion` endpoint
- [] Add `POST /issues/{id}/assign` endpoint
- [] Add `POST /issues/{id}/transition` endpoint
- [] Add frontend controls for apply suggestion, manual edits, assignee selection, and status transition

### Files to touch

- `src/IssueTracker.Application/Issues/*`
- `src/IssueTracker.Infrastructure/Persistence/*`
- `src/IssueTracker.API/Controllers/IssuesController.cs`
- `src/IssueTracker.API/Contracts/Issues/*`
- `frontend/src/issues/*`
- `frontend/src/pages/IssueDetailsPage.*`

## Phase 7 - Filters and search

- [] Add issue list query model supporting status, assignee, labels, text query, page, and page size
- [] Implement filtering and search persistence queries in `src/IssueTracker.Infrastructure/Persistence`
- [] Hide `done` issues by default unless explicitly requested
- [] Add filter query parameters to `GET /projects/{projectSlug}/issues`
- [] Add frontend filter bar and issue search UI

### Files to touch

- `src/IssueTracker.Application/Issues/*`
- `src/IssueTracker.Infrastructure/Persistence/*`
- `src/IssueTracker.API/Controllers/IssuesController.cs`
- `src/IssueTracker.API/Contracts/Issues/*`
- `frontend/src/issues/*`
- `frontend/src/pages/ProjectIssuesPage.*`

## Phase 8 - Hardening and demo readiness

- [] Add request validation for auth, project, issue, and triage endpoints
- [] Add consistent API error responses
- [] Add basic integration tests for auth, project creation, issue creation, and AI triage path
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
- [] User can create a project
- [] User can create an issue with title only
- [] New issue starts in `backlog` with `medium` priority and no assignee
- [] User can request AI triage for the issue
- [] User receives suggested priority, labels, and acceptance criteria
- [] User can edit or accept the suggestion
- [] User can assign the issue
- [] User can move the issue to `todo`
- [] User can find the issue again using list/search/filter UI

## Notes

- Keep tasks atomic. If a task becomes broad, split it into smaller checklist items before implementation.
- Update status in place using `[]` and `[V]` only.
- Do not mark a task as `[V]` until the change is actually implemented and verified to a reasonable degree.
