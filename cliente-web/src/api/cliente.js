// Cliente HTTP mínimo sobre fetch para las dos APIs del sistema.
// Agrega el JWT, entiende ProblemDetails (RFC 7807), permite cancelar con AbortController
// y avisa cuando la sesión expiró (401 con token).
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

// Mensajes en español cuando la API no entrega un detalle.
const MENSAJES = {
  401: 'Tu sesión no es válida. Inicia sesión nuevamente.',
  403: 'No tienes permiso para realizar esta acción.',
  404: 'El registro no existe o fue eliminado.',
  409: 'La operación choca con datos existentes.',
  422: 'La operación no cumple una regla del negocio.',
  503: 'El servicio de catálogo no está disponible. Intenta nuevamente.'
}

function crearApi(base) {
  async function solicitar(metodo, ruta, { cuerpo, signal } = {}) {
    const headers = { Accept: 'application/json' }
    if (cuerpo !== undefined) headers['Content-Type'] = 'application/json'
    const token = sesion?.token
    if (token) headers.Authorization = `Bearer ${token}`

    let res
    try {
      res = await fetch(base + ruta, {
        method: metodo,
        headers,
        body: cuerpo === undefined ? undefined : JSON.stringify(cuerpo),
        signal
      })
    } catch (e) {
      if (e.name === 'AbortError') throw e
      throw new ApiError(0, 'No se pudo conectar con el servidor.')
    }

    if (res.status === 204) return null
    let datos = null
    if (res.headers.get('content-type')?.includes('json')) {
      try {
        datos = await res.json()
      } catch (e) {
        if (!(e instanceof SyntaxError)) throw e
        if (res.ok) throw new ApiError(res.status, 'El servidor devolvió una respuesta inválida.')
      }
    }

    if (!res.ok) {
      // 401 con token = token vencido o inválido: se cierra la sesión y se avisa.
      if (res.status === 401 && token) {
        guardarSesion(null)
        alExpirar()
      }
      const errores = datos?.errors ?? null
      const primero = errores ? Object.values(errores).flat()[0] : null
      const mensaje = res.status === 503
        ? MENSAJES[503]
        : primero || datos?.detail || MENSAJES[res.status] || datos?.title || `Error ${res.status}`
      throw new ApiError(res.status, mensaje, errores)
    }
    return datos
  }

  return {
    get: (ruta, opciones) => solicitar('GET', ruta, opciones),
    post: (ruta, cuerpo) => solicitar('POST', ruta, { cuerpo }),
    put: (ruta, cuerpo) => solicitar('PUT', ruta, { cuerpo }),
    patch: (ruta, cuerpo) => solicitar('PATCH', ruta, { cuerpo }),
    del: (ruta) => solicitar('DELETE', ruta)
  }
}

// API de Catálogo e Identidad (login, productos, familias, usuarios).
export const catalogo = crearApi('/catalogo/api/v1')
// API de Ventas (clientes y notas de venta).
export const ventas = crearApi('/ventas/api/v1')

// Convierte un objeto en query string, omitiendo vacíos.
export function qs(params) {
  const p = new URLSearchParams()
  for (const [k, v] of Object.entries(params)) if (v !== '' && v != null) p.set(k, v)
  const s = p.toString()
  return s ? `?${s}` : ''
}

// Busca el error de validación de un campo (las claves pueden venir como "Sku", "sku" o "$.sku").
export function errorDe(errores, campo) {
  if (!errores) return null
  const clave = Object.keys(errores).find((k) => k.replace('$.', '').toLowerCase() === campo.toLowerCase())
  return clave ? errores[clave][0] : null
}
