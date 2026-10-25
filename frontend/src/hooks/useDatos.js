import { useCallback, useEffect, useState } from 'react'
import { api } from '../api/cliente'

// Pide datos a la API y cancela la petición anterior si cambian los parámetros
// (así una búsqueda vieja nunca pisa a la nueva).
export function useDatos(ruta) {
  const [datos, setDatos] = useState(null)
  const [error, setError] = useState(null)
  const [cargando, setCargando] = useState(true)
  const [version, setVersion] = useState(0)

  useEffect(() => {
    if (!ruta) return
    const ctrl = new AbortController()
    setCargando(true)
    setError(null)
    api.get(ruta, { signal: ctrl.signal })
      .then((d) => { if (!ctrl.signal.aborted) setDatos(d) })
      .catch((e) => { if (e.name !== 'AbortError') setError(e) })
      .finally(() => { if (!ctrl.signal.aborted) setCargando(false) })
    return () => ctrl.abort()
  }, [ruta, version])

  const recargar = useCallback(() => setVersion((v) => v + 1), [])
  return { datos, error, cargando, recargar }
}

// Retrasa un valor (por ejemplo, el texto de búsqueda) para no llamar a la API en cada tecla.
export function useDebounce(valor, ms = 300) {
  const [v, setV] = useState(valor)
  useEffect(() => {
    const t = setTimeout(() => setV(valor), ms)
    return () => clearTimeout(t)
  }, [valor, ms])
  return v
}
