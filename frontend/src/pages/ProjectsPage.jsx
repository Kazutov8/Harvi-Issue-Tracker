import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { projectsApi } from '../projects/projectsApi'

function ProjectsPage() {
  const { accessToken, logout, user } = useAuth()
  const [projects, setProjects] = useState([])
  const [name, setName] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    let isActive = true

    async function loadProjects() {
      setIsLoading(true)
      setError('')

      try {
        const response = await projectsApi.list(accessToken)

        if (isActive) {
          setProjects(response)
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

    loadProjects()

    return () => {
      isActive = false
    }
  }, [accessToken])

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setIsSubmitting(true)

    try {
      const createdProject = await projectsApi.create({ name }, accessToken)
      setProjects((current) => [...current, createdProject])
      setName('')
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
            <span className="eyebrow">Проекты</span>
            <h1>Ваше рабочее пространство</h1>
            <p className="auth-subtitle">Создайте первый проект и используйте его как основу для задач.</p>
          </div>

          <div className="projects-header-actions">
            <div className="user-summary">
              <strong>{user.displayName}</strong>
              <span>{user.email}</span>
            </div>
            <button type="button" className="secondary-button" onClick={logout}>
              Выйти
            </button>
          </div>
        </header>

        <section className="projects-grid">
          <section className="projects-panel">
            <h2>Создать проект</h2>
            <form className="auth-form" onSubmit={handleSubmit}>
              <label>
                <span>Название проекта</span>
                <input
                  type="text"
                  value={name}
                  onChange={(event) => setName(event.target.value)}
                  placeholder="Internal Tools"
                  required
                />
              </label>

              {error ? <p className="form-error">{error}</p> : null}

              <button type="submit" className="primary-button" disabled={isSubmitting}>
                {isSubmitting ? 'Создание проекта...' : 'Создать проект'}
              </button>
            </form>
          </section>

          <section className="projects-panel">
            <div className="projects-list-header">
              <h2>Проекты</h2>
              <span className="projects-count">{projects.length}</span>
            </div>

            {isLoading ? <p className="auth-subtitle">Загрузка проектов...</p> : null}

            {!isLoading && projects.length === 0 ? (
              <p className="auth-subtitle">Проектов пока нет. Создайте первый, чтобы начать работу в трекере.</p>
            ) : null}

            {!isLoading && projects.length > 0 ? (
              <ul className="projects-list">
                {projects.map((project) => (
                  <li key={project.id} className="project-card">
                    <div>
                      <Link to={`/projects/${project.slug}/issues`} className="issue-link">
                        {project.name}
                      </Link>
                      <p className="project-slug">/{project.slug}</p>
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

export default ProjectsPage
