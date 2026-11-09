// ECJ EventQuiz – app-rod: provider + react-router.
import { Navigate, Route, Routes } from 'react-router-dom'
import { GameProvider } from './game/GameContext'
import { useGame } from './game/useGame'
import Login from './pages/Login/Login'
import Dashboard from './pages/Dashboard/Dashboard'
import CreateQuiz from './pages/CreateQuiz/CreateQuiz'
import Lobby from './pages/Lobby/Lobby'
import HostLive from './pages/Live/HostLive'
import Results from './pages/Results/Results'
import Join from './pages/Join/Join'
import JoinName from './pages/Join/JoinName'
import PlayerLive from './pages/Live/PlayerLive'

function Router() {
  const { user, room, player } = useGame()

  // Hjælpe-guard: vis `node` hvis betingelsen er opfyldt, ellers omdiriger.
  const guard = (ok, to, node) => (ok ? node : <Navigate replace to={to} />)

  return (
    <Routes>
      {/* deltagerflow (altid tilgængelig) */}
      <Route path="/deltag" element={<Join />} />
      <Route path="/deltag/navn" element={guard(player, '/deltag', <JoinName />)} />
      <Route path="/spil" element={guard(player, '/deltag', <PlayerLive />)} />
      <Route path="/resultat" element={guard(room || player, '/', <Results />)} />

      {/* vært-flow (kræver login) */}
      <Route path="/login" element={guard(!user, '/dashboard', <Login />)} />
      <Route path="/dashboard" element={guard(user, '/login', <Dashboard />)} />
      <Route path="/opret" element={guard(user, '/login', <CreateQuiz />)} />
      <Route path="/lobby" element={guard(user && room, '/dashboard', <Lobby />)} />
      <Route path="/live" element={guard(user && room, '/dashboard', <HostLive />)} />

      {/* forsiden: login hvis ikke logget ind, ellers dashboard */}
      <Route path="/" element={user ? <Navigate replace to="/dashboard" /> : <Login />} />

      {/* ukendte ruter */}
      <Route path="*" element={<Navigate replace to="/" />} />
    </Routes>
  )
}

export default function App() {
  return (
    <GameProvider>
      <Router />
    </GameProvider>
  )
}
