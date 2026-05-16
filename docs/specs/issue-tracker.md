# Issue Tracker Specification

## Scope
Current module includes:
- JWT-based authentication
- project creation and listing
- issue creation and listing inside a project
- issue details
- project labels listing
- AI triage suggestion for an issue
- applying triage results to an issue
- assigning issue to a user
- manual issue status transition

All endpoints except `POST /auth/register` and `POST /auth/login` require authentication.

## Data Model

### User
Fields:
- `id: guid`
- `email: string`
- `displayName: string`
- `createdAtUtc: datetime`

Persistence also stores:
- `normalizedEmail: string`
- `passwordHash: string`

Behavior:
- email lookup is case-insensitive via uppercase normalization

### Project
Fields:
- `id: guid`
- `name: string`
- `slug: string`
- `createdByUserId: guid`
- `createdAtUtc: datetime`

Behavior:
- slug is generated from project name
- slug must be unique

### Label
Fields:
- `id: guid`
- `projectId: guid`
- `name: string`
- `createdAtUtc: datetime`

Persistence also stores:
- `normalizedName: string`

Behavior:
- labels belong to a single project
- labels are listed sorted by `name`

### Issue
Fields:
- `id: guid`
- `projectId: guid`
- `title: string`
- `description: string?`
- `status: backlog | todo | in-progress | in-review | done`
- `priority: low | medium | high | urgent`
- `reporterUserId: guid`
- `assigneeUserId: guid?`
- `acceptanceCriteria: string?`
- `acceptanceCriteriaIsAiGenerated: bool`
- `createdAtUtc: datetime`
- `closedAtUtc: datetime?`
- `labels: IssueLabel[]`

`IssueLabel` fields:
- `id: guid`
- `name: string`

Default issue state on creation:
- `status = backlog`
- `priority = medium`
- `assigneeUserId = null`
- `closedAtUtc = null`

## Status Model

Allowed status values:
- `backlog`
- `todo`
- `in-progress`
- `in-review`
- `done`

Transition behavior:
- backend accepts direct transition to any status
- no workflow validation is enforced
- entering `done` sets `closedAtUtc` to current UTC time
- leaving `done` clears `closedAtUtc`

## API Endpoints

## Auth

### `POST /auth/register`
Request:
```json
{
  "email": "user@example.com",
  "password": "string",
  "displayName": "string"
}
```

Response:
```json
{
  "accessToken": "jwt",
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "displayName": "string",
    "createdAtUtc": "datetime"
  }
}
```

Behavior:
- returns `409 Conflict` if email already exists

### `POST /auth/login`
Request:
```json
{
  "email": "user@example.com",
  "password": "string"
}
```

Response: same shape as register

Behavior:
- returns `401 Unauthorized` for invalid credentials

### `GET /auth/me`
Response:
```json
{
  "id": "guid",
  "email": "user@example.com",
  "displayName": "string",
  "createdAtUtc": "datetime"
}
```

### `GET /auth/users`
Response:
```json
[
  {
    "id": "guid",
    "email": "user@example.com",
    "displayName": "string",
    "createdAtUtc": "datetime"
  }
]
```

Behavior:
- users are sorted by `displayName`, then `email`

## Projects

### `POST /projects`
Request:
```json
{
  "name": "string"
}
```

Response:
```json
{
  "id": "guid",
  "name": "string",
  "slug": "string",
  "createdByUserId": "guid",
  "createdAtUtc": "datetime"
}
```

Behavior:
- creator is taken from current authenticated user
- slug is generated from `name`
- returns `409 Conflict` if generated slug already exists
- returns conflict-level error if name cannot produce a slug

### `GET /projects`
Response:
```json
[
  {
    "id": "guid",
    "name": "string",
    "slug": "string",
    "createdByUserId": "guid",
    "createdAtUtc": "datetime"
  }
]
```

Behavior:
- projects are sorted by `createdAtUtc`

## Issues

### `POST /projects/{projectSlug}/issues`
Request:
```json
{
  "title": "string",
  "description": "string?"
}
```

Response:
```json
{
  "id": "guid",
  "projectId": "guid",
  "title": "string",
  "description": "string?",
  "status": "backlog",
  "priority": "medium",
  "reporterUserId": "guid",
  "assigneeUserId": null,
  "acceptanceCriteria": null,
  "acceptanceCriteriaIsAiGenerated": false,
  "createdAtUtc": "datetime",
  "closedAtUtc": null,
  "labels": []
}
```

Behavior:
- reporter is taken from current authenticated user
- returns `404 Not Found` if project does not exist

### `GET /projects/{projectSlug}/issues`
Query parameters:
- `status`
- `assigneeUserId`
- `labelIds` (repeatable)
- `query`
- `page`
- `pageSize`
- `includeDone`

Response:
```json
[
  {
    "id": "guid",
    "projectId": "guid",
    "title": "string",
    "description": "string?",
    "status": "backlog | todo | in-progress | in-review | done",
    "priority": "low | medium | high | urgent",
    "reporterUserId": "guid",
    "assigneeUserId": "guid?",
    "acceptanceCriteria": "string?",
    "acceptanceCriteriaIsAiGenerated": true,
    "createdAtUtc": "datetime",
    "closedAtUtc": "datetime?",
    "labels": [
      { "id": "guid", "name": "string" }
    ]
  }
]
```

Behavior:
- if `status` is provided, issues are filtered by exact status
- if `status` is not provided and `includeDone` is `false`, `done` issues are excluded
- if `assigneeUserId` is provided, issues are filtered by assignee
- if `labelIds` are provided, issue matches if it has at least one of the labels
- if `query` is provided, search runs against `title` and `description`
- results are sorted by `createdAtUtc desc`
- backend normalizes invalid `page < 1` to `1`
- backend normalizes invalid `pageSize < 1` to `20`
- default query behavior is equivalent to:
  - `page = 1`
  - `pageSize = 20`
  - `includeDone = false`
- returns validation error if `status` is invalid
- returns `404 Not Found` if project does not exist

### `GET /projects/{projectSlug}/labels`
Response:
```json
[
  {
    "id": "guid",
    "projectId": "guid",
    "name": "string",
    "createdAtUtc": "datetime"
  }
]
```

Behavior:
- returns labels for the project only
- labels are sorted by `name`
- returns `404 Not Found` if project does not exist

### `GET /issues/{id}`
Response: same issue shape as issue list item

Behavior:
- returns `404 Not Found` if issue does not exist

### `POST /issues/{id}/ai-suggest`
Request body: none

Response on valid suggestion:
```json
{
  "issueId": "guid",
  "priority": "low | medium | high | urgent",
  "labels": [
    { "id": "guid", "name": "string" }
  ],
  "acceptanceCriteria": "string?",
  "isValid": true,
  "validationError": null
}
```

Response on invalid AI suggestion:
```json
{
  "issueId": "guid",
  "priority": "medium",
  "labels": [],
  "acceptanceCriteria": null,
  "isValid": false,
  "validationError": "string"
}
```

Behavior:
- AI receives issue title, description, and available project label names
- AI is instructed to return JSON only
- AI is instructed to use only existing project labels
- if project has no labels, AI is instructed to return `labels: []`
- AI is instructed to write `acceptanceCriteria` in Russian
- unknown AI label makes suggestion invalid
- invalid AI priority makes suggestion invalid
- empty or whitespace acceptance criteria becomes `null`
- returns `404 Not Found` if issue does not exist
- returns `422 Unprocessable Entity` when AI response is structurally accepted but semantically invalid for the project
- returns `502 Bad Gateway` when provider call fails or returns unusable output

### `POST /issues/{id}/apply-triage-suggestion`
Request:
```json
{
  "priority": "low | medium | high | urgent",
  "labelIds": ["guid"],
  "acceptanceCriteria": "string?"
}
```

Response: same issue shape as issue list item

Behavior:
- replaces issue priority
- replaces issue label set completely
- trims acceptance criteria
- empty or whitespace acceptance criteria becomes `null`
- sets `acceptanceCriteriaIsAiGenerated = true` when acceptance criteria is not null
- sets `acceptanceCriteriaIsAiGenerated = false` when acceptance criteria is null
- all label ids must belong to the same issue project
- returns validation error if `priority` is invalid
- returns `404 Not Found` if issue does not exist or any label is invalid for that project

### `POST /issues/{id}/assign`
Request:
```json
{
  "assigneeUserId": "guid"
}
```

Response: same issue shape as issue list item

Behavior:
- assignee must be an existing user
- returns validation error if `assigneeUserId` is empty
- returns `404 Not Found` if issue does not exist or assignee does not exist

### `POST /issues/{id}/transition`
Request:
```json
{
  "status": "backlog | todo | in-progress | in-review | done"
}
```

Response: same issue shape as issue list item

Behavior:
- no transition rules are enforced beyond enum parsing
- returns validation error if `status` is invalid
- returns `404 Not Found` if issue does not exist

## Key Behaviors

- JWT token is returned immediately after register and login.
- current user is identified from `ClaimTypes.NameIdentifier`.
- all issue list and detail responses include labels inline.
- issue label assignment is many-to-many.
- applying triage deduplicates labels by label id.
- AI suggestion deduplicates labels by resolved project label id.
- label matching for AI suggestion is case-insensitive through normalized label names.
- `title`, `description`, `name`, and similar text inputs are trimmed before persistence where handled by domain/application code.
- development startup runs EF Core migrations automatically and seeds demo data.

## Demo Seed Data
In development only, startup seeds:
- one demo user
- one demo project: `demo-workspace`
- three labels: `bug`, `ux`, `ai-triage`
- two demo issues

## Not Implemented
The following items are present in design discussions but not implemented in current code:
- comments
- issue update endpoint
- issue delete endpoint
- project details endpoint
- project description
- label create/update/delete endpoints
- issue comments, PR links, and comment permissions
- project membership or role model
