// 4. Lobby (arrangør): Room ID, QR-kode og deltagere, der tilslutter sig (simuleret).
import { useEffect } from 'react'
import Logo from '../components/Logo'
import QrCode from '../components/QrCode'
import Avatar from '../components/Avatar'
import { IconArrowLeft, IconX } from '../components/icons'
import { useGame } from '../game/useGame'
import { packageInfo, getQuiz } from '../data/mock'
import { pickBots } from '../lib/utils'
import './Lobby.css'

export default function Lobby() {
  const { room, quizzes, addParticipant, removeParticipant, launchQuiz } = useGame()
  const quiz = getQuiz(quizzes ?? [], room?.quizId)

  // Demo: mock-deltagere tilslutter sig automatisk. Rigtige deltagere via WebSocket senere.
  useEffect(() => {
    const names = pickBots(6)
    const timers = names.map((name, i) => setTimeout(() => addParticipant(name), i * 950 + 500))
    return () => timers.forEach(clearTimeout)
  }, [addParticipant])

  if (!room || !quiz) return null

  const pkg = packageInfo(quiz.packageId)
  const canStart = room.participants.length > 0

  return (
    <main className="page-lobby">
      <div className="lobby-wrap">
        <div className="lobby-top">
          <Logo size="sm" />
          <button type="button" className="link-ghost" onClick={() => window.history.back()}>
            <IconArrowLeft size={16} />
            Tilbage
          </button>
        </div>

        <div className="lobby-head">
          <h1>{quiz.title}</h1>
          <p>Lobby</p>
        </div>

        <section className="lobby-card room-card">
          <div className="room-id">
            <span className="room-label">Room ID</span>
            <span className="room-digits">{room.id}</span>
          </div>
          <div className="lobby-qr">
            <span className="qr-box">
              <QrCode seed={room.id} />
            </span>
            <span>Scan QR-kode</span>
          </div>
        </section>

        <section className="lobby-card">
          <div className="p-head">
            <span className="dot-live" aria-hidden="true" />
            Deltagere ({room.participants.length}/{pkg.cap})
          </div>
          <div className="p-list">
            {room.participants.map((name) => (
              <div className="p-row" key={name}>
                <Avatar size={28} />
                <span>{name}</span>
                <button
                  type="button"
                  className="p-x"
                  aria-label={`Fjern ${name}`}
                  onClick={() => removeParticipant(name)}
                >
                  <IconX size={14} />
                </button>
              </div>
            ))}
            {room.participants.length === 0 && <p className="p-empty">Venter på deltagere…</p>}
          </div>
        </section>

        <button type="button" className="btn btn-green btn-lg btn-block" onClick={launchQuiz} disabled={!canStart}>
          Start quiz
        </button>
        {!canStart && <p className="lobby-hint">Demo: der tilsluttes testdeltagere automatisk.</p>}
      </div>
    </main>
  )
}
