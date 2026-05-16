import { apiClient } from '../api/client'

export const issuesApi = {
  list(projectSlug, accessToken) {
    return apiClient.get(`/projects/${projectSlug}/issues`, { accessToken })
  },
  getById(issueId, accessToken) {
    return apiClient.get(`/issues/${issueId}`, { accessToken })
  },
  create(projectSlug, payload, accessToken) {
    return apiClient.post(`/projects/${projectSlug}/issues`, payload, { accessToken })
  },
  listLabels(projectSlug, accessToken) {
    return apiClient.get(`/projects/${projectSlug}/labels`, { accessToken })
  },
  suggestTriage(issueId, accessToken) {
    return apiClient.post(`/issues/${issueId}/ai-suggest`, undefined, { accessToken })
  },
  applyTriage(issueId, payload, accessToken) {
    return apiClient.post(`/issues/${issueId}/apply-triage-suggestion`, payload, { accessToken })
  },
  assign(issueId, assigneeUserId, accessToken) {
    return apiClient.post(`/issues/${issueId}/assign`, { assigneeUserId }, { accessToken })
  },
  transition(issueId, status, accessToken) {
    return apiClient.post(`/issues/${issueId}/transition`, { status }, { accessToken })
  },
  listUsers(accessToken) {
    return apiClient.get('/auth/users', { accessToken })
  },
}
