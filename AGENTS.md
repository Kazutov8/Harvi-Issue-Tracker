# AGENTS.md

## Language and Working Style
- Always write in Russian unless the user explicitly requests another language.
- Write concisely, clearly, and to the point.
- Don't pretend you've verified something if you haven't.
- If there's not enough information to confidently solve a problem, first gather context from the project files rather than guessing.
- Don't overcomplicate architecture unnecessarily. Priority: working MVP and practical value, not academic "perfection."

## Architectural Principles
- Prefer a simple and transparent MVP architecture.
- Separate:
  - domain logic,
  - data access layer,
  - HTTP/API layer,
  - AI integrations.
- The AI part should not be scattered as random calls throughout the codebase.
- All AI calls must have a clear entry point and an isolated layer.
- Keep prompts, response schemas, and tool/workflow logic in a well-structured manner, not as arbitrary strings scattered around.
- Design the system so that it can be debugged without magic.
- When in doubt between "abstraction for the future" and "simple code for the current scenario" → choose the simple implementation.

## Solution Quality Requirements
When working on a task:
- first, understand the current project structure;
- don't invent non-existent files, layers, or dependencies;
- don't perform large-scale refactorings without a direct need;
- don't change the project's style without reason;
- every new entity, endpoint, component, or service must be justified by a specific scenario.

### For Backend (ASP.NET Core)
- preserve domain invariants;
- validate input data;
- don't mix business logic and AI logic;
- make errors understandable and diagnosable.

### For Frontend (ReactJS)
- UI must be functional, clean, and fast.

## Development Preferences
- First, build a minimally working vertical slice. Then extend it.
- For new features, prefer this path:
  - domain model,
  - API/business logic,
  - UI,
  - tests/checks,
  - DX and prompt improvements.
- If a task is ambiguous, first propose 2–3 reasonable options and choose the simplest one that covers the current MVP.

## What the Agent Should NOT Do
- Don't turn an MVP into an enterprise platform.
- Don't add roles, permission matrices, project membership, audit logs, notifications, activity feeds, or complex workflow engines without a specific request.
- Don't implement OAuth unless asked.
- Don't build a "universal AI orchestration framework" for just two features.

## Definition of Done for Tasks
When implementing a feature:
- the user scenario actually works end-to-end;
- domain invariants are preserved;
- UI/API behavior is predictable;
- an AI feature does not perform irreversible actions without user confirmation;
- the solution can be explained in simple terms;
- the code hasn't become significantly more complex than the task requires.

## Decision-Making When Lacking Context
By default:
- choose the solution that favors simplicity;
- maintain transparency and observability;
- make AI features controllable and verifiable;