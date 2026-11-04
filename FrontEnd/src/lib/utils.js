// Små hjælpefunktioner (localStorage, point, mocks).
import { DEMO_NAMES } from '../data/mock'

export function loadLS(key, fallback) {
  try {
    const v = JSON.parse(window.localStorage.getItem(key))
    return v ?? fallback
  } catch {
    return fallback
  }
}

export function saveLS(key, value) {
  try {
    window.localStorage.setItem(key, JSON.stringify(value))
  } catch {
    // ignorér fx privado mode
  }
}

export function randRoomId(taken) {
  let id
  do {
    id = String(Math.floor(1000 + Math.random() * 9000))
  } while (taken.has(id))
  return id
}

// Deep clone af quiz-editor-draft
export const clone = (v) => JSON.parse(JSON.stringify(v))

// Vælg n mock-deltagere fra demopoolen (undtagen de allerede tilføjede)
export function pickBots(n, exclude = []) {
  const pool = DEMO_NAMES.filter((x) => !exclude.includes(x))
  const out = []
  while (out.length < n && pool.length) {
    out.push(...pool.splice(Math.floor(Math.random() * pool.length), 1))
  }
  let i = out.length + 1
  while (out.length < n) {
    out.push(`Gæst ${i++}`)
  }
  return out.slice(0, n)
}

// Kahoot-agtige point: korrekt svar = 300 + hastighedsbonus (op til 700)
export function pointsFor(secondsLeft, time) {
  const ratio = time > 0 ? Math.max(0, Math.min(1, secondsLeft / time)) : 0
  return Math.round(300 + 700 * ratio)
}

// Vælg en forkert svarmulighed (til mock-deltagere)
export function otherIndex(correct, count) {
  const i = Math.floor(Math.random() * (count - 1))
  return i >= correct ? i + 1 : i
}
