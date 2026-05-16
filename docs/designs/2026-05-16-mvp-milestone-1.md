# MVP Milestone 1 Design Document

## Status

Dynamic design document. This document captures what we are building in this milestone, why it matters, and the agreed product and architectural constraints. It is intended to evolve through dialogue with the agent. Clarifying questions should be asked one at a time before changing the design in meaningful ways.

## How this document is used

- The design is created and refined in dialogue with the agent.
- The agent should ask clarifying questions one by one when scope or behavior is ambiguous.
- The output of that dialogue is a structured document, not a vague brainstorm.
- This document should stay focused on the current milestone rather than the whole future platform.

## Stage name

`mvp-milestone-1`

## Goal of this stage

Build the first usable vertical slice of the AI-powered issue tracker:

1. a user can register and log in;
2. a user can create a project;
3. a user can create an issue quickly with minimal input;
4. a user can open that issue and request AI triage;
5. the system returns a suggested priority, labels, and acceptance criteria draft;
6. the user can review, edit, accept, or discard the suggestion;
7. the user can assign the issue and move it into `todo`;
8. the issue remains searchable and visible in the project issue list.

This milestone exists to prove the core product thesis: AI can reduce backlog triage effort without taking control away from the user.

## Why this stage exists

Without AI triage, the product is just another small issue tracker. Without the tracker workflow, the AI suggestion has nowhere useful to live. This milestone intentionally combines both so the demo proves the differentiator in a realistic workflow.

The milestone should optimize for:

- fast issue intake;
- explicit human review of AI output;
- transparent and debuggable system behavior;
- minimal architecture that still keeps domain logic and AI logic separate.

## Scope of this stage

### Included

- user registration and login with email and password;
- JWT-based authenticated API access;
- project creation and listing;
- project labels as the available label source for triage;
- issue creation with domain defaults;
- issue list and issue details;
- AI triage suggestion for an issue;
- manual acceptance or editing of suggested triage fields;
- issue assignment;
- issue status transition to prepare the issue for work;
- basic filters and text search for issue lists.

### Explicitly not included unless time remains

- refresh tokens;
- separate AI microservice;
- generic workflow engine;
- comment edge-case depth beyond the basic model;
- advanced audit history;
- multiple LLM providers;
- confidence scoring;
- automatic application of AI results;
- complex project membership or permissions.

## User scenarios

### 1. Fast issue intake

The user opens a project, clicks "New issue", enters a title, optionally enters a description, and saves.

Expected result:

- issue is created in `backlog`;
- priority defaults to `medium`;
- assignee is empty;
- reporter is the current user;
- creation flow is fast and does not require triage fields upfront.

### 2. AI triage suggestion

The user opens a newly created issue and clicks "AI suggest".

The system sends the issue context and project label set to the AI triage agent and returns:

- suggested `priority`;
- suggested `labels` from the existing project labels only;
- suggested `acceptance_criteria` draft.

Expected result:

- the suggestion is shown as a proposal;
- no issue data is silently changed;
- the user can inspect each suggested field before applying.

### 3. Accepting the suggestion

The user accepts the full suggestion or edits parts of it before saving.

Expected result:

- the issue is updated only through explicit user action;
- accepted AI-generated acceptance criteria is marked as AI-suggested;
- the user remains free to overwrite any field manually.

### 4. Manual triage and preparation for work

The user assigns the issue and moves it from `backlog` to `todo`.

Expected result:

- assignee becomes a valid existing user;
- status changes are reflected correctly;
- `closed_at` remains empty unless status becomes `done`.

### 5. Finding the issue again

The user returns to the issue list and filters or searches for the issue.

Expected result:

- list supports filtering by status, assignee, and labels;
- text search covers title and description;
- `done` issues are hidden by default unless requested.

## Business rules

### Domain invariants

- Issue is created with:
  - `status = backlog`
  - `priority = medium`
  - `assignee = null`
- `reporter` is the current user at creation time and does not change later.
- `assignee` may be any existing user in the system.
- `closed_at` is set automatically when status becomes `done`.
- `closed_at` is cleared automatically when status leaves `done`.
- `Label.name` must be unique within a project, case-insensitively.
- AI-generated acceptance criteria must be marked with an AI-origin flag.

### AI triage rules

- AI suggests values; it does not apply them automatically.
- AI may only suggest labels that already exist in the project.
- If the project has no labels, AI must return an empty `labels` array.
- Placeholder values such as `none`, `null`, and `n/a` are not valid labels unless they actually exist in the project's label set.
- If the model returns invalid labels, they must be rejected during validation.
- If the model returns invalid priority, the response must not be trusted as-is.
- Acceptance criteria is a draft for user review, not a source of truth.
- For MVP, the triage integration uses an OpenAI-compatible chat completions API.
- Local development targets LM Studio by default.
- Provider endpoint and model must be supplied via configuration rather than hardcoded in application logic.
- The prompt sent to the model must explicitly instruct it to use only existing project labels and to return `[]` when no labels are available.

### Auth rules

- Authentication is based on email and password.
- No OAuth.
- MVP uses JWT access token only.
- Token expiration may require re-login instead of refresh flow.

## Edge cases

### AI did not answer

If the LLM call fails, times out, or returns unusable output:

- the issue page must still function normally;
- the user should see a clear failure message;
- no partial mutation should be applied to the issue;
- the user can retry later.

### AI returned invalid labels

If the model suggests labels outside the project's label set:

- those labels must not be applied automatically;
- the suggestion should be sanitized or rejected;
- the API response should remain predictable and explicit.
- if the project label set is empty, any non-empty AI label suggestion is invalid;
- placeholder values are not valid labels unless a project actually contains such a label.

### AI returned weak or empty acceptance criteria

If the criteria are empty, nonsensical, or obviously low quality:

- the user may still keep priority or labels from the suggestion;
- acceptance criteria should remain editable or omitted;
- the system should not pretend the suggestion is valid.

### Issue without description

AI triage must still work when only title exists. This is a core scenario, not an exception.

### Status rollback

Moving from `in-review` back to `in-progress` or from `done` back to another state is allowed. `closed_at` must follow the current state rather than historical assumptions.

### Label uniqueness

Because SQLite is used for MVP, case-insensitive label uniqueness should be enforced by normalized storage or a normalized indexed field, not by wishful assumptions about collation behavior.

## Naming decisions

### Recommended backend naming

- Solution projects:
  - `IssueTracker.Domain`
  - `IssueTracker.Application`
  - `IssueTracker.Infrastructure`
  - `IssueTracker.API`
- AI port: `ITriageAgent`
- Use case names:
  - `RegisterUser`
  - `LoginUser`
  - `CreateProject`
  - `CreateIssue`
  - `GetIssueDetails`
  - `ListProjectIssues`
  - `SuggestIssueTriage`
  - `ApplyIssueTriage`
  - `AssignIssue`
  - `TransitionIssueStatus`

### Recommended frontend naming

- pages:
  - `LoginPage`
  - `ProjectsPage`
  - `ProjectIssuesPage`
  - `IssueDetailsPage`
- components:
  - `IssueForm`
  - `IssueList`
  - `IssueFilters`
  - `AiTriagePanel`

### API naming

Prefer resource routes plus explicit action endpoints:

- `POST /auth/register`
- `POST /auth/login`
- `GET /auth/me`
- `POST /projects`
- `GET /projects`
- `POST /projects/{projectSlug}/issues`
- `GET /projects/{projectSlug}/issues`
- `GET /issues/{id}`
- `POST /issues/{id}/ai-suggest`
- `POST /issues/{id}/apply-triage-suggestion`
- `POST /issues/{id}/assign`
- `POST /issues/{id}/transition`

## Key architectural decisions

### 1. Full Clean Architecture

Backend uses four projects: Domain, Application, Infrastructure, API.

Reason:

- preserves clear boundaries;
- keeps AI integration out of controllers and domain entities;
- allows testing business logic without database or HTTP concerns.

### 2. EF Core with SQLite for MVP

Reason:

- low setup cost;
- sufficient for MVP and demo;
- keeps local development friction low.

Tradeoff:

- not the strongest production storage choice for a collaborative issue tracker, but acceptable at this stage.

### 3. Single AI boundary

Use one application-facing interface: `ITriageAgent`.

Reason:

- enough isolation for current scope;
- avoids building an AI framework for one feature;
- keeps prompting and provider-specific logic inside Infrastructure.

### 4. Handwritten DTOs

Reason:

- fastest for MVP;
- avoids early tooling overhead.

Tradeoff:

- backend/frontend contracts can drift if not kept disciplined.

### 5. REST plus action endpoints

Reason:

- normal CRUD fits resources;
- assignment, transition, and AI suggest are explicit behaviors, not generic updates.

## What “accept suggestion” means

For this milestone, “accept suggestion” means the user explicitly confirms which suggested values should become issue data.

This may happen in one of two UX forms:

- accept all suggested fields at once;
- edit any suggested field first, then save the final chosen values.

Important constraints:

- acceptance is always a user action;
- raw AI output is never treated as already-applied state;
- partial acceptance is valid if supported by the UI;
- final persisted values are the user-reviewed values, not necessarily the model's raw output.

## What happens if the LLM does not answer

The product behavior should be:

- show a clear error state in the UI;
- preserve current issue data untouched;
- allow retry;
- avoid inventing fallback values silently;
- keep manual triage fully available.

For MVP, a simple synchronous failure path is enough. Background retries and queue-based recovery are not required.

## Open questions for future iterations

- Should accepted AI suggestions store a small rationale for debugging?
- Should acceptance be field-by-field with granular provenance, or just a single AI-suggested marker on acceptance criteria?
- Should comments be part of milestone 1 or deferred until the main triage flow is stable?
- Should PR URL be introduced as a dedicated issue field in a later milestone?

## Definition of done for this stage

This stage is complete when the following scenario works end-to-end:

1. user logs in;
2. user creates a project;
3. user creates an issue with only title;
4. user opens the issue and requests AI triage;
5. user receives a suggestion for priority, labels, and acceptance criteria;
6. user edits or accepts the suggestion;
7. user assigns the issue;
8. user moves it to `todo`;
9. user can find it again through the issue list and filters.
