// Global spilstyring.
// Lige nu: mock-engine der simulerer værten OG deltagerne lokalt.
// Senere: udskift funktionskroppene med fetch-kald til API'et – komponenterne kan forblive.

import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { GameContext } from './gameContext'
import { getQuiz, packageInfo, QUIZ_ICONS, TILE_STYLES } from '../data/mock'
import { SEED_QUIZZES } from '../data/seedQuizzes'
import { loadLS, pickBots, randRoomId, saveLS } from '../lib/utils'
import { useNavigate } from 'react-router-dom'

const LS_USER = 'ecj-user'
const LS_QUIZZES = 'ecj-quizzes-v1'

export function GameProvider({ children }) {
  const navigate = useNavigate()
  const [user, setUser] = useState(() => loadLS(LS_USER, null))
  const [quizzes, setQuizzes] = useState(() => {
    const stored = loadLS(LS_QUIZZES, null)
    return Array.isArray(stored) && stored.length ? stored : SEED_QUIZZES
  })
  const [room, setRoom] = useState(null)
  const [player, setPlayer] = useState(null)
  const [draft, setDraft] = useState(null)
  const roomsRef = useRef({})

  useEffect(() => {
    if (quizzes) saveLS(LS_QUIZZES, quizzes)
  }, [quizzes])
  useEffect(() => {
    if (user) saveLS(LS_USER, user)
    else window.localStorage.removeItem(LS_USER)
  }, [user])

  /* ---------------- Værten (arrangør) ---------------- */

  const login = useCallback((name) => {
    setUser({ name: name || 'Arrangør' })
    navigate('/dashboard')
  }, [navigate])

  const logout = useCallback(() => {
    setUser(null)
    setRoom(null)
    navigate('/login')
  }, [navigate])

  const startSession = useCallback((quiz) => {
    if (!quiz || !quiz.questions?.length) return
    const id = randRoomId(new Set(Object.keys(roomsRef.current)))
    const r = {
      id,
      quizId: quiz.id,
      cap: packageInfo(quiz.packageId).cap,
      participants: [],
      status: 'lobby', // 'lobby' | 'countdown' | 'question' | 'reveal' | 'results'
      index: 0,
      answerCounts: [0, 0, 0, 0],
      scores: {},
    }
    roomsRef.current[id] = r
    setRoom(r)
    navigate('/lobby')
  }, [navigate])

  const addParticipant = useCallback((name) => {
    setRoom((r) => {
      if (!r || r.status !== 'lobby' || r.participants.length >= r.cap || r.participants.includes(name)) return r
      return { ...r, participants: [...r.participants, name] }
    })
  }, [])

  const removeParticipant = useCallback((name) => {
    setRoom((r) => (r ? { ...r, participants: r.participants.filter((p) => p !== name) } : r))
  }, [])

  const launchQuiz = useCallback(() => {
    setRoom((r) => (r ? { ...r, status: 'countdown', index: 0, answerCounts: [0, 0, 0, 0], scores: {} } : r))
    navigate('/live')
  }, [navigate])

  const startQuizDraft = useCallback(() => {
    setDraft(null)
    navigate('/opret')
  }, [navigate])

  const openEditor = useCallback((quizId) => {
    setDraft(quizId)
    navigate('/opret')
  }, [navigate])

  const clearDraft = useCallback(() => setDraft(null), [])

  const saveQuiz = useCallback((quiz) => {
    setQuizzes((list) => {
      const base = list ?? []
      if (quiz.id) {
        return base.map((it) => (it.id === quiz.id ? quiz : it))
      }
      const icon = quiz.icon ?? QUIZ_ICONS[base.length % QUIZ_ICONS.length]
      const tile = quiz.tile ?? TILE_STYLES[base.length % TILE_STYLES.length]
      return [{ ...quiz, id: `quiz-${Date.now()}`, icon, tile }, ...base]
    })
    setDraft(null)
    navigate('/dashboard')
  }, [navigate])

  const duplicateQuiz = useCallback((quizId) => {
    setQuizzes((list) => {
      if (!list) return list
      const idx = list.findIndex((it) => it.id === quizId)
      if (idx < 0) return list
      const src = list[idx]
      const copy = { ...src, id: `quiz-${Date.now()}`, title: `${src.title} (kopi)` }
      copy.icon = QUIZ_ICONS[(list.length + 1) % QUIZ_ICONS.length]
      copy.tile = TILE_STYLES[(list.length + 1) % TILE_STYLES.length]
      const next = [...list]
      next.splice(idx + 1, 0, copy)
      return next
    })
  }, [])

  const deleteQuiz = useCallback((quizId) => {
    setQuizzes((list) => (list ? list.filter((it) => it.id !== quizId) : list))
  }, [])

  /* ---------------- Deltager (spiller) ---------------- */

  const startJoin = useCallback(
    (code) => {
      const known = roomsRef.current[code]
      let quiz
      let others
      if (known) {
        quiz = getQuiz(quizzes ?? [], known.quizId)
        others = pickBots(5, known.participants)
        others = [...known.participants, ...others].slice(0, 6)
      } else {
        quiz = getQuiz(quizzes ?? [], 'firmafest-2026')
        others = pickBots(5)
      }
      const scores = {}
      others.forEach((n) => {
        scores[n] = 0
      })
      setPlayer({
        roomId: code,
        quizId: quiz?.id,
        name: null,
        index: 0,
        phase: 'name', // 'name' | 'countdown' | 'question' | 'reveal' | 'results'
        scores,
        others,
        chosenIdx: null,
        correct: null,
        lastPoints: 0,
      })
      navigate('/deltag/navn')
    },
    [quizzes, navigate],
  )

  const setPlayerName = useCallback((name) => {
    const clean = name.trim() || 'Gæst'
    setPlayer((p) => (p ? { ...p, name: clean, scores: { ...p.scores, [clean]: 0 }, phase: 'countdown' } : p))
    navigate('/spil')
  }, [navigate])

  const resetPlayer = useCallback(() => {
    setPlayer(null)
    navigate('/deltag')
  }, [navigate])

  const value = useMemo(
    () => ({
      user,
      quizzes,
      room,
      player,
      draft,
      setRoom,
      setPlayer,
      // vært
      login,
      logout,
      startSession,
      addParticipant,
      removeParticipant,
      launchQuiz,
      startQuizDraft,
      openEditor,
      clearDraft,
      saveQuiz,
      duplicateQuiz,
      deleteQuiz,
      // deltager
      startJoin,
      setPlayerName,
      resetPlayer,
    }),
    [user, quizzes, room, player, draft, login, logout, startSession, addParticipant, removeParticipant, launchQuiz, startQuizDraft, openEditor, clearDraft, saveQuiz, duplicateQuiz, deleteQuiz, startJoin, setPlayerName, resetPlayer],
  )

  return <GameContext.Provider value={value}>{children}</GameContext.Provider>
}
