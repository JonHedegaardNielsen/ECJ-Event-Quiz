// Fælles 3-2-1-nedtælling mellem spørgsmål. Kalder onDone når den lander på 1.
import { useEffect, useState } from 'react'

export default function Countdown({ top, hint, onDone }) {
  const [count, setCount] = useState(3)

  useEffect(() => {
    let cur = 3
    const interval = setInterval(() => {
      cur -= 1
      setCount(Math.max(cur, 0))
      if (cur <= 0) {
        clearInterval(interval)
        onDone?.()
      }
    }, 800)
    return () => clearInterval(interval)
  }, [onDone])

  return (
    <main className="live-count">
      {top}
      <span key={count} className="big-num">
        {Math.max(count, 1)}
      </span>
      {hint && <p className="count-sub">{hint}</p>}
    </main>
  )
}
