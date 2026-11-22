import { useState } from 'react'
import { catalogo, errorDe } from '../api/cliente'
import { Aviso, Campo, Cargando, Dialogo, Encabezado } from '../components/Comunes'
import { useSesion } from '../context/Sesion'
import { useDatos } from '../hooks/useDatos'

// Familias del catálogo (RF04, solo Administrador).
export default function Familias() {
  const { cargarFamilias } = useSesion()
  const { datos, error: errorLista, cargando, recargar } = useDatos(catalogo, '/familias')
  const [editando, setEditando] = useState(null)
  const [error, setError] = useState(null)

  const listo = () => { setEditando(null); recargar(); cargarFamilias(true).catch(() => {}) }

  async function eliminar(f) {
    if (!window.confirm(`¿Eliminar la familia ${f.nombre}?`)) return
    setError(null)
    try { await catalogo.del(`/familias/${f.id}`); listo() } catch (e) { setError(e) }
  }

  return (
    <section>
      <Encabezado titulo="Familias">
        <button type="button" className="btn btn-principal" onClick={() => setEditando({})}>Nueva familia</button>
      </Encabezado>
      <Aviso error={error || errorLista} />
      {cargando && !datos && <Cargando />}
      {datos && (
        <div className={`tabla-marco ${cargando ? 'actualizando' : ''}`}>
          <table className="tabla">
            <thead><tr><th>Nombre</th><th>Descripción</th><th>Estado</th><th><span className="oculto">Acciones</span></th></tr></thead>
            <tbody>
              {datos.map((f) => (
                <tr key={f.id} className={f.activo ? '' : 'inactivo'}>
                  <td><strong>{f.nombre}</strong></td>
                  <td>{f.descripcion}</td>
                  <td>{f.activo ? 'Activa' : 'Inactiva'}</td>
                  <td className="acciones">
                    <button type="button" className="btn-texto" onClick={() => setEditando(f)}>Editar</button>
                    <button type="button" className="btn-texto peligro" onClick={() => eliminar(f)}>Eliminar</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {datos.length === 0 && <p className="vacio">Todavía no hay familias.</p>}
        </div>
      )}
      {editando && <FamiliaForm familia={editando} onCerrar={() => setEditando(null)} onGuardado={listo} />}
    </section>
  )
}

function FamiliaForm({ familia, onCerrar, onGuardado }) {
  const nueva = !familia.id
  const [f, setF] = useState({ nombre: familia.nombre ?? '', descripcion: familia.descripcion ?? '', activo: familia.activo ?? true })
  const [error, setError] = useState(null)
  const [guardando, setGuardando] = useState(false)

  async function guardar(e) {
    e.preventDefault()
    setGuardando(true)
    setError(null)
    const cuerpo = { nombre: f.nombre.trim(), descripcion: f.descripcion.trim() || null, activo: f.activo }
    try {
      if (nueva) await catalogo.post('/familias', cuerpo)
      else await catalogo.put(`/familias/${familia.id}`, cuerpo)
      onGuardado()
    } catch (err) {
      setError(err)
      setGuardando(false)
    }
  }

  const e = error?.errores
  return (
    <Dialogo abierto titulo={nueva ? 'Nueva familia' : `Editar ${familia.nombre}`} onCerrar={onCerrar}>
      <form className="formulario" onSubmit={guardar}>
        <Campo etiqueta="Nombre" error={errorDe(e, 'nombre')}>
          <input required maxLength={50} value={f.nombre} onChange={(ev) => setF({ ...f, nombre: ev.target.value })} />
        </Campo>
        <Campo etiqueta="Descripción" error={errorDe(e, 'descripcion')}>
          <input maxLength={200} value={f.descripcion} onChange={(ev) => setF({ ...f, descripcion: ev.target.value })} />
        </Campo>
        <label className="check">
          <input type="checkbox" checked={f.activo} onChange={(ev) => setF({ ...f, activo: ev.target.checked })} /> Familia activa
        </label>
        {error && !e && <Aviso error={error} />}
        <div className="formulario-pie">
          <button type="button" className="btn" onClick={onCerrar}>Cancelar</button>
          <button className="btn btn-principal" disabled={guardando}>
            {guardando ? 'Guardando…' : nueva ? 'Crear familia' : 'Guardar cambios'}
          </button>
        </div>
      </form>
    </Dialogo>
  )
}
