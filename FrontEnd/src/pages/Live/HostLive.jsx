// 6. Live spørgsmål for værten – LIVE: server-drevet flow via SignalR; MOCK: simulerede deltagere.
// Deler layout + CSS med deltagerens spil-skærm (PlayerLive).
import { useCallback, useEffect, useRef, useState } from 'react'
import Logo from '../../components/Logo'
import Countdown from '../../components/Countdown'
import QuestionMedia from '../../components/QuestionMedia'
import AnswerButton from '../../components/AnswerButton'
import { IconClock } from '../../components/icons'
import { useGame } from '../../game/useGame'
import { ANSWER_COLORS, getQuiz } from '../../data/mock'
import { otherIndex, pointsFor } from '../../lib/utils'
import { useNavigate } from 'react-router-dom'
import './Live.css'

export default function HostLive() {
  const { room, quizzes } = useGame()

  if (!room) return null

  if (room.live) return <LiveHostView room={room} />

  return <MockHostLive room={room} quiz={getQuiz(quizzes ?? [], room.quizId)} />
}

/* ---------------- LIVE: server-drevet værtsskærm ---------------- */

function LiveHostView({ room }) {
  const { openQuestion, closeQuestion, nextQuestion } = useGame()

  if (room.status === 'results') return null

  const q = room.question
  if (!q) return null

  if (room.status === 'countdown') {
    return (
      <Countdown
        key={`cd-${room.index}`}
        onDone={() => openQuestion(room.id)}
        top={<Logo size="md" />}
        hint={`Spørgsmål ${room.index + 1} af ${room.total || q.total}`}
      />
    )
  }

  return (
    <LiveHostQuestion
      key={`q-${room.index}`}
      room={room}
      onClose={closeQuestion}
      onNext={nextQuestion}
    />
  )
}

function LiveHostQuestion({ room, onClose, onNext }) {
  const q = room.question
  const total = room.total || q.total
  const isReveal = room.status === 'reveal'
  const [seconds, setSeconds] = useState(q.time)
  const closedRef = useRef(false)
  const nextRef = useRef(false)
  const nextTimer = useRef(null)

  // Hvis svarvinduet udløber: luk spørgsmålet (serveren afslører til alle).
  useEffect(() => {
    if (isReveal || closedRef.current) return undefined
    let left = q.time
    const iv = setInterval(() => {
      left -= 1
      setSeconds(Math.max(left, 0))
      if (left <= 0 && !closedRef.current) {
        clearInterval(iv)
        closedRef.current = true
        onClose(room.id)
      }
    }, 1000)
    return () => clearInterval(iv)
  }, [q.time, isReveal, room.id, onClose])

  // Efter afsløring spørger værten serveren om næste spørgsmål – eller QuizEnded.
  useEffect(() => {
    if (!isReveal || nextRef.current) return undefined
    nextRef.current = true
    nextTimer.current = setTimeout(() => {
      nextRef.current = false
      // serveren sender enten ny QuestionChanged (countdown) eller QuizEnded
      onNext(room.id)
    }, 2400)
    return () => clearTimeout(nextTimer.current)
  }, [isReveal, room.id, q.time, onNext])
  useEffect(() => () => clearTimeout(nextTimer.current), [])

  const answered = (room.answerCounts ?? []).reduce((a, b) => a + b, 0)

  return (
    <main className="live">
      <header className="live-head">
        <Logo size="md" />
        <span className="live-count-num">
          {room.index + 1} / {total}
        </span>
      </header>
      <div className="live-progress">
        <span style={{ width: `${((room.index + 1) / total) * 100}%` }} />
      </div>

      <div className="live-main">
        <QuestionMedia question={q} />
        <div className="qcard">
          <h1>{q.text}</h1>
        </div>
        <div className="answers" aria-label="Svarmuligheder">
          {q.options.map((opt, i) => (
            <AnswerButton
              key={q.optionIds[i]}
              index={i}
              color={ANSWER_COLORS[i]}
              option={opt}
              count={room.answerCounts?.[i] ?? 0}
              state={isReveal && room.correctIdx === i ? 'correct' : null}
            />
          ))}
        </div>
        <div className="live-foot">
          <span className="live-timer">
            <IconClock size={18} />
            {Math.max(seconds, 0)} sek.
          </span>
          <span className="live-answered">
            {answered}/{room.participants.length} har svaret
          </span>
        </div>
      </div>
    </main>
  )
}

/* ---------------- MOCK: simulerede deltagere (VITE_USE_SIGNALR=false) ---------------- */

function MockHostLive({ room, quiz }) {
  const { setRoom } = useGame()
  const navigate = useNavigate()
  const total = quiz?.questions?.length ?? 0

  const roomRef = useRef(null)
  useEffect(() => {
    roomRef.current = room
  })

  const beginQuestion = useCallback(() => {
    setRoom((r) => (r && r.status === 'countdown' ? { ...r, status: 'question' } : r))
  }, [setRoom])

  const revealQuestion = useCallback(() => {
    setRoom((r) => (r && r.status === 'question' ? { ...r, status: 'reveal' } : r))
  }, [setRoom])

  /* Når et spørgsmål er afsløret: næste spørgsmål – eller resultaterne */
  const roomStatus = room?.status
  useEffect(() => {
    if (roomStatus !== 'reveal') return undefined
    const t = setTimeout(() => {
      const r = roomRef.current
      if (!r) return
      const nextIndex = r.index + 1
      if (nextIndex >= total) {
        setRoom((cur) => (cur ? { ...cur, status: 'results' } : cur))
        navigate('/resultat')
      } else {
        setRoom((cur) => (cur ? { ...cur, status: 'countdown', index: nextIndex, answerCounts: [0, 0, 0, 0] } : cur))
      }
    }, 2400)
    return () => clearTimeout(t)
  }, [roomStatus, total, setRoom, navigate])

  if (!quiz || total === 0) return null
  if (room.status === 'results') return null
  const question = quiz.questions[room.index]
  if (!question) return null

  if (room.status === 'countdown') {
    return (
      <Countdown
        key={`cd-${room.index}`}
        onDone={beginQuestion}
        top={<Logo size="md" />}
        hint={`Spørgsmål ${room.index + 1} af ${total}`}
      />
    )
  }

  return (
    <HostQuestionRunner
      key={`q-${room.index}`}
      question={question}
      participants={room.participants}
      isReveal={room.status === 'reveal'}
      answerCounts={room.answerCounts}
      index={room.index}
      total={total}
      onReveal={revealQuestion}
    />
  )
}

function HostQuestionRunner({ question, participants, isReveal, answerCounts, index, total, onReveal }) {
  const { room, setRoom } = useGame()
  const [seconds, setSeconds] = useState(question.time)
  const ids = useRef([])

  const clearAll = () => {
    ids.current.forEach((id) => {
      clearTimeout(id)
      clearInterval(id)
    })
    ids.current = []
  }
  useEffect(() => () => clearAll(), [])

  useEffect(() => {
    if (isReveal) clearAll()
  }, [isReveal])

  /* Mock-deltagerne svarer med forskellige tider; uret tick-ned; + afsløring */
  useEffect(() => {
    clearAll()
    const time = question.time
    let left = time
    const localCounts = [0, 0, 0, 0]

    participants.forEach((name) => {
      const correct = Math.random() < 0.72
      const delay = 1.4 + Math.random() * Math.max(1, time - 3)
      const option = correct ? question.correct : otherIndex(question.correct, question.options.length)
      const t = setTimeout(() => {
        localCounts[option] += 1
        const allAnswered = localCounts.reduce((a, b) => a + b, 0) >= participants.length
        setRoom((r) => {
          if (!r || r.status !== 'question') return r
          const scores = { ...r.scores }
          if (correct) scores[name] = (scores[name] || 0) + pointsFor(Math.max(0, time - delay), time)
          return { ...r, answerCounts: localCounts.slice(), scores }
        })
        if (allAnswered) onReveal()
      }, delay * 1000)
      ids.current.push(t)
    })

    const iv = setInterval(() => {
      left -= 1
      setSeconds(Math.max(left, 0))
      if (left <= 0) {
        onReveal()
      }
    }, 1000)
    ids.current.push(iv)

    return () => clearAll()
  }, [question, participants, onReveal, setRoom])

  const answered = answerCounts.reduce((a, b) => a + b, 0)

  return (
    <main className="live">
      <header className="live-head">
        <Logo size="md" />
        <span className="live-count-num">
          {index + 1} / {total}
        </span>
      </header>
      <div className="live-progress">
        <span style={{ width: `${((index + 1) / total) * 100}%` }} />
      </div>

      <div className="live-main">
        <QuestionMedia question={question} />
        <div className="qcard">
          <h1>{question.text}</h1>
        </div>
        <div className="answers" aria-label="Svarmuligheder">
          {question.options.map((opt, i) => (
            <AnswerButton
              key={i}
              index={i}
              color={ANSWER_COLORS[i]}
              option={opt}
              count={answerCounts[i]}
              state={isReveal && i === question.correct ? 'correct' : null}
            />
          ))}
        </div>
        <div className="live-foot">
          <span className="live-timer">
            <IconClock size={18} />
            {Math.max(seconds, 0)} sek.
          </span>
          <span className="live-answered">
            {answered}/{room?.participants.length ?? 0} har svaret
          </span>
        </div>
      </div>
    </main>
  )
}
