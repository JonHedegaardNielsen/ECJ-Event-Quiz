// Rund avatar med person-ikon.
import { IconUser } from './icons'

export default function Avatar({ size = 34 }) {
  return (
    <span className="avatar" style={{ width: size, height: size }}>
      <IconUser size={Math.round(size * 0.55)} />
    </span>
  )
}
