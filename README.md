# Issue Tracker

## Local startup

### Backend

1. Trust the ASP.NET development certificate if you have not done it yet:
   `dotnet dev-certs https --trust`
2. Start the API from the repository root:
   `dotnet run --project src/IssueTracker.API`

The backend is pinned to these local addresses:

- `https://localhost:7017`
- `http://localhost:5013`

### Frontend

1. Install dependencies:
   `npm install --prefix frontend`
2. Start the Vite dev server:
   `npm run dev --prefix frontend`

In local development, the frontend uses the Vite proxy and forwards API requests to `https://localhost:7017`.

## Optional frontend API override

If you need the frontend to call a different backend address, create `frontend/.env.local` with:

`VITE_API_BASE_URL=https://your-backend-host`
