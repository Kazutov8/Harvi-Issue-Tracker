import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { issuesApi } from '../issues/issuesApi'

const STATUS_OPTIONS = [
  { value: '', label: 'All open statuses' },
  { value: 'backlog', label: 'backlog' },
  { value: 'todo', label: 'todo' },
  { value: 'in-progress', label: 'in-progress' },
  { value: 'in-review', label: 'in-review' },
  { value: 'done', label: 'done' },
]

function ProjectIssuesPage() {
  const { projectSlug } = useParams()
  const { accessToken } = useAuth()
  const [issues, setIssues] = useState([])
  const [availableLabels, setAvailableLabels] = useState([])
  const [availableUsers, setAvailableUsers] = useState([])
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [filters, setFilters] = useState({
    status: '',
    assigneeUserId: '',
    query: '',
    labelIds: [],
    includeDone: false,
  })

  useEffect(() => {
    let isActive = true

    async function loadIssues() {
      setIsLoading(true)
      setError('')

      try {
        const [issuesResponse, labelsResponse, usersResponse] = await Promise.all([
          issuesApi.list(projectSlug, accessToken, filters),
          issuesApi.listLabels(projectSlug, accessToken),
          issuesApi.listUsers(accessToken),
        ])

        if (isActive) {
          setIssues(issuesResponse)
          setAvailableLabels(labelsResponse)
          setAvailableUsers(usersResponse)
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
  }, [accessToken, filters, projectSlug])

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setIsSubmitting(true)

    try {
      await issuesApi.create(projectSlug, { title, description }, accessToken)
      const refreshedIssues = await issuesApi.list(projectSlug, accessToken, filters)
      setIssues(refreshedIssues)
      setTitle('')
      setDescription('')
    } catch (submitError) {
      setError(submitError.message)
    } finally {
      setIsSubmitting(false)
    }
  }

  function handleFilterChange(name, value) {
    setFilters((current) => ({ ...current, [name]: value }))
  }

  function handleLabelToggle(labelId) {
    setFilters((current) => ({
      ...current,
      labelIds: current.labelIds.includes(labelId)
        ? current.labelIds.filter((currentLabelId) => currentLabelId !== labelId)
        : [...current.labelIds, labelId],
    }))
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

            <section className="issue-section">
              <h3>Filters</h3>
              <div className="triage-actions-grid">
                <label>
                  <span>Search</span>
                  <input
                    value={filters.query}
                    onChange={(event) => handleFilterChange('query', event.target.value)}
                    placeholder="Search title or description"
                  />
                </label>

                <label>
                  <span>Status</span>
                  <select value={filters.status} onChange={(event) => handleFilterChange('status', event.target.value)}>
                    {STATUS_OPTIONS.map((option) => (
                      <option key={option.value || 'all'} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                </label>

                <label>
                  <span>Assignee</span>
                  <select
                    value={filters.assigneeUserId}
                    onChange={(event) => handleFilterChange('assigneeUserId', event.target.value)}
                  >
                    <option value="">Anyone</option>
                    {availableUsers.map((user) => (
                      <option key={user.id} value={user.id}>
                        {user.displayName}
                      </option>
                    ))}
                  </select>
                </label>
              </div>

              <label className="checkbox-item">
                <input
                  type="checkbox"
                  checked={filters.includeDone}
                  onChange={(event) => handleFilterChange('includeDone', event.target.checked)}
                />
                <span>Include done issues when no explicit status filter is selected</span>
              </label>

              <div>
                <span className="issue-meta-label">Labels</span>
                {availableLabels.length === 0 ? (
                  <p className="auth-subtitle">No project labels yet.</p>
                ) : (
                  <div className="checkbox-list">
                    {availableLabels.map((label) => (
                      <label key={label.id} className="checkbox-item">
                        <input
                          type="checkbox"
                          checked={filters.labelIds.includes(label.id)}
                          onChange={() => handleLabelToggle(label.id)}
                        />
                        <span>{label.name}</span>
                      </label>
                    ))}
                  </div>
                )}
              </div>
            </section>

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
