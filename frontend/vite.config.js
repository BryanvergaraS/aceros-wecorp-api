import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// La API corre en http://localhost:5080. El proxy evita CORS en desarrollo y en preview.
const api = { '/api': { target: 'http://localhost:5080', changeOrigin: true } }

export default defineConfig({
  plugins: [react()],
  server: { port: 5173, proxy: api },
  preview: { port: 4173, proxy: api },
  build: {
    target: 'es2022',
    cssCodeSplit: true,
    rollupOptions: {
      output: {
        // React y el router en un chunk aparte: cambian poco y quedan en caché del navegador.
        manualChunks(id) {
          if (id.includes('node_modules/react') || id.includes('node_modules/scheduler')) return 'react'
        }
      }
    }
  }
})
