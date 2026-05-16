import { apiClient } from '../api/client'

export const projectsApi = {
  list(accessToken) {
    return apiClient.get('/projects', { accessToken })
  },
  create(payload, accessToken) {
    return apiClient.post('/projects', payload, { accessToken })
  },
}
