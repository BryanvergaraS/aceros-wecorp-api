// Cliente HTTP mínimo sobre fetch: agrega el JWT, entiende ProblemDetails y permite cancelar.
const BASE = '/api/v1'
const CLAVE = 'wecorp.sesion'

let sesion = leerSesion()
let alExpirar = () => {}

function leerSesion() {
  try {
    const s = JSON.parse(sessionStorage.getItem(CLAVE))
    return s && new Date(s.expira) > new Date() ? s : null
  } catch {
    return null
  }
}

export const obtenerSesion = () => sesion

export function guardarSesion(s) {
  sesion = s
  try {
    if (s) sessionStorage.setItem(CLAVE, JSON.stringify(s))
    else sessionStorage.removeItem(CLAVE)
  } catch { /* almacenamiento no disponible: la sesión vive solo en memoria */ }
}

export const alExpirarSesion = (fn) => { alExpirar = fn }

export class ApiError extends Error {
  constructor(status, mensaje, errores) {
    super(mensaje)
    this.status = status
    this.errores = errores // errores de validación por campo (400)
  }
}

async function solicitar(metodo, ruta, { cuerpo, signal } = {}) {
  const headers = { Accept: 'application/json' }
  if (cuerpo !== undefined) headers['Content-Type'] = 'application/json'
  if (sesion?.token) headers.Authorization = `Bearer ${sesion.token}`

  const res = await fetch(BASE + ruta, {
    method: metodo,
    headers,
    body: cuerpo === undefined ? undefined : JSON.stringify(cuerpo),
    signal
  })

  if (res.status === 204) return null
  const datos = res.headers.get('content-type')?.includes('json') ? await res.json() : null

  if (!res.ok) {
    if (res.status === 401 && sesion) {
      guardarSesion(null)
      alExpirar()
    }
    const errores = datos?.errors ?? null
    const primero = errores ? Object.values(errores).flat()[0] : null
    const mensaje = primero || datos?.detail || datos?.title || `Error ${res.status}`
    throw new ApiError(res.status, mensaje, errores)
  }
  return datos
}

export const api = {
  get: (ruta, opciones) => solicitar('GET', ruta, opciones),
  post: (ruta, cuerpo) => solicitar('POST', ruta, { cuerpo }),
  put: (ruta, cuerpo) => solicitar('PUT', ruta, { cuerpo }),
  patch: (ruta, cuerpo) => solicitar('PATCH', ruta, { cuerpo }),
  del: (ruta) => solicitar('DELETE', ruta)
}

// Convierte un objeto en query string, omitiendo vacíos.
export function qs(params) {
  const p = new URLSearchParams()
  for (const [k, v] of Object.entries(params)) if (v !== '' && v != null) p.set(k, v)
  const s = p.toString()
  return s ? `?${s}` : ''
}
