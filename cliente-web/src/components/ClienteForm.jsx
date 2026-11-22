import { useState } from 'react'
import { errorDe, ventas } from '../api/cliente'
import { Aviso, Campo, Dialogo } from './Comunes'

// Registrar o editar un cliente (ClienteGuardar del contrato de la API de Ventas).
// La API valida el RUT con su dígito verificador y responde 409 si ya existe.
export default function ClienteForm({ cliente, onCerrar, onGuardado }) {
  const nuevo = !cliente.id
  const [f, setF] = useState({
    rut: cliente.rut ?? '',
    nombre: cliente.nombre ?? '',
    email: cliente.email ?? '',
    telefono: cliente.telefono ?? '',
    direccion: cliente.direccion ?? ''
  })
  const [error, setError] = useState(null)
  const [guardando, setGuardando] = useState(false)
  const set = (k) => (e) => setF((x) => ({ ...x, [k]: e.target.value }))

  async function guardar(e) {
    e.preventDefault()
    setGuardando(true)
    setError(null)
    const cuerpo = {
      rut: f.rut.trim(),
      nombre: f.nombre.trim(),
      email: f.email.trim() || null,
      telefono: f.telefono.trim() || null,
      direccion: f.direccion.trim() || null
    }
    try {
      const r = nuevo ? await ventas.post('/clientes', cuerpo) : await ventas.put(`/clientes/${cliente.id}`, cuerpo)
      onGuardado(r ?? { ...cliente, ...cuerpo })
    } catch (err) {
      setError(err)
      setGuardando(false)
    }
  }

  const e = error?.errores
  return (
    <Dialogo abierto titulo={nuevo ? 'Nuevo cliente' : `Editar ${cliente.nombre}`} onCerrar={onCerrar}>
      <form className="formulario" onSubmit={guardar}>
        <div className="rejilla">
          <Campo etiqueta="RUT" error={errorDe(e, 'rut')}>
            <input required maxLength={12} placeholder="12.345.678-5" value={f.rut} onChange={set('rut')} />
          </Campo>
          <Campo etiqueta="Teléfono" error={errorDe(e, 'telefono')}>
            <input maxLength={20} value={f.telefono} onChange={set('telefono')} />
          </Campo>
        </div>
        <Campo etiqueta="Nombre o razón social" error={errorDe(e, 'nombre')}>
          <input required maxLength={150} value={f.nombre} onChange={set('nombre')} />
        </Campo>
        <Campo etiqueta="Email" error={errorDe(e, 'email')}>
          <input type="email" maxLength={150} value={f.email} onChange={set('email')} />
        </Campo>
        <Campo etiqueta="Dirección" error={errorDe(e, 'direccion')}>
          <input maxLength={200} value={f.direccion} onChange={set('direccion')} />
        </Campo>
        {error && !e && <Aviso error={error} />}
        <div className="formulario-pie">
          <button type="button" className="btn" onClick={onCerrar}>Cancelar</button>
          <button className="btn btn-principal" disabled={guardando}>
            {guardando ? 'Guardando…' : nuevo ? 'Registrar cliente' : 'Guardar cambios'}
          </button>
        </div>
      </form>
    </Dialogo>
  )
}
