import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { issuesApi } from '../issues/issuesApi'

function IssueDetailsPage() {
  const { issueId } = useParams()
  const navigate = useNavigate()
  const { accessToken } = useAuth()
  const [issue, setIssue] = useState(null)
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    let isActive = true

    async function loadIssue() {
      setIsLoading(true)
      setError('')

      try {
        const issueResponse = await issuesApi.getById(issueId, accessToken)

        if (isActive) {
          setIssue(issueResponse)
        }
      } catch (loadError) {
        if (isActive) {
          setError(loadError.message)
        }
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    loadIssue()

    return () => {
      isActive = false
    }
  }, [accessToken, issueId])

  if (error) {
    return (
      <main className="page-shell projects-shell">
        <section className="projects-layout">
          <p className="form-error">{error}</p>
          <button type="button" className="secondary-button" onClick={() => navigate('/projects')}>
            Back to projects
          </button>
        </section>
      </main>
    )
  }

  return (
    <main className="page-shell projects-shell">
      <section className="projects-layout">
        {isLoading || !issue ? (
          <p className="auth-subtitle">Loading issue...</p>
        ) : (
          <section className="projects-panel issue-details-panel">
            <div className="issue-details-header">
              <div>
                <span className="eyebrow">Issue details</span>
                <h1>{issue.title}</h1>
              </div>
              <Link to="/projects" className="secondary-button nav-link-button">
                Back to projects
              </Link>
            </div>

            <div className="issue-meta-grid">
              <div>
                <span className="issue-meta-label">Status</span>
                <strong>{issue.status}</strong>
              </div>
              <div>
                <span className="issue-meta-label">Priority</span>
                <strong>{issue.priority}</strong>
              </div>
              <div>
                <span className="issue-meta-label">Reporter</span>
                <strong>{issue.reporterUserId}</strong>
              </div>
              <div>
                <span className="issue-meta-label">Assignee</span>
                <strong>{issue.assigneeUserId ?? 'Unassigned'}</strong>
              </div>
            </div>

            <section className="issue-section">
              <h2>Description</h2>
              <p className="auth-subtitle">{issue.description || 'No description provided.'}</p>
            </section>

            <section className="issue-section">
              <h2>Labels</h2>
              {issue.labels.length === 0 ? (
                <p className="auth-subtitle">No labels assigned yet.</p>
              ) : (
                <ul className="label-list">
                  {issue.labels.map((label) => (
                    <li key={label.id} className="label-chip">
                      {label.name}
                    </li>
                  ))}
                </ul>
              )}
            </section>
          </section>
        )}
      </section>
    </main>
  )
}

export default IssueDetailsPage
