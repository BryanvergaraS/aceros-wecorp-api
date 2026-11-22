import { useEffect, useState } from 'react'
import { Link, useLocation } from 'react-router'
import { qs, ventas } from '../api/cliente'
import { fechaHora, numero, pesos } from '../api/formato'
import { Aviso, Cargando, Dialogo, Encabezado, EstadoNota, Paginador } from '../components/Comunes'
import { useSesion } from '../context/Sesion'
import { useDatos } from '../hooks/useDatos'

const TAMANO = 20

// Notas de venta (RF09, RF10, RF11). La API entrega al Vendedor solo sus notas.
export default function NotasVenta() {
  const { esAdmin } = useSesion()
  const { state } = useLocation()
  const [filtros, setFiltros] = useState({ estado: '', desde: '', hasta: '' })
  const [pagina, setPagina] = useState(1)
  const [abiertaId, setAbiertaId] = useState(null)
  const { datos, error, cargando, recargar } =
    useDatos(ventas, `/notas-venta${qs({ ...filtros, pagina, tamano: TAMANO })}`)

  const setFiltro = (k) => (e) => { setFiltros((f) => ({ ...f, [k]: e.target.value })); setPagina(1) }

  return (
    <section>
      <Encabezado titulo={esAdmin ? 'Notas de venta' : 'Mis notas de venta'}>
        <Link to="/notas/nueva" className="btn btn-principal">Nueva nota de venta</Link>
      </Encabezado>
      {state?.emitida && <p className="exito" role="status">Nota de venta {state.emitida} emitida.</p>}

      <div className="filtros">
        <select value={filtros.estado} onChange={setFiltro('estado')} aria-label="Estado">
          <option value="">Todos los estados</option>
          <option>Pendiente</option>
          <option>Pagada</option>
          <option>Anulada</option>
        </select>
        <label className="filtro-fecha">Desde <input type="date" value={filtros.desde} onChange={setFiltro('desde')} /></label>
        <label className="filtro-fecha">Hasta <input type="date" value={filtros.hasta} onChange={setFiltro('hasta')} /></label>
      </div>
      <Aviso error={error} />
      {cargando && !datos && <Cargando />}

      {datos && (
        <div className={`tabla-marco ${cargando ? 'actualizando' : ''}`} aria-busy={cargando}>
          <table className="tabla">
            <thead>
              <tr>
                <th>Folio</th><th>Fecha</th><th>Cliente</th>{esAdmin && <th>Vendedor</th>}
                <th className="num">Total</th><th>Estado</th><th><span className="oculto">Ver</span></th>
              </tr>
            </thead>
            <tbody>
              {datos.items.map((n) => (
                <tr key={n.id}>
                  <td><span className="sku">{n.folio}</span></td>
                  <td className="nowrap">{fechaHora(n.fechaCreacion)}</td>
                  <td>{n.clienteNombre}</td>
                  {esAdmin && <td>{n.vendedorNombre}</td>}
                  <td className="num">{pesos(n.total)}</td>
                  <td><EstadoNota estado={n.estado} /></td>
                  <td className="acciones"><button type="button" className="btn-texto" onClick={() => setAbiertaId(n.id)}>Ver detalle</button></td>
                </tr>
              ))}
            </tbody>
          </table>
          {datos.items.length === 0 && <p className="vacio">Todavía no hay notas de venta con estos filtros.</p>}
        </div>
      )}
      {datos && <Paginador pagina={pagina} total={datos.total} tamano={TAMANO} onCambiar={setPagina} />}

      {abiertaId && (
        <DetalleNota id={abiertaId} esAdmin={esAdmin} onCerrar={() => setAbiertaId(null)} onCambio={recargar} />
      )}
    </section>
  )
}

// El listado no trae el detalle: se pide la nota completa al abrirla.
function DetalleNota({ id, esAdmin, onCerrar, onCambio }) {
  const { datos, error: errorCarga, cargando } = useDatos(ventas, `/notas-venta/${id}`)
  const [nota, setNota] = useState(null)
  const [error, setError] = useState(null)
  const [cambiando, setCambiando] = useState(false)

  useEffect(() => { if (datos) setNota(datos) }, [datos])

  async function cambiarEstado(nuevo) {
    const pregunta = nuevo === 'Anulada'
      ? `¿Anular la nota ${nota.folio}? El stock vuelve al catálogo.`
      : `¿Marcar la nota ${nota.folio} como pagada?`
    if (!window.confirm(pregunta)) return
    setCambiando(true)
    setError(null)
    try {
      setNota(await ventas.patch(`/notas-venta/${nota.id}/estado`, { estado: nuevo }))
      onCambio()
    } catch (e) {
      setError(e)
    } finally {
      setCambiando(false)
    }
  }

  return (
    <Dialogo abierto titulo={nota ? `Nota de venta ${nota.folio}` : 'Nota de venta'} onCerrar={onCerrar}>
      <div className="detalle-nota">
        {cargando && !nota && <Cargando />}
        <Aviso error={errorCarga} />
        {nota && (
          <>
            <dl className="datos">
              <dt>Cliente</dt><dd>{nota.clienteNombre}, RUT {nota.clienteRut}</dd>
              <dt>Vendedor</dt><dd>{nota.vendedorNombre}</dd>
              <dt>Fecha</dt><dd>{fechaHora(nota.fechaCreacion)}</dd>
              <dt>Estado</dt><dd><EstadoNota estado={nota.estado} /></dd>
              {nota.fechaActualizacion && <><dt>Actualizada</dt><dd>{fechaHora(nota.fechaActualizacion)}</dd></>}
              {nota.observaciones && <><dt>Observaciones</dt><dd>{nota.observaciones}</dd></>}
            </dl>
            <div className="tabla-marco">
              <table className="tabla tabla-compacta">
                <thead><tr><th>SKU</th><th>Producto</th><th className="num">Cantidad</th><th className="num">Precio</th><th className="num">Subtotal</th></tr></thead>
                <tbody>
                  {nota.detalles.map((d) => (
                    <tr key={d.productoId}>
                      <td><span className="sku">{d.sku}</span></td><td>{d.nombreProducto}</td>
                      <td className="num">{numero(d.cantidad)}</td><td className="num">{pesos(d.precioUnitario)}</td>
                      <td className="num">{pesos(d.subtotal)}</td>
                    </tr>
                  ))}
                </tbody>
                <tfoot><tr><td colSpan={4}>Total</td><td className="num">{pesos(nota.total)}</td></tr></tfoot>
              </table>
            </div>
            <Aviso error={error} />
            {esAdmin && nota.estado === 'Pendiente' && (
              <div className="formulario-pie">
                <button type="button" className="btn peligro" disabled={cambiando} onClick={() => cambiarEstado('Anulada')}>Anular nota</button>
                <button type="button" className="btn btn-principal" disabled={cambiando} onClick={() => cambiarEstado('Pagada')}>
                  {cambiando ? 'Guardando…' : 'Marcar como pagada'}
                </button>
              </div>
            )}
          </>
        )}
      </div>
    </Dialogo>
  )
}
