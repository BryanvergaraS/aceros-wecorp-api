import { useState } from 'react'
import { api, qs } from '../api/cliente'
import { Aviso, Campo, Dialogo, Encabezado } from '../components/Comunes'
import { useDatos, useDebounce } from '../hooks/useDatos'

const VACIO = { rut: '', razonSocial: '', telefono: '', email: '', direccion: '', estado: true }

export default function Proveedores() {
  const [buscar, setBuscar] = useState('')
  const texto = useDebounce(buscar)
  const { datos, error: errorLista, recargar } = useDatos(`/proveedores${qs({ buscar: texto })}`)
  const [editando, setEditando] = useState(null)
  const [error, setError] = useState(null)

  const listo = () => { setEditando(null); recargar() }

  async function eliminar(p) {
    if (!window.confirm(`¿Eliminar a ${p.razonSocial}? También se quita de los productos que tenga asignados.`)) return
    try { await api.del(`/proveedores/${p.id}`); listo() } catch (e) { setError(e) }
  }

  return (
    <section>
      <Encabezado titulo="Proveedores">
        <button type="button" className="btn btn-principal" onClick={() => setEditando({})}>Nuevo proveedor</button>
      </Encabezado>
      <div className="filtros">
        <input type="search" placeholder="Buscar por razón social o RUT" value={buscar}
          onChange={(e) => setBuscar(e.target.value)} aria-label="Buscar proveedor" />
      </div>
      <Aviso error={error || errorLista} />
      <div className="tabla-marco">
        <table className="tabla">
          <thead><tr><th>RUT</th><th>Razón social</th><th>Teléfono</th><th>Email</th><th>Estado</th><th><span className="oculto">Acciones</span></th></tr></thead>
          <tbody>
            {datos?.map((p) => (
              <tr key={p.id} className={p.estado ? '' : 'inactivo'}>
                <td><span className="sku">{p.rut}</span></td>
                <td>{p.razonSocial}</td>
                <td>{p.telefono}</td>
                <td>{p.email}</td>
                <td>{p.estado ? 'Activo' : 'Inactivo'}</td>
                <td className="acciones">
                  <button type="button" className="btn-texto" onClick={() => setEditando(p)}>Editar</button>
                  <button type="button" className="btn-texto peligro" onClick={() => eliminar(p)}>Eliminar</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {datos && datos.length === 0 && <p className="vacio">No hay proveedores que coincidan.</p>}
      </div>
      {editando && <ProveedorForm proveedor={editando} onCerrar={() => setEditando(null)} onGuardado={listo} />}
    </section>
  )
}

function ProveedorForm({ proveedor, onCerrar, onGuardado }) {
  const nuevo = !proveedor.id
  const [f, setF] = useState(() => {
    const base = { ...VACIO, ...proveedor }
    for (const k of Object.keys(VACIO)) if (base[k] == null) base[k] = VACIO[k]
    return base
  })
  const [error, setError] = useState(null)
  const set = (k) => (e) => setF({ ...f, [k]: e.target.type === 'checkbox' ? e.target.checked : e.target.value })

  async function guardar(e) {
    e.preventDefault()
    const cuerpo = { ...f, telefono: f.telefono || null, email: f.email || null, direccion: f.direccion || null }
    try {
      if (nuevo) await api.post('/proveedores', cuerpo)
      else await api.put(`/proveedores/${proveedor.id}`, cuerpo)
      onGuardado()
    } catch (err) { setError(err) }
  }

  return (
    <Dialogo abierto titulo={nuevo ? 'Nuevo proveedor' : `Editar ${proveedor.razonSocial}`} onCerrar={onCerrar}>
      <form className="formulario" onSubmit={guardar}>
        <div className="rejilla">
          <Campo etiqueta="RUT"><input required placeholder="76.111.111-1" value={f.rut} onChange={set('rut')} /></Campo>
          <Campo etiqueta="Teléfono"><input maxLength={20} value={f.telefono} onChange={set('telefono')} /></Campo>
        </div>
        <Campo etiqueta="Razón social"><input required maxLength={150} value={f.razonSocial} onChange={set('razonSocial')} /></Campo>
        <Campo etiqueta="Email"><input type="email" maxLength={150} value={f.email} onChange={set('email')} /></Campo>
        <Campo etiqueta="Dirección"><input maxLength={200} value={f.direccion} onChange={set('direccion')} /></Campo>
        <label className="check"><input type="checkbox" checked={f.estado} onChange={set('estado')} /> Proveedor activo</label>
        <Aviso error={error} />
        <div className="formulario-pie">
          <button type="button" className="btn" onClick={onCerrar}>Cancelar</button>
          <button className="btn btn-principal">{nuevo ? 'Crear proveedor' : 'Guardar cambios'}</button>
        </div>
      </form>
    </Dialogo>
  )
}
