import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  // Lyt på hele netværket, så deltagere kan joine fra telefoner (se "Network:" i terminalen)
  server: { host: true },
})
