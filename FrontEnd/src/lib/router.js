// Minimal hash-router (ingen dependencies – API'et/arkitektur kan udskiftes senere).
import { useEffect, useState } from 'react'

export function currentPath() {
  const raw = window.location.hash.replace(/^#/, '')
  return (raw || '/').split('?')[0]
}

export function navigate(to) {
  if (currentPath() === to) return
  window.location.hash = to
}

export function useRoute() {
  const [path, setPath] = useState(currentPath)
  useEffect(() => {
    const onChange = () => setPath(currentPath())
    window.addEventListener('hashchange', onChange)
    return () => window.removeEventListener('hashchange', onChange)
  }, [])
  return path
}
