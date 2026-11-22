import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Cada API tiene un prefijo propio. El proxy los quita y reenvía la petición,
// así el navegador habla con un solo origen y no se necesita CORS.
//   /catalogo/api/v1/...  ->  http://localhost:5081/api/v1/...
//   /ventas/api/v1/...    ->  http://localhost:5082/api/v1/...
const proxy = {
  '/catalogo': {
    target: 'http://localhost:5081',
    changeOrigin: true,
    rewrite: (ruta) => ruta.replace(/^\/catalogo/, '')
  },
  '/ventas': {
    target: 'http://localhost:5082',
    changeOrigin: true,
    rewrite: (ruta) => ruta.replace(/^\/ventas/, '')
  }
}

export default defineConfig({
  plugins: [react()],
  server: { port: 5173, proxy },
  preview: { port: 4173, proxy },
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
