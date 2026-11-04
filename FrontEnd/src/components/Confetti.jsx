// Animeret konfetti til resultat-skærmen. Deterministisk "random" (ren render).
import { useMemo } from 'react'

const COLORS = ['#e2364c', '#1368ce', '#2e9e44', '#e3b53b', '#ffffff', '#38bdf8']

function mix(i, salt) {
  let x = Math.imul(i + 1, 2654435761) ^ Math.imul(salt + 7, 40503)
  x ^= x >>> 13
  x = Math.imul(x, 1274126177)
  x ^= x >>> 16
  return (x >>> 0) / 4294967295
}

export default function Confetti({ pieces = 40 }) {
  const list = useMemo(
    () =>
      Array.from({ length: pieces }, (_, i) => ({
        id: i,
        left: mix(i, 1) * 100,
        delay: mix(i, 2) * 3,
        dur: 2.8 + mix(i, 3) * 2.8,
        size: 6 + mix(i, 4) * 7,
        color: COLORS[i % COLORS.length],
        round: mix(i, 5) > 0.7,
      })),
    [pieces],
  )
  return (
    <div className="confetti" aria-hidden="true">
      {list.map((p) => (
        <i
          key={p.id}
          style={{
            left: `${p.left}%`,
            width: p.size,
            height: p.round ? p.size : Math.round(p.size * 0.55),
            background: p.color,
            borderRadius: p.round ? '50%' : 2,
            animationDelay: `${p.delay}s`,
            animationDuration: `${p.dur}s`,
          }}
        />
      ))}
    </div>
  )
}
