// Venstremenu til host-flowet (Dashboard / Mine quizzer / Opret quiz / Indstillinger / Log ud).
import Logo from './Logo'
import { IconHome, IconList, IconPlusCircle, IconGear, IconLogout } from './icons'
import { useGame } from '../game/useGame'
import { useRoute, navigate } from '../lib/router'

export default function Sidebar() {
  const { logout } = useGame()
  const path = useRoute()
  const isDash = path === '/dashboard'
  const isOpret = path === '/opret'

  return (
    <aside className="sidebar">
      <div className="sidebar-logo">
        <Logo size="sm" />
      </div>
      <button type="button" className={`sidebar-item${isDash ? ' active' : ''}`} onClick={() => navigate('/dashboard')}>
        <IconHome size={19} />
        Dashboard
      </button>
      <button type="button" className="sidebar-item" disabled title="Kommer snart">
        <IconGear size={19} />
        Indstillinger
      </button>
      <div className="sidebar-bottom">
        <button type="button" className="sidebar-item" onClick={logout}>
          <IconLogout size={19} />
          Log ud
        </button>
      </div>
    </aside>
  )
}
