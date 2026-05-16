import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { issuesApi } from '../issues/issuesApi'

const STATUS_OPTIONS = [
  { value: '', label: 'Все открытые статусы' },
  { value: 'backlog', label: 'backlog' },
  { value: 'todo', label: 'todo' },
  { value: 'in-progress', label: 'in-progress' },
  { value: 'in-review', label: 'in-review' },
  { value: 'done', label: 'done' },
]

function ProjectIssuesPage() {
  const { projectSlug } = useParams()
  const { accessToken } = useAuth()
  const halfWidthFieldStyle = { display: 'flex', flexDirection: 'column', gap: '8px', width: '50%' }
  const fullWidthControlStyle = { width: '100%' }
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
            <span className="eyebrow">Задачи</span>
            <h1>{projectSlug}</h1>
            <p className="auth-subtitle">Быстро создавайте задачи и подготавливайте их к triage.</p>
          </div>

          <Link to="/projects" className="secondary-button nav-link-button">
            Назад к проектам
          </Link>
        </header>

        <section className="projects-grid">
          <section className="projects-panel">
            <h2>Новая задача</h2>
            <form className="auth-form" onSubmit={handleSubmit}>
              <label>
                <span>Заголовок</span>
                <input value={title} onChange={(event) => setTitle(event.target.value)} placeholder="Login form breaks on Safari" required />
              </label>

              <label>
                <span>Описание</span>
                <textarea
                  value={description}
                  onChange={(event) => setDescription(event.target.value)}
                  placeholder="Необязательный контекст для задачи"
                  rows={5}
                />
              </label>

              {error ? <p className="form-error">{error}</p> : null}

              <button type="submit" className="primary-button" disabled={isSubmitting}>
                {isSubmitting ? 'Создание задачи...' : 'Создать задачу'}
              </button>
            </form>
          </section>

          <section className="projects-panel">
            <div className="projects-list-header">
              <h2>Задачи проекта</h2>
              <span className="projects-count">{issues.length}</span>
            </div>

            <section className="issue-section">
              <h3>Фильтры</h3>
              <div className="project-issues-filters-stack">
                <label className="project-issues-filter-field" style={halfWidthFieldStyle}>
                  <span>Поиск</span>
                  <input
                    value={filters.query}
                    onChange={(event) => handleFilterChange('query', event.target.value)}
                    placeholder="Искать по заголовку или описанию"
                    style={fullWidthControlStyle}
                  />
                </label>

                <label className="project-issues-filter-field" style={halfWidthFieldStyle}>
                  <span>Статус</span>
                  <select
                    value={filters.status}
                    onChange={(event) => handleFilterChange('status', event.target.value)}
                    style={fullWidthControlStyle}
                  >
                    {STATUS_OPTIONS.map((option) => (
                      <option key={option.value || 'all'} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                </label>

                <label className="project-issues-filter-field" style={halfWidthFieldStyle}>
                  <span>Исполнитель</span>
                  <select
                    value={filters.assigneeUserId}
                    onChange={(event) => handleFilterChange('assigneeUserId', event.target.value)}
                    style={fullWidthControlStyle}
                  >
                    <option value="">Любой</option>
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
                <span>Показывать выполненные задачи, если явно не выбран фильтр по статусу</span>
              </label>

              <div>
                <span className="issue-meta-label">Метки</span>
                {availableLabels.length === 0 ? (
                  <p className="auth-subtitle">У проекта пока нет меток.</p>
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

            {isLoading ? <p className="auth-subtitle">Загрузка задач...</p> : null}

            {!isLoading && issues.length === 0 ? (
              <p className="auth-subtitle">Задач пока нет. Создайте первый элемент backlog для этого проекта.</p>
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
