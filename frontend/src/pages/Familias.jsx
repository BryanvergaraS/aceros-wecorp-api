import { useState } from 'react'
import { api } from '../api/cliente'
import { Aviso, Campo, Dialogo, Encabezado } from '../components/Comunes'
import { useSesion } from '../context/Sesion'
import { useDatos } from '../hooks/useDatos'

export default function Familias() {
  const { cargarFamilias } = useSesion()
  const { datos, error: errorLista, recargar } = useDatos('/familias')
  const [editando, setEditando] = useState(null)
  const [error, setError] = useState(null)

  const listo = () => { setEditando(null); recargar(); cargarFamilias(true) }

  async function eliminar(f) {
    if (!window.confirm(`¿Eliminar la familia ${f.nombre}?`)) return
    try { await api.del(`/familias/${f.id}`); listo() } catch (e) { setError(e) }
  }

  return (
    <section>
      <Encabezado titulo="Familias">
        <button type="button" className="btn btn-principal" onClick={() => setEditando({})}>Nueva familia</button>
      </Encabezado>
      <Aviso error={error || errorLista} />
      <div className="tabla-marco">
        <table className="tabla">
          <thead><tr><th>Nombre</th><th>Descripción</th><th>Estado</th><th><span className="oculto">Acciones</span></th></tr></thead>
          <tbody>
            {datos?.map((f) => (
              <tr key={f.id} className={f.estado ? '' : 'inactivo'}>
                <td><strong>{f.nombre}</strong></td>
                <td>{f.descripcion}</td>
                <td>{f.estado ? 'Activa' : 'Inactiva'}</td>
                <td className="acciones">
                  <button type="button" className="btn-texto" onClick={() => setEditando(f)}>Editar</button>
                  <button type="button" className="btn-texto peligro" onClick={() => eliminar(f)}>Eliminar</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {editando && <FamiliaForm familia={editando} onCerrar={() => setEditando(null)} onGuardado={listo} />}
    </section>
  )
}

function FamiliaForm({ familia, onCerrar, onGuardado }) {
  const nueva = !familia.id
  const [f, setF] = useState({ nombre: familia.nombre ?? '', descripcion: familia.descripcion ?? '', estado: familia.estado ?? true })
  const [error, setError] = useState(null)

  async function guardar(e) {
    e.preventDefault()
    try {
      if (nueva) await api.post('/familias', f)
      else await api.put(`/familias/${familia.id}`, f)
      onGuardado()
    } catch (err) { setError(err) }
  }

  return (
    <Dialogo abierto titulo={nueva ? 'Nueva familia' : `Editar ${familia.nombre}`} onCerrar={onCerrar}>
      <form className="formulario" onSubmit={guardar}>
        <Campo etiqueta="Nombre"><input required maxLength={50} value={f.nombre} onChange={(e) => setF({ ...f, nombre: e.target.value })} /></Campo>
        <Campo etiqueta="Descripción"><input maxLength={200} value={f.descripcion} onChange={(e) => setF({ ...f, descripcion: e.target.value })} /></Campo>
        <label className="check"><input type="checkbox" checked={f.estado} onChange={(e) => setF({ ...f, estado: e.target.checked })} /> Familia activa</label>
        <Aviso error={error} />
        <div className="formulario-pie">
          <button type="button" className="btn" onClick={onCerrar}>Cancelar</button>
          <button className="btn btn-principal">{nueva ? 'Crear familia' : 'Guardar cambios'}</button>
        </div>
      </form>
    </Dialogo>
  )
}
