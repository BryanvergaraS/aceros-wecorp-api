// Formateadores creados una sola vez (Intl es costoso de instanciar en cada render).
const clp = new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP', maximumFractionDigits: 0 })
const entero = new Intl.NumberFormat('es-CL')
const fecha = new Intl.DateTimeFormat('es-CL', { dateStyle: 'short', timeStyle: 'short' })

export const pesos = (n) => clp.format(n ?? 0)
export const numero = (n) => entero.format(n ?? 0)
export const fechaHora = (s) => (s ? fecha.format(new Date(s)) : '')
