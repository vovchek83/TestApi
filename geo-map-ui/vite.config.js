import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: parseInt(process.env.PORT) || 3000,
    proxy: {
      '/Geo':   { target: 'http://localhost:5297', changeOrigin: true },
      '/Los':   { target: 'http://localhost:5297', changeOrigin: true },
      '/tiles': { target: 'http://localhost:5297', changeOrigin: true },
      '/Srtm':  { target: 'http://localhost:5297', changeOrigin: true, timeout: 0, proxyTimeout: 0 },
    },
  },
})
