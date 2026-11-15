import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'
import os from 'node:os'

// Hostens LAN-IPv4, så QR-koden kan pege på en adresse telefoner kan nå,
// selv når værten har åbnet frontend'en via localhost i dev-mode.
// Samme filtrering som NetworkAddresses.cs i ServerUI (skipper Hyper-V/WSL/VPN m.m.).
function lanIPv4() {
  const skip = /hyper-v|vethernet|wsl|virtualbox|vmware|vpn|tunnel|tap-|tun|bluetooth|loopback|pseudo/i
  // Trådløs først, som i NetworkAddresses.cs (Wireless80211 => 0, Ethernet => 1)
  const wireless = /wi-?fi|wlan|wireless|802\.11/i
  const entries = Object.entries(os.networkInterfaces())
    .filter(([name]) => !skip.test(name))
    .sort((a, b) => (wireless.test(a[0]) ? 0 : 1) - (wireless.test(b[0]) ? 0 : 1))
  for (const [, addrs] of entries) {
    for (const addr of addrs) {
      if (addr.family !== 'IPv4' || addr.internal) continue
      if (addr.address.startsWith('169.254.')) continue // link-local
      return addr.address
    }
  }
  return ''
}

// https://vite.dev/config/
export default defineConfig(({ command }) => ({
  plugins: [react()],
  // Lyt på hele netværket, så deltagere kan joine fra telefoner (se "Network:" i terminalen)
  server: { host: true },
  // I dev peger QR-koden på hostens LAN-IP når værten åbner siden via localhost.
  // I build er konstanten tom (ingen LAN-IP lækkes), og QrCode bruger den aktuelle URL.
  define: { __DEV_HOST_IP__: JSON.stringify(command === 'serve' ? lanIPv4() : '') },
}))
