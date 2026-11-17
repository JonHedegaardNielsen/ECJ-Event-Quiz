// Tynd HTTP-klient til EventQuiz API.
// Dev (Vite): port 5218. Release (ServerUI): same-origin (tom base).
// Overrives via localStorage ('ecj-api-url') eller VITE_API_URL.

const API_FALLBACK = import.meta.env.PROD
  ? ''
  : `${window.location.protocol}//${window.location.hostname}:5218`

export function apiBase() {
  try {
    const stored = window.localStorage.getItem('ecj-api-url')
    if (stored) return stored
  } catch {
    // privado mode m.m. – ignorér
  }
  if (import.meta.env.VITE_API_URL !== undefined) return import.meta.env.VITE_API_URL
  return API_FALLBACK
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

  if (res.ok) {
    // 204 (No Content) og andre kropsløse svar må ikke parses som JSON
    if (res.status === 204) return null
    const text = await res.text()
    return text ? JSON.parse(text) : null
  }

  // Fejlbeskeder fra API'et kan være JSON-string eller ProblemDetails.
  // Ved valideringsfejl (ValidationProblemDetails) vises den første konkrete fejl.
  let msg = ''
  const raw = await res.text().catch(() => '')
  if (raw) {
    try {
      const parsed = JSON.parse(raw)
      if (typeof parsed === 'string') {
        msg = parsed
      } else {
        const firstError = parsed.errors ? Object.values(parsed.errors).flat()[0] : undefined
        msg = firstError ?? parsed.detail ?? parsed.title ?? raw
      }
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

export function listOpenRooms() {
  return request('/api/quizsession/open', { method: 'GET' })
}
