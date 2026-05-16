import { useEffect, useState } from 'react'
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
            <span className="eyebrow">Projects</span>
            <h1>Your workspace</h1>
            <p className="auth-subtitle">Create the first project and use it as the base for issues.</p>
          </div>

          <div className="projects-header-actions">
            <div className="user-summary">
              <strong>{user.displayName}</strong>
              <span>{user.email}</span>
            </div>
            <button type="button" className="secondary-button" onClick={logout}>
              Sign out
            </button>
          </div>
        </header>

        <section className="projects-grid">
          <section className="projects-panel">
            <h2>Create project</h2>
            <form className="auth-form" onSubmit={handleSubmit}>
              <label>
                <span>Project name</span>
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
                {isSubmitting ? 'Creating project...' : 'Create project'}
              </button>
            </form>
          </section>

          <section className="projects-panel">
            <div className="projects-list-header">
              <h2>Projects</h2>
              <span className="projects-count">{projects.length}</span>
            </div>

            {isLoading ? <p className="auth-subtitle">Loading projects...</p> : null}

            {!isLoading && projects.length === 0 ? (
              <p className="auth-subtitle">No projects yet. Create one to start the tracker flow.</p>
            ) : null}

            {!isLoading && projects.length > 0 ? (
              <ul className="projects-list">
                {projects.map((project) => (
                  <li key={project.id} className="project-card">
                    <div>
                      <strong>{project.name}</strong>
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
