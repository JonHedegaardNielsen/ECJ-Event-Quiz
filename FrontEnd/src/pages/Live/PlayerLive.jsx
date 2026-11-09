// Deltagerens live-spil: 3-2-1 → spørgsmål → afsløring → næste → resultater.
// LIVE: serveren driver faserne via SignalR. MOCK: simulerede bots + lokal rytme.
// Layoutet spejler mockup 6/7 (og 8 på mobil).
import { useCallback, useEffect, useRef, useState } from 'react'
import Logo from '../../components/Logo'
import Countdown from '../../components/Countdown'
import Avatar from '../../components/Avatar'
import QuestionMedia from '../../components/QuestionMedia'
import AnswerButton from '../../components/AnswerButton'
import { IconClock } from '../../components/icons'
import { useGame } from '../../game/useGame'
import { ANSWER_COLORS, getQuiz } from '../../data/mock'
import { pointsFor } from '../../lib/utils'
import './Live.css'

export default function PlayerLive() {
  const { player, quizzes, setPlayer } = useGame()

  if (!player) return null

  if (player.live) return <LivePlayerView player={player} />

  return <MockPlayerLive player={player} quizzes={quizzes} setPlayer={setPlayer} />
}

/* ---------------- LIVE: server-drevet deltagerskærm ---------------- */

function LivePlayerView({ player }) {
  const { submitPlayerAnswer } = useGame()

  if (player.phase === 'name' || player.phase === 'results') return null

  const q = player.question

  if (player.phase === 'countdown') {
    if (!q) return null // spørgsmålet kommer fra serveren via QuestionChanged
    const first = player.index === 0
    return (
      <Countdown
        key={`cd-${player.index}`}
        onDone={() => {}}
        top={
          first ? (
            <>
              <Logo size="lg" />
              <div className="joined">
                <Avatar size={76} />
              </div>
              <p className="joined-title">Du deltager nu i quizzen</p>
            </>
          ) : (
            <Logo size="lg" />
          )
        }
        hint={first ? 'Gør plads – quizzen starter!' : `Spørgsmål ${player.index + 1} af ${player.total}`}
      />
    )
  }

  if (!q) return null
  return <LivePlayerQuestion key={`q-${player.index}`} player={player} onSubmit={submitPlayerAnswer} />
}

function LivePlayerQuestion({ player, onSubmit }) {
  const q = player.question
  const isReveal = player.phase === 'reveal'
  const [chosen, setChosen] = useState(null)
  const [seconds, setSeconds] = useState(q.time)
  const chosenRef = useRef(null)

  // Nedtælling i svarvinduet (serveren afslører – hosten lukker spørgsmålet).
  useEffect(() => {
    if (isReveal) return undefined
    let left = q.time
    const iv = setInterval(() => {
      left -= 1
      setSeconds(Math.max(left, 0))
    }, 1000)
    return () => clearInterval(iv)
  }, [q.time, isReveal])

  const choose = (i) => {
    if (chosen != null || isReveal) return
    setChosen(i)
    chosenRef.current = i
    onSubmit(i, seconds)
  }

  // Korrekthed: QuestionClosed-svaret fra serveren (evt. forbedret af SubmitAnswer-svaret).
  const correct = player.correct
  const revealOptionIdx = player.question.optionIds.indexOf(player.correctOptionId ?? null)
  const chosenShown = chosen ?? player.chosenIdx

  return (
    <main className="live">
      <header className="live-head">
        <Logo size="md" />
        <span className="live-count-num">
          {player.index + 1} / {player.total}
        </span>
      </header>
      <div className="live-progress">
        <span style={{ width: `${((player.index + 1) / player.total) * 100}%` }} />
      </div>

      <div className="live-main">
        <QuestionMedia question={q} />
        <div className="qcard">
          <h1>{q.text}</h1>
        </div>
        <div className="answers" aria-label="Svarmuligheder">
          {q.options.map((opt, i) => {
            const pickedState = chosen === i && !isReveal ? 'picked' : null
            const revealState = isReveal
              ? i === revealOptionIdx
                ? 'correct'
                : chosenShown === i
                  ? 'wrong'
                  : null
              : null
            return (
              <AnswerButton
                key={q.optionIds[i]}
                index={i}
                color={ANSWER_COLORS[i]}
                option={opt}
                state={revealState ?? pickedState}
                clickable={!isReveal && chosen == null}
                onClick={() => choose(i)}
              />
            )
          })}
        </div>
        {isReveal && (
          <div className={`reveal-chip${correct ? '' : ' bad'}`}>
            {player.chosenOptionId == null
              ? 'Tid udløb – 0 point'
              : correct
                ? `+${player.lastPoints ?? 0} point`
                : 'Forkert – 0 point'}
          </div>
        )}
        <div className="live-foot">
          <span className="live-timer">
            <IconClock size={18} />
            {Math.max(seconds, 0)} sek.
          </span>
          <span className="live-answered">
            {player.name} · {player.score ?? 0} point
          </span>
        </div>
      </div>
    </main>
  )
}

/* ---------------- MOCK: simulerede bots (VITE_USE_SIGNALR=false) ---------------- */

function MockPlayerLive({ player, quizzes, setPlayer }) {
  const { finishPlayer } = useGame()
  const quiz = getQuiz(quizzes ?? [], player.quizId)
  const total = quiz?.questions?.length ?? 0

  const beginQuestion = () => {
    setPlayer((p) => (p && p.phase === 'countdown' ? { ...p, phase: 'question' } : p))
  }

  if (!quiz || total === 0) return null

  if (player.phase === 'countdown') {
    const first = player.index === 0
    return (
      <Countdown
        key={`cd-${player.index}`}
        onDone={beginQuestion}
        top={
          first ? (
            <>
              <Logo size="lg" />
              <div className="joined">
                <Avatar size={76} />
              </div>
              <p className="joined-title">Du deltager nu i {quiz.title}</p>
            </>
          ) : (
            <Logo size="lg" />
          )
        }
        hint={first ? 'Gør plads – quizzen starter!' : `Spørgsmål ${player.index + 1} af ${total}`}
      />
    )
  }

  if (player.phase === 'results' || player.phase === 'name') return null
  const question = quiz.questions[player.index]
  if (!question) return null

  return (
    <PlayerQuestionRunner
      key={`q-${player.index}`}
      question={question}
      index={player.index}
      total={total}
      quizTitle={quiz.title}
      player={player}
      setPlayer={setPlayer}
      finishPlayer={finishPlayer}
    />
  )
}

function PlayerQuestionRunner({ question, index, total, quizTitle, player, setPlayer, finishPlayer }) {
  const [chosen, setChosen] = useState(null)
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

  const isReveal = player?.phase === 'reveal'
  const timedOut = isReveal && player?.chosenIdx === -1

  const advance = useCallback(() => {
    const nextIndex = index + 1
    if (nextIndex >= total) {
      finishPlayer()
    } else {
      setPlayer((cur) =>
        cur ? { ...cur, phase: 'countdown', index: nextIndex, chosenIdx: null, correct: null, lastPoints: 0 } : cur,
      )
    }
  }, [index, total, finishPlayer, setPlayer])

  /* Efter afsløring: videre automatisk */
  useEffect(() => {
    if (!isReveal) return undefined
    const t = setTimeout(advance, 1900)
    return () => clearTimeout(t)
  }, [isReveal, advance])

  /* Spørgsmålet kører: nedtælling */
  useEffect(() => {
    if (isReveal) {
      clearAll()
      return undefined
    }
    clearAll()
    const time = question.time
    let left = time

    const timeoutReveal = () => {
      clearAll()
      setPlayer((p) => {
        if (!p || p.phase !== 'question') return p
        const scores = bumpBots(p, question)
        return { ...p, chosenIdx: -1, correct: false, lastPoints: 0, phase: 'reveal', scores }
      })
    }

    const iv = setInterval(() => {
      left -= 1
      setSeconds(Math.max(left, 0))
      if (left <= 0) timeoutReveal()
    }, 1000)
    ids.current.push(iv)

    return () => clearAll()
  }, [question, isReveal, setPlayer])

  const choose = (i) => {
    if (chosen != null || isReveal) return
    setChosen(i)
    const correct = i === question.correct
    const earned = correct ? pointsFor(Math.max(seconds, 0), question.time) : 0
    clearAll() // stop uret mens afsløringen venter
    const t = setTimeout(() => {
      setPlayer((p) => {
        if (!p || p.phase !== 'question') return p
        const scores = bumpBots(p, question)
        scores[p.name] = (scores[p.name] || 0) + earned
        return { ...p, chosenIdx: i, correct, lastPoints: earned, phase: 'reveal', scores }
      })
    }, 480)
    ids.current.push(t)
  }

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
          {question.options.map((opt, i) => {
            const pickedState = chosen === i && !isReveal ? 'picked' : null
            const revealState = isReveal
              ? i === question.correct
                ? 'correct'
                : chosen === i
                  ? 'wrong'
                  : null
              : null
            return (
              <AnswerButton
                key={i}
                index={i}
                color={ANSWER_COLORS[i]}
                option={opt}
                state={revealState ?? pickedState}
                clickable={!isReveal && chosen == null}
                onClick={() => choose(i)}
              />
            )
          })}
        </div>
        {isReveal && (
          <div className={`reveal-chip${player?.correct ? '' : ' bad'}`}>
            {timedOut
              ? 'Tid udløb – 0 point'
              : player?.correct
                ? `+${player?.lastPoints ?? 0} point`
                : 'Forkert – 0 point'}
          </div>
        )}
        <div className="live-foot">
          <span className="live-timer">
            <IconClock size={18} />
            {Math.max(seconds, 0)} sek.
          </span>
          <span className="live-answered">
            {player?.name} · {quizTitle}
          </span>
        </div>
      </div>
    </main>
  )
}

/* Mock: de øvrige "deltagere" scorer point parallelt med spilleren */
function bumpBots(p, question) {
  const scores = { ...p.scores }
  p.others.forEach((bot) => {
    if (Math.random() < 0.72) {
      scores[bot] = (scores[bot] || 0) + pointsFor(Math.round(Math.random() * question.time), question.time)
    }
  })
  return scores
}
