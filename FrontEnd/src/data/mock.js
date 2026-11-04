// Testdata + små utilities. Udskiftes senere med rigtige API-kald.

export const LETTERS = ['A', 'B', 'C', 'D']

export const ANSWER_COLORS = ['#e2364c', '#1368ce', '#2e9e44', '#e3b53b']

export const PACKAGES = [
  { id: 'lille', name: 'Lille', players: '5 deltagere', features: 'Kun tekst', cap: 5 },
  { id: 'mellem', name: 'Mellem', players: '50 deltagere', features: 'Tekst + billeder', cap: 50 },
  { id: 'stor', name: 'Stor', players: '100 deltagere', features: 'Tekst + billeder + lyd', cap: 100 },
  { id: 'unlimited', name: 'Unlimited', players: '500 deltagere', features: 'Tekst + billede + lyd', cap: 500 },
]

export const packageInfo = (id) => PACKAGES.find((p) => p.id === id) ?? PACKAGES[2]

export const getQuiz = (quizzes, id) => (quizzes.find((q) => q.id === id) ?? quizzes[0]) ?? null

export const DEMO_NAMES = ['Mads', 'Sofie', 'Kasper', 'Lasse', 'Emma', 'Sarah', 'Oliver', 'Jonas', 'Anna', 'Mette']

export const QUIZ_ICONS = ['🚀', '🎯', '🎸', '🧩', '🎬', '🎉']

export const TILE_STYLES = [
  'linear-gradient(135deg,#122a4f,#1b3f78)',
  'linear-gradient(135deg,#1c4fd8,#2f7bf0)',
  'linear-gradient(135deg,#262031,#41394f)',
  'linear-gradient(135deg,#0f5c46,#17966d)',
]

export const emptyQuestion = () => ({ text: '', options: ['', '', '', ''], correct: 0, time: 20, image: '' })
