// Deterministisk "QR-kode" (mock – rigtig QR genereres senere fra API/URL).
import { useMemo } from 'react'

function buildMatrix(seed) {
  let h = 0
  for (const ch of String(seed)) h = (h * 31 + ch.charCodeAt(0)) >>> 0
  const rnd = () => {
    h ^= h << 13
    h ^= h >>> 17
    h ^= h << 5
    h >>>= 0
    return h / 4294967296
  }
  const size = 21
  const m = Array.from({ length: size }, () => Array(size).fill(0))
  for (let r = 0; r < size; r++) {
    for (let c = 0; c < size; c++) {
      m[r][c] = rnd() > 0.52 ? 1 : 0
    }
  }
  const finder = (top, left) => {
    for (let r = 0; r < 7; r++) {
      for (let c = 0; c < 7; c++) {
        const border = r === 0 || r === 6 || c === 0 || c === 6
        const core = r >= 2 && r <= 4 && c >= 2 && c <= 4
        m[top + r][left + c] = border || core ? 1 : 0
      }
    }
  }
  finder(0, 0)
  finder(14, 0)
  finder(0, 14)
  // lille rydning ved finder-hjørner
  m[7][7] = 0
  return m
}

export default function QrCode({ seed = '0000', size = 78 }) {
  const matrix = useMemo(() => buildMatrix(seed), [seed])
  return (
    <svg width={size} height={size} viewBox="0 0 21 21" role="img" aria-label="QR-kode">
      {matrix.flatMap((row, r) =>
        row.map((cell, c) =>
          cell ? <rect key={`${r}-${c}`} x={c} y={r} width="1" height="1" fill="#0b2140" /> : null,
        ),
      )}
    </svg>
  )
}
