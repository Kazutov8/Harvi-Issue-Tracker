import { Route, Routes } from 'react-router-dom'
import ProtectedRoute from './auth/ProtectedRoute'
import HomePage from './pages/HomePage'
import IssueDetailsPage from './pages/IssueDetailsPage'
import LoginPage from './pages/LoginPage'
import ProjectIssuesPage from './pages/ProjectIssuesPage'
import ProjectsPage from './pages/ProjectsPage'
import RegisterPage from './pages/RegisterPage'

function App() {
  return (
    <Routes>
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <HomePage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/projects"
        element={
          <ProtectedRoute>
            <ProjectsPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/projects/:projectSlug/issues"
        element={
          <ProtectedRoute>
            <ProjectIssuesPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/issues/:issueId"
        element={
          <ProtectedRoute>
            <IssueDetailsPage />
          </ProtectedRoute>
        }
      />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
    </Routes>
  )
}

export default App
