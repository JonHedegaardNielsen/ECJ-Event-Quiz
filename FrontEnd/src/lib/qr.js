// Join-URL til QR-koden. I dev-mode peger den på hostens LAN-IP (injiceret af
// vite.config.js som __DEV_HOST_IP__), så telefoner kan scanne den selv når
// værten har åbnet siden via localhost. I produktion bruges den aktuelle URL.

const LOCAL_HOSTS = ['localhost', '127.0.0.1', '::1']

export function joinUrl(roomId) {
  const path = `/#/deltag?code=${encodeURIComponent(roomId)}`
  const devIp = __DEV_HOST_IP__ || ''
  if (import.meta.env.DEV && devIp && LOCAL_HOSTS.includes(window.location.hostname)) {
    return `${window.location.protocol}//${devIp}:${window.location.port}${path}`
  }
  return `${window.location.origin}${path}`
}
