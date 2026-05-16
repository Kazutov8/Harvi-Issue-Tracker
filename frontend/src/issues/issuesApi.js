import { apiClient } from '../api/client'

function buildIssuesQuery(filters = {}) {
  const searchParams = new URLSearchParams()

  if (filters.status) {
    searchParams.set('status', filters.status)
  }

  if (filters.assigneeUserId) {
    searchParams.set('assigneeUserId', filters.assigneeUserId)
  }

  if (filters.query) {
    searchParams.set('query', filters.query)
  }

  if (filters.includeDone) {
    searchParams.set('includeDone', 'true')
  }

  if (filters.page) {
    searchParams.set('page', String(filters.page))
  }

  if (filters.pageSize) {
    searchParams.set('pageSize', String(filters.pageSize))
  }

  for (const labelId of filters.labelIds ?? []) {
    searchParams.append('labelIds', labelId)
  }

  const queryString = searchParams.toString()
  return queryString ? `?${queryString}` : ''
}

export const issuesApi = {
  list(projectSlug, accessToken, filters) {
    return apiClient.get(`/projects/${projectSlug}/issues${buildIssuesQuery(filters)}`, { accessToken })
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
