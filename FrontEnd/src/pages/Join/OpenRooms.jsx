// Deltager-forside: oversigt over åbne lobby-rum i stedet for login/dashboard.
import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import Logo from '../../components/Logo'
import { listOpenRooms } from '../../lib/api'
import { useGame } from '../../game/useGame'
import './OpenRooms.css'

export default function OpenRooms() {
  const { startJoin } = useGame()
  const [rooms, setRooms] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [joining, setJoining] = useState(null)

  const refresh = useCallback(async () => {
    setError(null)
    try {
      const data = await listOpenRooms()
      setRooms(Array.isArray(data) ? data : [])
    } catch (err) {
      setError(err?.message || 'Kunne ikke hente åbne rum.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    refresh()
    const id = setInterval(refresh, 4000)
    return () => clearInterval(id)
  }, [refresh])

  const join = async (roomId) => {
    setJoining(roomId)
    setError(null)
    const err = await startJoin(roomId)
    if (err) setError(err)
    setJoining(null)
  }

  return (
    <main className="page-open-rooms">
      <div className="open-rooms-card">
        <Logo size="lg" />
        <h1>Åbne quizzer</h1>
        <p className="open-rooms-lead">
          Vælg et rum for at deltage, eller indtast et Room ID manuelt.
        </p>

        {loading && rooms.length === 0 && <p className="open-rooms-muted">Henter åbne rum…</p>}
        {error && <p className="open-rooms-error" role="alert">{error}</p>}

        {!loading && rooms.length === 0 && !error && (
          <p className="open-rooms-empty">Ingen åbne rum lige nu. Prøv igen om lidt, eller indtast et Room ID.</p>
        )}

        <ul className="open-rooms-list">
          {rooms.map((room) => (
            <li key={room.roomId ?? room.RoomId} className="open-room-row">
              <div>
                <strong>{room.quizName ?? room.QuizName}</strong>
                <span>
                  Room ID {(room.roomId ?? room.RoomId)} · {room.participantCount ?? room.ParticipantCount ?? 0} deltagere
                </span>
              </div>
              <button
                type="button"
                className="btn"
                disabled={joining === (room.roomId ?? room.RoomId)}
                onClick={() => join(room.roomId ?? room.RoomId)}
              >
                {joining === (room.roomId ?? room.RoomId) ? 'Tilmelder…' : 'Deltag'}
              </button>
            </li>
          ))}
        </ul>

        <div className="open-rooms-actions">
          <Link className="btn btn-ghost" to="/deltag">
            Indtast Room ID
          </Link>
          <button type="button" className="link-ghost" onClick={refresh}>
            Opdater liste
          </button>
        </div>

        <Link className="open-rooms-host" to="/login">
          Er du arrangør? Log ind
        </Link>
      </div>
    </main>
  )
}
