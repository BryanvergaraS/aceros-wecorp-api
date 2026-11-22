import { useEffect, useState } from 'react'
import { qs, ventas } from '../api/cliente'
import { fechaHora } from '../api/formato'
import { Aviso, Cargando, Encabezado, Paginador } from '../components/Comunes'
import ClienteForm from '../components/ClienteForm'
import { useSesion } from '../context/Sesion'
import { useDatos, useDebounce } from '../hooks/useDatos'

const TAMANO = 20

// Clientes (RF06): ambos roles registran y editan; solo el Administrador elimina.
export default function Clientes() {
  const { esAdmin } = useSesion()
  const [buscar, setBuscar] = useState('')
  const [pagina, setPagina] = useState(1)
  const [editando, setEditando] = useState(null)
  const [error, setError] = useState(null)
  const texto = useDebounce(buscar)

  useEffect(() => { setPagina(1) }, [texto])

  const { datos, error: errorLista, cargando, recargar } =
    useDatos(ventas, `/clientes${qs({ buscar: texto, pagina, tamano: TAMANO })}`)

  async function eliminar(c) {
    if (!window.confirm(`¿Eliminar al cliente ${c.nombre}?`)) return
    setError(null)
    try {
      await ventas.del(`/clientes/${c.id}`)
      recargar()
    } catch (e) {
      setError(e)
    }
  }

  return (
    <section>
      <Encabezado titulo="Clientes">
        <button type="button" className="btn btn-principal" onClick={() => setEditando({})}>Nuevo cliente</button>
      </Encabezado>

      <div className="filtros">
        <input type="search" placeholder="Buscar por RUT o nombre" value={buscar}
          onChange={(e) => setBuscar(e.target.value)} aria-label="Buscar cliente" />
      </div>

      <Aviso error={error || errorLista} />
      {cargando && !datos && <Cargando />}

      {datos && (
        <div className={`tabla-marco ${cargando ? 'actualizando' : ''}`} aria-busy={cargando}>
          <table className="tabla">
            <thead>
              <tr>
                <th>RUT</th><th>Nombre</th><th>Email</th><th>Teléfono</th><th>Registro</th>
                <th><span className="oculto">Acciones</span></th>
              </tr>
            </thead>
            <tbody>
              {datos.items.map((c) => (
                <tr key={c.id}>
                  <td className="nowrap">{c.rut}</td>
                  <td><strong>{c.nombre}</strong>{c.direccion && <span className="texto-suave">{c.direccion}</span>}</td>
                  <td>{c.email}</td>
                  <td className="nowrap">{c.telefono}</td>
                  <td className="nowrap">{fechaHora(c.fechaRegistro)}</td>
                  <td className="acciones">
                    <button type="button" className="btn-texto" onClick={() => setEditando(c)}>Editar</button>
                    {esAdmin && <button type="button" className="btn-texto peligro" onClick={() => eliminar(c)}>Eliminar</button>}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {datos.items.length === 0 && <p className="vacio">No hay clientes que coincidan.</p>}
        </div>
      )}

      {datos && <Paginador pagina={pagina} total={datos.total} tamano={TAMANO} onCambiar={setPagina} />}

      {editando && (
        <ClienteForm cliente={editando} onCerrar={() => setEditando(null)}
          onGuardado={() => { setEditando(null); recargar() }} />
      )}
    </section>
  )
}
