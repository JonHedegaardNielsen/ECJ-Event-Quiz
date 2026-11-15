// 5c. Venterum (deltager): tilmeldt og venter på, at værten starter quizzen.
// LIVE: serveren sender QuestionChanged (countdown) → videre til /spil automatisk.
import { useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import Logo from '../../components/Logo'
import Avatar from '../../components/Avatar'
import { IconX } from '../../components/icons'
import { useGame } from '../../game/useGame'
import './Waiting.css'

export default function Waiting() {
  const { player, resetPlayer } = useGame()
  const navigate = useNavigate()

  // QuizStarted: første QuestionChanged (countdown) betyder, at quizzen er i gang.
  useEffect(() => {
    if (!player) return undefined
    if (player.phase === 'countdown' || player.phase === 'question') {
      navigate('/spil', { replace: true })
    }
    return undefined
  }, [player, navigate])

  if (!player) return null

  const others = (player.others ?? []).filter((name) => name !== player.name)

  return (
    <main className="page-waiting">
      <Logo size="lg" />
      <section className="waiting-card" aria-live="polite">
        <div className="waiting-avatar">
          <Avatar size={76} />
        </div>
        <p className="waiting-hi">Du er med, {player.name}!</p>
        <p className="waiting-room">Room {player.roomId}</p>

        <div className="waiting-dots" aria-hidden="true">
          <span />
          <span />
          <span />
        </div>
        <p className="waiting-text">Venter på at værten starter quizzen…</p>

        {others.length > 0 && (
          <div className="waiting-players">
            <div className="waiting-players-head">
              <span className="waiting-dot" aria-hidden="true" />
              {others.length + 1} spillere er med
            </div>
            <div className="waiting-players-list">
              {others.map((name) => (
                <div className="waiting-player" key={name}>
                  <Avatar size={26} />
                  <span>{name}</span>
                </div>
              ))}
            </div>
          </div>
        )}

        <button type="button" className="waiting-leave" onClick={resetPlayer}>
          <IconX size={13} />
          Forlad quizzen
        </button>
      </section>
    </main>
  )
}
