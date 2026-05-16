import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { issuesApi } from '../issues/issuesApi'

function ProjectIssuesPage() {
  const { projectSlug } = useParams()
  const { accessToken } = useAuth()
  const [issues, setIssues] = useState([])
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)

  useEffect(() => {
    let isActive = true

    async function loadIssues() {
      setIsLoading(true)
      setError('')

      try {
        const response = await issuesApi.list(projectSlug, accessToken)
        if (isActive) {
          setIssues(response)
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

    loadIssues()

    return () => {
      isActive = false
    }
  }, [accessToken, projectSlug])

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setIsSubmitting(true)

    try {
      const createdIssue = await issuesApi.create(projectSlug, { title, description }, accessToken)
      setIssues((current) => [createdIssue, ...current])
      setTitle('')
      setDescription('')
    } catch (submitError) {
      setError(submitError.message)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="page-shell projects-shell">
      <section className="projects-layout">
        <header className="projects-header">
          <div>
            <span className="eyebrow">Issues</span>
            <h1>{projectSlug}</h1>
            <p className="auth-subtitle">Create issues quickly and prepare them for triage.</p>
          </div>

          <Link to="/projects" className="secondary-button nav-link-button">
            Back to projects
          </Link>
        </header>

        <section className="projects-grid">
          <section className="projects-panel">
            <h2>New issue</h2>
            <form className="auth-form" onSubmit={handleSubmit}>
              <label>
                <span>Title</span>
                <input value={title} onChange={(event) => setTitle(event.target.value)} placeholder="Login form breaks on Safari" required />
              </label>

              <label>
                <span>Description</span>
                <textarea
                  value={description}
                  onChange={(event) => setDescription(event.target.value)}
                  placeholder="Optional context for the issue"
                  rows={5}
                />
              </label>

              {error ? <p className="form-error">{error}</p> : null}

              <button type="submit" className="primary-button" disabled={isSubmitting}>
                {isSubmitting ? 'Creating issue...' : 'Create issue'}
              </button>
            </form>
          </section>

          <section className="projects-panel">
            <div className="projects-list-header">
              <h2>Project issues</h2>
              <span className="projects-count">{issues.length}</span>
            </div>

            {isLoading ? <p className="auth-subtitle">Loading issues...</p> : null}

            {!isLoading && issues.length === 0 ? (
              <p className="auth-subtitle">No issues yet. Create the first backlog item for this project.</p>
            ) : null}

            {!isLoading && issues.length > 0 ? (
              <ul className="projects-list">
                {issues.map((issue) => (
                  <li key={issue.id} className="project-card issue-card">
                    <div>
                      <Link to={`/issues/${issue.id}`} className="issue-link">
                        {issue.title}
                      </Link>
                      <p className="project-slug">
                        {issue.status} · {issue.priority}
                      </p>
                    </div>
                  </li>
                ))}
              </ul>
            ) : null}
          </section>
        </section>
      </section>
    </main>
  )
}

export default ProjectIssuesPage
