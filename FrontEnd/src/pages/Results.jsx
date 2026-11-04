// 7. Resultatliste (efter quiz) – bruges af både værten og deltageren.
import Confetti from '../components/Confetti'
import { useGame } from '../game/useGame'
import { getQuiz } from '../data/mock'
import './Results.css'

const MEDALS = ['🥇', '🥈', '🥉']
const BORDER = ['#f5c518', '#c7cfda', '#cd8a4b']

export default function Results() {
  const { user, room, player, quizzes, startSession, resetPlayer } = useGame()
  const isHost = Boolean(user && room)

  const quiz = isHost ? getQuiz(quizzes ?? [], room?.quizId) : getQuiz(quizzes ?? [], player?.quizId)
  const scoresMap = isHost ? room?.scores : player?.scores
  const myName = isHost ? null : player?.name

  const rows = Object.entries(scoresMap ?? {})
    .map(([name, points]) => ({ name, points: Math.round(points || 0) }))
    .sort((a, b) => b.points - a.points)

  const restart = () => {
    if (isHost) startSession(quiz)
    else resetPlayer()
  }

  return (
    <main className="page-results">
      <Confetti />
      <span className="trophy" aria-hidden="true">
        🏆
      </span>
      <h1 className="results-title">Quizzen er slut!</h1>
      <p className="results-sub">Her er den endelige rangliste</p>
      {quiz && <p className="results-quiz">{quiz.title}</p>}

      <section className="board" aria-label="Rangliste">
        <div className="board-cols">
          <span>#</span>
          <span>Navn</span>
          <span>Point</span>
        </div>
        {rows.map((row, i) => (
          <div
            className={`b-row${myName && row.name === myName ? ' me' : ''}`}
            key={`${row.name}-${i}`}
            style={{ borderLeftColor: i < 3 ? BORDER[i] : 'transparent' }}
          >
            <span className="b-rank">
              {i < 3 && <span aria-hidden="true">{MEDALS[i]}</span>}
              {i + 1}
            </span>
            <span className="b-name">
              {row.name}
              {myName && row.name === myName && <em> (dig)</em>}
            </span>
            <span className="b-points">{row.points}</span>
          </div>
        ))}
        {rows.length === 0 && <p className="board-empty">Ingen resultater.</p>}
      </section>

      <button type="button" className="btn btn-lg board-restart" onClick={restart}>
        Spil igen
      </button>
      {isHost && (
        <a className="results-back" href="#/dashboard">
          Tilbage til dashboard
        </a>
      )}
    </main>
  )
}
