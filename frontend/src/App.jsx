import { lazy, Suspense } from 'react'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router'
import { SesionProvider, useSesion } from './context/Sesion'
import Layout from './components/Layout'
import Login from './pages/Login'

// Cada pantalla se descarga solo cuando se abre (code splitting por ruta).
const Productos = lazy(() => import('./pages/Productos'))
const NotasVenta = lazy(() => import('./pages/NotasVenta'))
const NuevaNota = lazy(() => import('./pages/NuevaNota'))
const Familias = lazy(() => import('./pages/Familias'))
const Proveedores = lazy(() => import('./pages/Proveedores'))
const Usuarios = lazy(() => import('./pages/Usuarios'))
const Logs = lazy(() => import('./pages/Logs'))

function Rutas() {
  const { sesion, esAdmin } = useSesion()
  if (!sesion) return <Login />

  const soloAdmin = (el) => (esAdmin ? el : <Navigate to="/productos" replace />)
  return (
    <Layout>
      <Suspense fallback={<p className="cargando">Cargando…</p>}>
        <Routes>
          <Route path="/productos" element={<Productos />} />
          <Route path="/notas" element={<NotasVenta />} />
          <Route path="/notas/nueva" element={<NuevaNota />} />
          <Route path="/familias" element={soloAdmin(<Familias />)} />
          <Route path="/proveedores" element={soloAdmin(<Proveedores />)} />
          <Route path="/usuarios" element={soloAdmin(<Usuarios />)} />
          <Route path="/logs" element={soloAdmin(<Logs />)} />
          <Route path="*" element={<Navigate to="/productos" replace />} />
        </Routes>
      </Suspense>
    </Layout>
  )
}

export default function App() {
  return (
    <SesionProvider>
      <BrowserRouter>
        <Rutas />
      </BrowserRouter>
    </SesionProvider>
  )
}
