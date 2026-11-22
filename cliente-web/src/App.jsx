import { lazy, Suspense } from 'react'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router'
import { SesionProvider, useSesion } from './context/Sesion'
import Layout from './components/Layout'
import { Cargando } from './components/Comunes'
import Login from './pages/Login'

// Cada pantalla se descarga solo cuando se abre (code splitting por ruta).
const Productos = lazy(() => import('./pages/Productos'))
const Clientes = lazy(() => import('./pages/Clientes'))
const NuevaNota = lazy(() => import('./pages/NuevaNota'))
const NotasVenta = lazy(() => import('./pages/NotasVenta'))
const Familias = lazy(() => import('./pages/Familias'))
const Usuarios = lazy(() => import('./pages/Usuarios'))

function Rutas() {
  const { sesion, esAdmin } = useSesion()

  // Sin sesión solo existe /login; cualquier otra ruta redirige ahí.
  if (!sesion) {
    return (
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    )
  }

  const soloAdmin = (el) => (esAdmin ? el : <Navigate to="/productos" replace />)
  return (
    <Layout>
      <Suspense fallback={<Cargando />}>
        <Routes>
          <Route path="/productos" element={<Productos />} />
          <Route path="/clientes" element={<Clientes />} />
          <Route path="/notas/nueva" element={<NuevaNota />} />
          <Route path="/notas" element={<NotasVenta />} />
          <Route path="/familias" element={soloAdmin(<Familias />)} />
          <Route path="/usuarios" element={soloAdmin(<Usuarios />)} />
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
