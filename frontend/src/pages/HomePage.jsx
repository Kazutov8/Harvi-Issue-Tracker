import { useAuth } from '../auth/AuthContext'

function HomePage() {
  const { logout, user } = useAuth()

  return (
    <main className="page-shell">
      <section className="hero-card">
        <span className="eyebrow">MVP milestone 1</span>
        <h1>AI Issue Tracker</h1>
        <p className="lead">
          Authentication is now wired end-to-end. You are signed in and ready for the next
          vertical slices: projects, issues, and AI triage.
        </p>
        <div className="user-summary">
          <strong>{user.displayName}</strong>
          <span>{user.email}</span>
        </div>
        <button type="button" className="secondary-button" onClick={logout}>
          Sign out
        </button>
      </section>
    </main>
  )
}

export default HomePage
