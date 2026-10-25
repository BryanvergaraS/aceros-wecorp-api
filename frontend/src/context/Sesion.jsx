import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'
import { alExpirarSesion, api, guardarSesion, obtenerSesion } from '../api/cliente'

const SesionContext = createContext(null)

export function SesionProvider({ children }) {
  const [sesion, setSesion] = useState(obtenerSesion)
  const [familias, setFamilias] = useState(null)

  const cerrar = useCallback(() => {
    guardarSesion(null)
    setSesion(null)
    setFamilias(null)
  }, [])

  useEffect(() => alExpirarSesion(cerrar), [cerrar])

  const iniciar = useCallback(async (email, password) => {
    const r = await api.post('/auth/login', { email, password })
    const nueva = { token: r.token, expira: r.expira, usuario: r.usuario }
    guardarSesion(nueva)
    setSesion(nueva)
  }, [])

  // Las familias se piden una vez por sesión y se comparten entre pantallas.
  const cargarFamilias = useCallback(async (forzar = false) => {
    if (familias && !forzar) return familias
    const lista = await api.get('/familias')
    setFamilias(lista)
    return lista
  }, [familias])

  const valor = useMemo(() => ({
    sesion,
    usuario: sesion?.usuario ?? null,
    esAdmin: sesion?.usuario?.rol === 'Administrador',
    iniciar,
    cerrar,
    familias,
    cargarFamilias
  }), [sesion, iniciar, cerrar, familias, cargarFamilias])

  return <SesionContext.Provider value={valor}>{children}</SesionContext.Provider>
}

export const useSesion = () => useContext(SesionContext)
