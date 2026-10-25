import { useMemo, useState } from 'react'
import { useNavigate } from 'react-router'
import { api, qs } from '../api/cliente'
import { numero, pesos } from '../api/formato'
import { Aviso, Campo, Encabezado } from '../components/Comunes'
import { useDatos, useDebounce } from '../hooks/useDatos'

export default function NuevaNota() {
  const navegar = useNavigate()
  const [buscar, setBuscar] = useState('')
  const texto = useDebounce(buscar)
  const [carro, setCarro] = useState([]) // [{ producto, cantidad }]
  const [cliente, setCliente] = useState({ clienteNombre: '', clienteRut: '', observaciones: '' })
  const [error, setError] = useState(null)
  const [enviando, setEnviando] = useState(false)

  const { datos, error: errorLista } =
    useDatos(`/productos${qs({ buscar: texto, soloActivos: true, tamano: 12 })}`)

  const total = useMemo(() => carro.reduce((s, l) => s + l.cantidad * l.producto.precioVenta, 0), [carro])

  function agregar(p) {
    setCarro((c) => {
      const existe = c.find((l) => l.producto.id === p.id)
      if (existe) return c.map((l) => (l === existe ? { ...l, cantidad: Math.min(l.cantidad + 1, p.stock) } : l))
      return [...c, { producto: p, cantidad: 1 }]
    })
  }

  const cambiarCantidad = (id, cantidad) =>
    setCarro((c) => c.map((l) => (l.producto.id === id ? { ...l, cantidad } : l)))
  const quitar = (id) => setCarro((c) => c.filter((l) => l.producto.id !== id))

  async function emitir(e) {
    e.preventDefault()
    setEnviando(true)
    setError(null)
    try {
      const nota = await api.post('/notas-venta', {
        clienteNombre: cliente.clienteNombre,
        clienteRut: cliente.clienteRut || null,
        observaciones: cliente.observaciones || null,
        detalles: carro.map((l) => ({ productoId: l.producto.id, cantidad: Number(l.cantidad) }))
      })
      navegar('/notas', { state: { emitida: nota.folio } })
    } catch (err) {
      setError(err)
      setEnviando(false)
    }
  }

  const enCarro = new Set(carro.map((l) => l.producto.id))
  return (
    <section>
      <Encabezado titulo="Nueva nota de venta" />
      <div className="venta">
        <div className="venta-catalogo">
          <input type="search" placeholder="Buscar por nombre o SKU" value={buscar}
            onChange={(e) => setBuscar(e.target.value)} aria-label="Buscar producto" />
          <Aviso error={errorLista} />
          <ul className="resultados">
            {datos?.items.map((p) => (
              <li key={p.id}>
                <button type="button" className="resultado" disabled={p.stock === 0 || enCarro.has(p.id)}
                  onClick={() => agregar(p)}>
                  <span className="sku">{p.sku}</span>
                  <span className="resultado-nombre">{p.nombre}</span>
                  <span className="resultado-datos">
                    <span>{pesos(p.precioVenta)} por {p.unidadVenta.toLowerCase()}</span>
                    <span className={p.stock <= p.stockMinimo ? 'texto-alerta' : ''}>
                      {p.stock === 0 ? 'Sin stock' : `${numero(p.stock)} en stock`}
                    </span>
                  </span>
                </button>
              </li>
            ))}
          </ul>
          {datos && datos.items.length === 0 && <p className="vacio">No hay productos que coincidan.</p>}
        </div>

        <form className="venta-carro" onSubmit={emitir}>
          <h2>Detalle</h2>
          {carro.length === 0 && <p className="vacio-corto">Elija productos del catálogo para agregarlos.</p>}
          <ul className="carro">
            {carro.map(({ producto: p, cantidad }) => (
              <li key={p.id}>
                <div>
                  <span className="sku">{p.sku}</span>
                  <span className="carro-nombre">{p.nombre}</span>
                </div>
                <input type="number" min="1" max={p.stock} required value={cantidad}
                  onChange={(e) => cambiarCantidad(p.id, e.target.value)} aria-label={`Cantidad de ${p.nombre}`} />
                <span className="num">{pesos(cantidad * p.precioVenta)}</span>
                <button type="button" className="btn-texto peligro" onClick={() => quitar(p.id)}>Quitar</button>
              </li>
            ))}
          </ul>
          <p className="carro-total"><span>Total neto</span><strong>{pesos(total)}</strong></p>

          <Campo etiqueta="Cliente">
            <input required maxLength={150} value={cliente.clienteNombre}
              onChange={(e) => setCliente({ ...cliente, clienteNombre: e.target.value })} />
          </Campo>
          <Campo etiqueta="RUT del cliente (opcional)">
            <input placeholder="12.345.678-9" value={cliente.clienteRut}
              onChange={(e) => setCliente({ ...cliente, clienteRut: e.target.value })} />
          </Campo>
          <Campo etiqueta="Observaciones">
            <textarea rows={2} maxLength={300} value={cliente.observaciones}
              onChange={(e) => setCliente({ ...cliente, observaciones: e.target.value })} />
          </Campo>
          <Aviso error={error} />
          <button className="btn btn-principal" disabled={enviando || carro.length === 0}>
            {enviando ? 'Emitiendo…' : 'Emitir nota de venta'}
          </button>
        </form>
      </div>
    </section>
  )
}
