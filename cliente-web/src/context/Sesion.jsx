import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'
import { alExpirarSesion, catalogo, guardarSesion, obtenerSesion } from '../api/cliente'

const SesionContext = createContext(null)
const AVISO_EXPIRADA = 'Tu sesión expiró. Inicia sesión nuevamente.'

export function SesionProvider({ children }) {
  const [sesion, setSesion] = useState(obtenerSesion)
  const [aviso, setAviso] = useState('')
  const [familias, setFamilias] = useState(null)

  const cerrar = useCallback((motivo = '') => {
    guardarSesion(null)
    setSesion(null)
    setFamilias(null)
    setAviso(motivo)
  }, [])

  // Cualquier 401 con token (en cualquiera de las dos APIs) cierra la sesión con aviso.
  useEffect(() => alExpirarSesion(() => cerrar(AVISO_EXPIRADA)), [cerrar])

  // También se cierra sola cuando llega la hora de expiración del token, aunque no haya peticiones.
  useEffect(() => {
    if (!sesion) return
    const resta = new Date(sesion.expira).getTime() - Date.now()
    if (resta <= 0) { cerrar(AVISO_EXPIRADA); return }
    const t = setTimeout(() => cerrar(AVISO_EXPIRADA), Math.min(resta, 2 ** 31 - 1))
    return () => clearTimeout(t)
  }, [sesion, cerrar])

  const iniciar = useCallback(async (email, password) => {
    const r = await catalogo.post('/auth/login', { email, password })
    const nueva = { token: r.token, expira: r.expira, usuario: r.usuario }
    guardarSesion(nueva)
    setAviso('')
    setSesion(nueva)
  }, [])

  // Las familias se piden una vez por sesión y se comparten entre pantallas.
  const cargarFamilias = useCallback(async (forzar = false) => {
    if (familias && !forzar) return familias
    const lista = await catalogo.get('/familias')
    setFamilias(lista)
    return lista
  }, [familias])

  const valor = useMemo(() => ({
    sesion,
    aviso,
    usuario: sesion?.usuario ?? null,
    esAdmin: sesion?.usuario?.rol === 'Administrador',
    iniciar,
    cerrar,
    familias,
    cargarFamilias
  }), [sesion, aviso, iniciar, cerrar, familias, cargarFamilias])

  return <SesionContext.Provider value={valor}>{children}</SesionContext.Provider>
}

export const useSesion = () => useContext(SesionContext)
