const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7017';

async function request(path, options = {}) {
  const headers = {
    'Content-Type': 'application/json',
    ...(options.accessToken ? { Authorization: `Bearer ${options.accessToken}` } : {}),
    ...(options.headers ?? {}),
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers,
    ...options,
  })

  const responseText = response.status === 204 ? '' : await response.text()

  if (!response.ok) {
    const error = new Error(responseText || `Request failed with status ${response.status}`)
    error.status = response.status
    error.responseText = responseText
    throw error
  }

  if (response.status === 204) {
    return null
  }

  return responseText ? JSON.parse(responseText) : null
}

export const apiClient = {
  get: (path, options) => request(path, { ...options, method: 'GET' }),
  post: (path, body, options) =>
    request(path, {
      ...options,
      method: 'POST',
      body: body === undefined ? undefined : JSON.stringify(body),
    }),
}
