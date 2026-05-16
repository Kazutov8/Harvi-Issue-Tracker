# IssueTracker.API integration tests

Run backend tests locally from the repository root:

```bash
dotnet test IssueTracker.sln
```

This phase uses `WebApplicationFactory` with a per-test-run temporary SQLite database file, so auth integration tests remain isolated from the local development database.
