// Global spilstyring.
// Lige nu: mock-engine der simulerer værten OG deltagerne lokalt.
// SignalR: sæt USE_SIGNALR=true når ServerUI kører – se koblingspunkterne nedenfor.
// Klient: ../lib/signalrClient.js  |  Hub: QuizHub i ECJ.EventQuiz.Core

import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { GameContext } from './gameContext'
import { getQuiz, packageInfo, QUIZ_ICONS, TILE_STYLES } from '../data/mock'
import { SEED_QUIZZES } from '../data/seedQuizzes'
import { createQuizHub } from '../lib/signalrClient'
import { loadLS, pickBots, randRoomId, saveLS } from '../lib/utils'
import { navigate } from '../lib/router'

const LS_USER = 'ecj-user'
const LS_QUIZZES = 'ecj-quizzes-v1'

/** Skift til true når WPF ServerUI kører (http://localhost:5188). */
const USE_SIGNALR = false

export function GameProvider({ children }) {
  const [user, setUser] = useState(() => loadLS(LS_USER, null))
  const [quizzes, setQuizzes] = useState(() => {
    const stored = loadLS(LS_QUIZZES, null)
    return Array.isArray(stored) && stored.length ? stored : SEED_QUIZZES
  })
  const [room, setRoom] = useState(null)
  const [player, setPlayer] = useState(null)
  const [draft, setDraft] = useState(null)
  const roomsRef = useRef({})
  const hubRef = useRef(null)

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
  }, [])

  const logout = useCallback(() => {
    setUser(null)
    setRoom(null)
    navigate('/login')
  }, [])

  const startSession = useCallback(async (quiz) => {
    if (!quiz || !quiz.questions?.length) return

    // --- SignalR-kobling (når USE_SIGNALR) ---
    // Seed-quizzer på serveren har int-id 1/2/3. Map lokal mock-id → server-id efter behov.
    if (USE_SIGNALR) {
      const hub = createQuizHub()
      hubRef.current = hub
      await hub.connect()
      hub.on('LobbyUpdated', (dto) => {
        setRoom((r) =>
          r
            ? { ...r, participants: dto.participants.map((p) => p.name) }
            : r,
        )
      })
      hub.on('QuestionChanged', (dto) => {
        setRoom((r) =>
          r
            ? {
                ...r,
                status: dto.phase,
                index: dto.question.index,
                answerCounts: [0, 0, 0, 0],
              }
            : r,
        )
      })
      hub.on('AnswerCountsUpdated', (dto) => {
        setRoom((r) => (r ? { ...r, answerCounts: [...dto.counts] } : r))
      })
      hub.on('QuestionClosed', () => {
        setRoom((r) => (r ? { ...r, status: 'reveal' } : r))
      })
      hub.on('QuizEnded', () => {
        setRoom((r) => (r ? { ...r, status: 'results' } : r))
        navigate('/resultat')
      })

      const serverQuizId = Number(quiz.serverId) || 1
      const created = await hub.createSession(serverQuizId)
      const r = {
        id: created.roomId,
        quizId: quiz.id,
        cap: packageInfo(quiz.packageId).cap,
        participants: [],
        status: 'lobby',
        index: 0,
        answerCounts: [0, 0, 0, 0],
        scores: {},
        live: true,
      }
      roomsRef.current[r.id] = r
      setRoom(r)
      navigate('/lobby')
      return
    }

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
  }, [])

  const addParticipant = useCallback((name) => {
    setRoom((r) => {
      if (!r || r.status !== 'lobby' || r.participants.length >= r.cap || r.participants.includes(name)) return r
      return { ...r, participants: [...r.participants, name] }
    })
  }, [])

  const removeParticipant = useCallback((name) => {
    setRoom((r) => (r ? { ...r, participants: r.participants.filter((p) => p !== name) } : r))
  }, [])

  const launchQuiz = useCallback(async () => {
    // --- SignalR: hub.startQuiz(room.id) i stedet for lokal status ---
    if (USE_SIGNALR && hubRef.current && room?.id) {
      await hubRef.current.startQuiz(room.id)
      navigate('/live')
      return
    }
    setRoom((r) => (r ? { ...r, status: 'countdown', index: 0, answerCounts: [0, 0, 0, 0], scores: {} } : r))
    navigate('/live')
  }, [room?.id])

  const startQuizDraft = useCallback(() => {
    setDraft(null)
    navigate('/opret')
  }, [])

  const openEditor = useCallback((quizId) => {
    setDraft(quizId)
    navigate('/opret')
  }, [])

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
  }, [])

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
    async (code) => {
      // --- SignalR: connect + gem roomId; JoinLobby kaldes i setPlayerName ---
      if (USE_SIGNALR) {
        const hub = createQuizHub()
        hubRef.current = hub
        await hub.connect()
        hub.on('LobbyUpdated', () => {})
        hub.on('QuizStarted', () => navigate('/spil'))
        hub.on('QuestionChanged', (dto) => {
          setPlayer((p) =>
            p
              ? {
                  ...p,
                  phase: dto.phase,
                  index: dto.question.index,
                  chosenIdx: null,
                  correct: null,
                  lastPoints: 0,
                  liveQuestion: dto.question,
                }
              : p,
          )
        })
        hub.on('QuestionClosed', () => {
          setPlayer((p) => (p ? { ...p, phase: 'reveal' } : p))
        })
        hub.on('QuizEnded', (dto) => {
          setPlayer((p) => {
            if (!p) return p
            const scores = { ...p.scores }
            for (const entry of dto.leaderboard ?? []) scores[entry.name] = entry.score
            return { ...p, phase: 'results', scores }
          })
          navigate('/resultat')
        })

        setPlayer({
          roomId: code,
          quizId: 'firmafest-2026',
          name: null,
          index: 0,
          phase: 'name',
          scores: {},
          others: [],
          chosenIdx: null,
          correct: null,
          lastPoints: 0,
          live: true,
        })
        navigate('/deltag/navn')
        return
      }

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
    [quizzes],
  )

  const setPlayerName = useCallback(async (name) => {
    const clean = name.trim() || 'Gæst'

    // --- SignalR: JoinLobby(roomId, name) – værten ser LobbyUpdated ---
    if (USE_SIGNALR && hubRef.current && player?.roomId) {
      await hubRef.current.joinLobby(player.roomId, clean)
      setPlayer((p) => (p ? { ...p, name: clean, scores: { ...p.scores, [clean]: 0 }, phase: 'countdown' } : p))
      navigate('/spil')
      return
    }

    setPlayer((p) => (p ? { ...p, name: clean, scores: { ...p.scores, [clean]: 0 }, phase: 'countdown' } : p))
    navigate('/spil')
  }, [player?.roomId])

  const resetPlayer = useCallback(() => {
    setPlayer(null)
    navigate('/deltag')
  }, [])

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
