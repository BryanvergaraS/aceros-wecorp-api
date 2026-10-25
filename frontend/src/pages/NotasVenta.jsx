import { useState } from 'react'
import { Link, useLocation } from 'react-router'
import { api, qs } from '../api/cliente'
import { fechaHora, numero, pesos } from '../api/formato'
import { Aviso, Dialogo, Encabezado, EstadoNota, Paginador } from '../components/Comunes'
import { useSesion } from '../context/Sesion'
import { useDatos } from '../hooks/useDatos'

const TAMANO = 20

export default function NotasVenta() {
  const { esAdmin } = useSesion()
  const { state } = useLocation()
  const [estado, setEstado] = useState('')
  const [pagina, setPagina] = useState(1)
  const [abierta, setAbierta] = useState(null)
  const [error, setError] = useState(null)
  const { datos, error: errorLista, cargando, recargar } =
    useDatos(`/notas-venta${qs({ estado, pagina, tamano: TAMANO })}`)

  async function cambiarEstado(nota, nuevo) {
    const pregunta = nuevo === 'Anulada'
      ? `¿Anular la nota ${nota.folio}? El stock vuelve al catálogo.`
      : `¿Marcar la nota ${nota.folio} como pagada?`
    if (!window.confirm(pregunta)) return
    try {
      const r = await api.patch(`/notas-venta/${nota.id}/estado`, { estado: nuevo })
      setAbierta(r)
      recargar()
    } catch (e) {
      setError(e)
    }
  }

  return (
    <section>
      <Encabezado titulo={esAdmin ? 'Notas de venta' : 'Mis notas de venta'}>
        <Link to="/notas/nueva" className="btn btn-principal">Nueva nota de venta</Link>
      </Encabezado>
      {state?.emitida && <p className="exito" role="status">Nota de venta {state.emitida} emitida.</p>}

      <div className="filtros">
        <select value={estado} onChange={(e) => { setEstado(e.target.value); setPagina(1) }} aria-label="Estado">
          <option value="">Todos los estados</option>
          <option>Pendiente</option>
          <option>Pagada</option>
          <option>Anulada</option>
        </select>
      </div>
      <Aviso error={error || errorLista} />

      <div className={`tabla-marco ${cargando ? 'actualizando' : ''}`}>
        <table className="tabla">
          <thead>
            <tr>
              <th>Folio</th><th>Fecha</th><th>Cliente</th>{esAdmin && <th>Vendedor</th>}
              <th className="num">Total neto</th><th>Estado</th><th><span className="oculto">Ver</span></th>
            </tr>
          </thead>
          <tbody>
            {datos?.items.map((n) => (
              <tr key={n.id}>
                <td><span className="sku">{n.folio}</span></td>
                <td>{fechaHora(n.fechaCreacion)}</td>
                <td>{n.clienteNombre}</td>
                {esAdmin && <td>{n.vendedor}</td>}
                <td className="num">{pesos(n.total)}</td>
                <td><EstadoNota estado={n.estado} /></td>
                <td className="acciones"><button type="button" className="btn-texto" onClick={() => setAbierta(n)}>Ver detalle</button></td>
              </tr>
            ))}
          </tbody>
        </table>
        {datos && datos.items.length === 0 && <p className="vacio">Todavía no hay notas de venta con este filtro.</p>}
      </div>
      {datos && <Paginador pagina={pagina} total={datos.total} tamano={TAMANO} onCambiar={setPagina} />}

      <Dialogo abierto={!!abierta} titulo={abierta ? `Nota de venta ${abierta.folio}` : ''} onCerrar={() => setAbierta(null)}>
        {abierta && (
          <div className="detalle-nota">
            <dl className="datos">
              <dt>Cliente</dt><dd>{abierta.clienteNombre}{abierta.clienteRut && `, RUT ${abierta.clienteRut}`}</dd>
              <dt>Vendedor</dt><dd>{abierta.vendedor}</dd>
              <dt>Fecha</dt><dd>{fechaHora(abierta.fechaCreacion)}</dd>
              <dt>Estado</dt><dd><EstadoNota estado={abierta.estado} /></dd>
              {abierta.observaciones && <><dt>Observaciones</dt><dd>{abierta.observaciones}</dd></>}
            </dl>
            <table className="tabla tabla-compacta">
              <thead><tr><th>SKU</th><th>Producto</th><th className="num">Cantidad</th><th className="num">Precio</th><th className="num">Subtotal</th></tr></thead>
              <tbody>
                {abierta.detalles.map((d) => (
                  <tr key={d.productoId}>
                    <td><span className="sku">{d.sku}</span></td><td>{d.nombre}</td>
                    <td className="num">{numero(d.cantidad)}</td><td className="num">{pesos(d.precioUnitario)}</td>
                    <td className="num">{pesos(d.subtotal)}</td>
                  </tr>
                ))}
              </tbody>
              <tfoot><tr><td colSpan={4}>Total neto</td><td className="num">{pesos(abierta.total)}</td></tr></tfoot>
            </table>
            {esAdmin && abierta.estado === 'Pendiente' && (
              <div className="formulario-pie">
                <button type="button" className="btn peligro" onClick={() => cambiarEstado(abierta, 'Anulada')}>Anular nota</button>
                <button type="button" className="btn btn-principal" onClick={() => cambiarEstado(abierta, 'Pagada')}>Marcar como pagada</button>
              </div>
            )}
          </div>
        )}
      </Dialogo>
    </section>
  )
}
