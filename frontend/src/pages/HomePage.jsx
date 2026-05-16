import { Navigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

function HomePage() {
  const { user } = useAuth()

  if (user) {
    return <Navigate to="/projects" replace />
  }

  return null
}

export default HomePage
