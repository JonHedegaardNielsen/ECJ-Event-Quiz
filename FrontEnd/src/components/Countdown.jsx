// Fælles 3-2-1-nedtælling mellem spørgsmål. Kalder onDone når den lander på 1.
import { useEffect, useState } from 'react'

export default function Countdown({ top, hint, onDone }) {
  const [n, setN] = useState(3)

  useEffect(() => {
    let cur = 3
    const iv = setInterval(() => {
      cur -= 1
      setN(Math.max(cur, 0))
      if (cur <= 0) {
        clearInterval(iv)
        onDone?.()
      }
    }, 800)
    return () => clearInterval(iv)
  }, [onDone])

  return (
    <main className="live-count">
      {top}
      <span key={n} className="big-num">
        {Math.max(n, 1)}
      </span>
      {hint && <p className="count-sub">{hint}</p>}
    </main>
  )
}
