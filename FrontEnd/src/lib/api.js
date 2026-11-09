// Tynd HTTP-klient til EventQuiz.Api (BackEnd/EventQuiz.Api, default port 5218).
// Overrives via localStorage ('ecj-api-url') eller VITE_API_URL.

const API_FALLBACK = 'http://localhost:5218'

export function apiBase() {
  try {
    const stored = window.localStorage.getItem('ecj-api-url')
    if (stored) return stored
  } catch {
    // privado mode m.m. – ignorér
  }
  return import.meta.env?.VITE_API_URL ?? API_FALLBACK
}

async function request(path, options) {
  let res
  try {
    res = await fetch(`${apiBase()}${path}`, {
      headers: options.body ? { 'Content-Type': 'application/json' } : undefined,
      ...options,
    })
  } catch {
    throw new Error(`Kan ikke nå serveren (${apiBase()}) – er API'et startet?`)
  }

  if (res.ok) return res.json()

  // Fejlbeskeder fra API'et kan være JSON-string eller ProblemDetails.
  let msg = ''
  const raw = await res.text().catch(() => '')
  if (raw) {
    try {
      const parsed = JSON.parse(raw)
      msg = typeof parsed === 'string' ? parsed : (parsed.detail ?? parsed.title ?? raw)
    } catch {
      msg = raw
    }
  }
  throw new ApiError(msg.replace(/^"|"$/g, '') || 'Ukendt fejl fra serveren', res.status)
}

export class ApiError extends Error {
  constructor(message, status) {
    super(message)
    this.status = status
  }
}

export function loginUser(username, password) {
  return request('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify({ username, password }),
  })
}

export function registerUser(username, password) {
  return request('/api/auth/register', {
    method: 'POST',
    body: JSON.stringify({ username, password }),
  })
}

export function createQuizOnApi(quiz) {
  return request('/api/Quiz', {
    method: 'POST',
    body: JSON.stringify(quiz),
  })
}

export function listQuizzesFromApi(creatorId) {
  const query = creatorId != null ? `?creatorId=${encodeURIComponent(creatorId)}` : ''
  return request(`/api/Quiz${query}`, { method: 'GET' })
}

export function getQuizFromApi(id) {
  return request(`/api/Quiz/${encodeURIComponent(id)}`, { method: 'GET' })
}

export function deleteQuizOnApi(id) {
  return request(`/api/Quiz/${id}`, { method: 'DELETE' })
}
