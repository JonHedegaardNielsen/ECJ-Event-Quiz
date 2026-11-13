// Global spilstyring.
// LIVE-mode (default): rigtige deltagere via SignalR til QuizHub (WPF ServerUI, port 5188).
// MOCK-mode: engine der simulerer værten OG deltagerne lokalt (VITE_USE_SIGNALR=false).
// Klient: ../lib/signalrClient.js  |  Hub: QuizHub i ECJ.EventQuiz.Core

import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { GameContext } from './gameContext'
import { getQuiz, packageInfo, QUIZ_ICONS, TILE_STYLES } from '../data/mock'
import { SEED_QUIZZES } from '../data/seedQuizzes'
import { createQuizHub } from '../lib/signalrClient'
import { loadLS, pickBots, randRoomId, saveLS } from '../lib/utils'
import { createQuizOnApi, deleteQuizOnApi, getQuizFromApi, listQuizzesFromApi, loginUser } from '../lib/api'
import { useNavigate } from 'react-router-dom'

const LS_USER = 'ecj-user'
const LS_QUIZZES = 'ecj-quizzes-v1'

/** SignalR live som default; sæt VITE_USE_SIGNALR=false for mock-demo uden server. */
const USE_SIGNALR = import.meta.env?.VITE_USE_SIGNALR !== 'false'

/** Serverens QuestionDto → den q-shape UI'en kender (options er strings + optionIds til hub-kald). */
function dtoQuestion(dto) {
  return {
    id: dto.id,
    text: dto.text,
    time: dto.timeLimitSeconds,
    index: dto.index,
    total: dto.total,
    options: dto.options.map((o) => o.answer),
    optionIds: dto.options.map((o) => o.id),
  }
}

const API_PREFIX = 'api-'

/** QuizDto (fuld) → lokal quiz-shape. None-standard lokal meta sættes via overrides. */
function quizToLocal(dto, overrides = {}) {
  return {
    id: `${API_PREFIX}${dto.id}`,
    serverId: dto.id,
    title: overrides.title ?? dto.name,
    description: overrides.description ?? '',
    packageId: overrides.packageId ?? 'stor',
    icon: overrides.icon ?? QUIZ_ICONS[apiIndex(dto.id) % QUIZ_ICONS.length],
    tile: overrides.tile ?? TILE_STYLES[apiIndex(dto.id) % TILE_STYLES.length],
    questions: dto.questions.map((q) => {
      const answers = q.answerOptions.map((o) => o.answer)
      const correctIdx = q.answerOptions.findIndex((o) => o.isCorrect)
      const padded = [...answers]
      while (padded.length < Math.max(4, answers.length)) padded.push('')
      return {
        text: q.question,
        options: padded,
        correct: correctIdx >= 0 ? correctIdx : 0,
        time: q.timeLimitSeconds ?? 20,
        image: '',
      }
    }),
  }
}

/** QuizSummaryDto (listen-hit) → lokal quiz-shape uden spørgsmål (hydreres senere via getQuizFromApi). */
function summaryToLocal(s) {
  return {
    id: `${API_PREFIX}${s.id}`,
    serverId: s.id,
    title: s.name,
    description: '',
    packageId: 'stor',
    icon: QUIZ_ICONS[apiIndex(s.id) % QUIZ_ICONS.length],
    tile: TILE_STYLES[apiIndex(s.id) % TILE_STYLES.length],
    questions: [],
  }
}

/** Stabil udvælgelse så ikon/tile ser ens ud for et givet quiz-id. */
const apiIndex = (serverId) => Number(serverId) - 1

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
  const [quizzesLoading, setQuizzesLoading] = useState(false)
  const [quizzesError, setQuizzesError] = useState(null)
  const roomsRef = useRef({})
  const hubRef = useRef(null)

  useEffect(() => {
    if (quizzes) saveLS(LS_QUIZZES, quizzes)
  }, [quizzes])
  useEffect(() => {
    if (user) saveLS(LS_USER, user)
    else window.localStorage.removeItem(LS_USER)
  }, [user])

  const stopHub = useCallback(() => {
    hubRef.current?.stop?.().catch(() => {})
    hubRef.current = null
  }, [])

  /* ---------------- Quizzer + spørgsmål (fra API) ---------------- */

  const refreshQuizzes = useCallback(
    async () => {
      setQuizzesLoading(true)
      setQuizzesError(null)
      try {
        // Ikke logget ind = alle quizzer (ingen creatorId-filter); ellers "mine" quizzer.
        const summaries = await listQuizzesFromApi(user?.id ?? null)
        if (!summaries.length) {
          // API'et er tomt: logget ind vises en tom liste; ellers beholdes seed/demo-data.
          if (user?.id) setQuizzes([])
          return
        }
        // Fuldt indhold for hver quiz (spørgsmål + svarmuligheder) hentes parallelt
        const details = await Promise.allSettled(summaries.map((s) => getQuizFromApi(s.id)))
        setQuizzes(
          summaries.map((s, i) =>
            details[i].status === 'fulfilled' ? quizToLocal(details[i].value) : summaryToLocal(s),
          ),
        )
      } catch (err) {
        // Behold den viste liste (fx seed/demo-data) ved fejl, og vis en synlig fejlbesked
        setQuizzesError('Kunne ikke hente dine quizzer fra API-et.')
        console.warn('Henting af quizzer fejlede:', err)
      } finally {
        setQuizzesLoading(false)
      }
    },
    [user],
  )

  // Hent quizzerne fra API'et ved start, og igen når brugeren logger ind/ud.
  useEffect(() => {
    refreshQuizzes()
  }, [refreshQuizzes])

  /* ---------------- SignalR-handlers (værten) ---------------- */

  const wireHostHandlers = useCallback(
    (hub) => {
      hub.on('LobbyUpdated', (dto) => {
        setRoom((r) => (r && r.id === dto.roomId ? { ...r, participants: dto.participants.map((p) => p.name) } : r))
      })
      hub.on('QuestionChanged', (dto) => {
        // Broadcastes til hele rummet: phase = 'countdown' | 'question'
        setRoom((r) =>
          r && r.id === dto.roomId
            ? {
                ...r,
                status: dto.phase,
                index: dto.question.index,
                total: dto.question.total,
                question: dtoQuestion(dto.question),
                answerCounts: dto.question.options.map(() => 0),
                correctIdx: null,
              }
            : r,
        )
      })
      hub.on('AnswerCountsUpdated', (dto) => {
        setRoom((r) => (r && r.id === dto.roomId ? { ...r, answerCounts: [...dto.counts] } : r))
      })
      hub.on('QuestionClosed', (dto) => {
        // Reveal: korrekt svarmulighed + endelige antal svar
        setRoom((r) => {
          if (!r || r.id !== dto.roomId) return r
          const correctIdx = r.question ? r.question.optionIds.indexOf(dto.correctAnswerOptionId) : null
          return { ...r, status: 'reveal', answerCounts: [...dto.counts], correctIdx }
        })
      })
      hub.on('QuizEnded', (dto) => {
        setRoom((r) => {
          if (!r || r.id !== dto.roomId) return r
          const scores = {}
          for (const entry of dto.leaderboard ?? []) scores[entry.name] = entry.score
          return { ...r, status: 'results', scores }
        })
        navigate('/resultat')
      })
    },
    [navigate],
  )

  /* ---------------- SignalR-handlers (deltageren) ---------------- */

  const wirePlayerHandlers = useCallback(
    (hub, roomId) => {
      hub.on('LobbyUpdated', (dto) => {
        setPlayer((p) => (p && p.roomId === roomId ? { ...p, others: dto.participants.map((x) => x.name) } : p))
      })
      hub.on('QuestionChanged', (dto) => {
        // Serverens fase-rytme driver hele flowet (countdown → question → reveal → …)
        setPlayer((p) =>
          p && p.roomId === roomId
            ? {
                ...p,
                phase: dto.phase,
                index: dto.question.index,
                total: dto.question.total,
                question: dtoQuestion(dto.question),
                chosenIdx: null,
                chosenOptionId: null,
                correct: null,
                answeredSecondsLeft: 0,
                lastPoints: 0,
              }
            : p,
        )
      })
      hub.on('QuestionClosed', (dto) => {
        // Reveal: korrekt svarmulighed afsløres; point beregnes i frontend med serverens formel
        // (ScoreCalculator: korrekt = 100 + 900 * resterende-tids-andel, forkert = 0).
        setPlayer((p) => {
          if (!p || p.roomId !== roomId || p.revealedForQuestion === dto.questionId) return p
          const time = p.question?.time ?? 0
          const ratio = time > 0 ? Math.max(0, Math.min(1, (p.answeredSecondsLeft ?? 0) / time)) : 0
          const correct = p.chosenOptionId == null ? null : p.chosenOptionId === dto.correctAnswerOptionId
          const points = correct ? Math.round(100 + ratio * 900) : 0
          return {
            ...p,
            phase: 'reveal',
            correctOptionId: dto.correctAnswerOptionId,
            correct,
            lastPoints: points,
            score: (p.score ?? 0) + points,
            revealedForQuestion: dto.questionId,
          }
        })
      })
      hub.on('QuizEnded', (dto) => {
        setPlayer((p) => {
          if (!p || p.roomId !== roomId) return p
          const scores = {}
          for (const entry of dto.leaderboard ?? []) scores[entry.name] = entry.score
          const mine = scores[p.name] ?? p.score ?? 0
          return { ...p, phase: 'results', scores, score: mine }
        })
        navigate('/resultat')
      })
    },
    [navigate],
  )

  /* ---------------- Værten (arrangør) ---------------- */

  const login = useCallback(
    async (username, password) => {
      const user = await loginUser(username, password)
      setUser({ id: user.id, name: user.username })
      navigate('/dashboard')
      return user
    },
    [navigate],
  )

  const logout = useCallback(() => {
    setUser(null)
    setRoom(null)
    stopHub()
    navigate('/login')
  }, [navigate, stopHub])

  const startSession = useCallback(
    async (quiz) => {
      if (!quiz || !quiz.questions?.length) return

      if (USE_SIGNALR) {
        // LIVE: Opret rum på serveren og vis lobbyen. Deltagerne JoinLobby'er med Room ID + navn.
        stopHub()
        const hub = createQuizHub()
        hubRef.current = hub

        try {
          await hub.connect()
          wireHostHandlers(hub)

          const serverQuizId = quiz.serverId ?? 1
          const created = await hub.createSession(serverQuizId)
          roomsRef.current[created.roomId] = quiz.id
          setRoom({
            id: created.roomId,
            quizId: quiz.id, // lokal mock-id (bruges af titel/ikon i lobbyen)
            title: created.quizName ?? quiz.title,
            serverQuizId,
            cap: packageInfo(quiz.packageId).cap,
            participants: [],
            status: 'lobby',
            index: 0,
            total: 0,
            question: null,
            answerCounts: [],
            correctIdx: null,
            scores: {},
            live: true,
          })
          navigate('/lobby')
          return
        } catch (err) {
          console.error('Kunne ikke oprette quiz-session', err)
          stopHub()
          return
        }
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
    },
    [navigate, stopHub, wireHostHandlers],
  )

  const addParticipant = useCallback((name) => {
    setRoom((r) => {
      if (!r || r.status !== 'lobby' || r.participants.length >= r.cap || r.participants.includes(name)) return r
      return { ...r, participants: [...r.participants, name] }
    })
  }, [])

  const removeParticipant = useCallback((name) => {
    setRoom((r) => (r ? { ...r, participants: r.participants.filter((p) => p !== name) } : r))
  }, [])

  const launchQuiz = useCallback(
    async () => {
      // LIVE: serveren starter quizzen og broadcast'er QuestionChanged til alle.
      if (USE_SIGNALR && hubRef.current && room?.live && room.id) {
        try {
          await hubRef.current.startQuiz(room.id)
        } catch (err) {
          console.error('Kunne ikke starte quizzen', err)
          return
        }
        navigate('/live')
        return
      }
      setRoom((r) => (r ? { ...r, status: 'countdown', index: 0, answerCounts: [0, 0, 0, 0], scores: {} } : r))
      navigate('/live')
    },
    [navigate, room],
  )

  // LIVE-only hub-metoder til live-værtsvisningen:
  const openQuestion = useCallback(
    (roomId) => hubRef.current?.openQuestion(roomId).catch(() => {}),
    [],
  )

  const closeQuestion = useCallback(
    (roomId) => hubRef.current?.closeQuestion(roomId).catch(() => {}),
    [],
  )

  const nextQuestion = useCallback(
    (roomId) => hubRef.current?.nextQuestion(roomId).catch(() => {}),
    [],
  )

  const startQuizDraft = useCallback(() => {
    setDraft(null)
    navigate('/opret')
  }, [navigate])

  const openEditor = useCallback((quizId) => {
    setDraft(quizId)
    navigate('/opret')
  }, [navigate])

  const clearDraft = useCallback(() => setDraft(null), [])

  const saveQuiz = useCallback(
    async (quiz) => {
      const title = quiz.title?.trim() || 'Uden titel'
      const localId = quiz.id || `quiz-${Date.now()}`

      // Lokal validering (matcher API-reglerne, så brugeren ikke venter på en 400):
      quiz.questions.forEach((q, i) => {
        const n = i + 1
        if (!q.text.trim()) throw new Error(`Spørgsmål ${n} mangler en tekst.`)
        if (q.options.filter((o) => o.trim() !== '').length < 2) {
          throw new Error(`Spørgsmål ${n} skal have mindst to udfyldte svarmuligheder.`)
        }
        if (q.correct == null || q.options[q.correct]?.trim() === '') {
          throw new Error(`Spørgsmål ${n} skal have et rigtigt svar blandt de udfyldte muligheder.`)
        }
      })

      // --- POST /api/Quiz: gem quizzen i databasen ---
      const payload = {
        name: title,
        creatorId: user?.id ?? 0,
        questions: quiz.questions.map((q) => ({
          question: q.text.trim(),
          // isCorrect er på den OPRINDELIGE indeksposition, så tomme muligheder ikke skifter index
          answerOptions: q.options
            .map((opt, i) => ({ answer: opt.trim(), isCorrect: q.correct === i }))
            .filter((opt) => opt.answer !== ''),
        })),
      }
      const dto = await createQuizOnApi(payload)

      // QuizDto → lokal quiz-shape (behold pakke/meta fra editoren)
      const mapped = quizToLocal(dto, {
        title,
        description: quiz.description ?? '',
        packageId: quiz.packageId ?? 'stor',
        icon: quiz.icon ?? QUIZ_ICONS[(quizzes?.length ?? 0) % QUIZ_ICONS.length],
        tile: quiz.tile ?? TILE_STYLES[(quizzes?.length ?? 0) % TILE_STYLES.length],
      })

      setQuizzes((list) => {
        const base = list ?? []
        return [mapped, ...base.filter((it) => it.id !== localId)]
      })
      setDraft(null)
      navigate('/dashboard')
    },
    [navigate, user, quizzes],
  )

  const duplicateQuiz = useCallback(
    async (quizId) => {
      const src = (quizzes ?? []).find((it) => it.id === quizId)
      if (!src) return

      // API-oprettede quizzer (id 'api-N'): gem kopien på serveren via saveQuiz
      // (uden id → gemmer som ny quiz; validering/fejl håndteres dér)
      if (String(quizId).startsWith(API_PREFIX)) {
        try {
          await saveQuiz({ ...src, id: null, title: `${src.title} (kopi)` })
        } catch (err) {
          setQuizzesError(err?.message || 'Kunne ikke duplikere quizzen.')
        }
        return
      }

      // Lokal quiz (mock/seed): lokal kopi uden serveroprettelse
      setQuizzes((list) => {
        if (!list) return list
        const idx = list.findIndex((it) => it.id === quizId)
        if (idx < 0) return list
        const s = list[idx]
        const copy = { ...s, id: `quiz-${Date.now()}`, title: `${s.title} (kopi)` }
        copy.icon = QUIZ_ICONS[(list.length + 1) % QUIZ_ICONS.length]
        copy.tile = TILE_STYLES[(list.length + 1) % TILE_STYLES.length]
        const next = [...list]
        next.splice(idx + 1, 0, copy)
        return next
      })
    },
    [quizzes, saveQuiz],
  )

  const deleteQuiz = useCallback(
    async (quizId) => {
      // API-oprettede quizzer (id 'api-N') slettes server-side først
      if (String(quizId).startsWith(API_PREFIX)) {
        const serverId = Number(String(quizId).slice(API_PREFIX.length))
        try {
          await deleteQuizOnApi(serverId)
        } catch (err) {
          if (err.status !== 404) {
            // 404 = allerede væk på serveren → slet den stadig lokalt
            setQuizzesError('Kunne ikke slette quizzen på serveren.')
            console.warn('Sletning af quiz fejlede:', err)
            return
          }
        }
      }
      setQuizzes((list) => (list ? list.filter((it) => it.id !== quizId) : list))
    },
    [],
  )

  /* ---------------- Deltager (spiller) ---------------- */

  const startJoin = useCallback(
    async (code) => {
      if (USE_SIGNALR) {
        // LIVE: forbind til hubben; JoinLobby kaldes i setPlayerName når navnet er valgt.
        stopHub()
        const hub = createQuizHub()
        hubRef.current = hub

        try {
          await hub.connect()
        } catch (err) {
          console.error('Kunne ikke forbinde til quizserveren', err)
          stopHub()
          return
        }
        wirePlayerHandlers(hub, code)

        setPlayer({
          roomId: code,
          quizId: null, // serveren fortæller quiz-indholdet via QuestionChanged
          quizTitle: null,
          name: null,
          index: 0,
          total: 0,
          phase: 'name', // 'name' | 'countdown' | 'question' | 'reveal' | 'results'
          question: null,
          chosenIdx: null,
          chosenOptionId: null,
          answeredSecondsLeft: 0,
          correct: null,
          correctOptionId: null,
          lastPoints: 0,
          score: 0,
          scores: {},
          others: [],
          joinError: null,
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
    [quizzes, navigate, stopHub, wirePlayerHandlers],
  )

  const setPlayerName = useCallback(
    async (name) => {
      const clean = name.trim() || 'Gæst'

      // LIVE: JoinLobby(roomId, name) – værten ser deltagere via LobbyUpdated.
      if (USE_SIGNALR && hubRef.current && player?.live && player.roomId) {
        try {
          await hubRef.current.joinLobby(player.roomId, clean)
          setPlayer((p) =>
            p && p.roomId === player.roomId
              ? { ...p, name: clean, phase: 'countdown', score: 0, joinError: null }
              : p,
          )
          navigate('/spil')
        } catch (err) {
          // Typisk: navnet er optaget, eller quizzen er allerede startet.
          setPlayer((p) =>
            p && p.roomId === player.roomId
              ? { ...p, joinError: err?.message?.replace(/^An unexpected error occurred[^:]*: /, '') || 'Kunne ikke deltage' }
              : p,
          )
        }
        return
      }

      setPlayer((p) => (p ? { ...p, name: clean, scores: { ...p.scores, [clean]: 0 }, phase: 'countdown' } : p))
      navigate('/spil')
    },
    [navigate, player],
  )

  // LIVE-only: send svar til serveren (serveren registrerer svaret; point beregnes i UI'et).
  const submitPlayerAnswer = useCallback(
    async (i, secondsLeft = 0) => {
      const p = player
      const hub = hubRef.current
      if (!(USE_SIGNALR && hub && p?.live && p.question)) return null
      const optionId = p.question.optionIds[i]

      // Spørgsmålet deadline: gem hvor epidet var tilbage (bruges til hastighedsbonus ved reveal).
      setPlayer((cur) =>
        cur && cur.roomId === p.roomId && cur.question?.id === p.question.id
          ? { ...cur, chosenOptionId: optionId, answeredSecondsLeft: Math.max(0, secondsLeft ?? 0) }
          : cur,
      )
      // Afsend til serveren (hub'ens returnværdi bruges ikke – den serialiseres ikke pålideligt).
      hub.submitAnswer(p.roomId, p.question.id, optionId).catch(() => {})
      return null
    },
    [player],
  )

  const resetPlayer = useCallback(() => {
    setPlayer(null)
    if (USE_SIGNALR) stopHub()
    navigate('/deltag')
  }, [navigate, stopHub])

  const value = useMemo(
    () => ({
      user,
      quizzes,
      quizzesLoading,
      quizzesError,
      refreshQuizzes,
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
      openQuestion,
      closeQuestion,
      nextQuestion,
      startQuizDraft,
      openEditor,
      clearDraft,
      saveQuiz,
      duplicateQuiz,
      deleteQuiz,
      // deltager
      startJoin,
      setPlayerName,
      submitPlayerAnswer,
      resetPlayer,
    }),
    [user, quizzes, quizzesLoading, quizzesError, refreshQuizzes, room, player, draft, login, logout, startSession, addParticipant, removeParticipant, launchQuiz, openQuestion, closeQuestion, nextQuestion, startQuizDraft, openEditor, clearDraft, saveQuiz, duplicateQuiz, deleteQuiz, startJoin, setPlayerName, submitPlayerAnswer, resetPlayer],
  )

  return <GameContext.Provider value={value}>{children}</GameContext.Provider>
}
