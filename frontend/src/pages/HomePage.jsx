import { apiClient } from '../api/client'

async function checkHealth() {
  return apiClient.get('/health')
}

function HomePage() {
  return (
    <main className="page-shell">
      <section className="hero-card">
        <span className="eyebrow">MVP milestone 1</span>
        <h1>AI Issue Tracker</h1>
        <p className="lead">
          Frontend and backend skeletons are in place. The next phases will build auth,
          projects, issues, and AI triage on top of this foundation.
        </p>
        <button type="button" className="secondary-button" onClick={checkHealth}>
          Ping health endpoint
        </button>
      </section>
    </main>
  )
}

export default HomePage
