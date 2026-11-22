import { useMemo, useState } from 'react'
import { useNavigate } from 'react-router'
import { catalogo, qs, ventas } from '../api/cliente'
import { numero, pesos } from '../api/formato'
import { Aviso, Campo, Cargando, Encabezado } from '../components/Comunes'
import ClienteForm from '../components/ClienteForm'
import { useDatos, useDebounce } from '../hooks/useDatos'

// Emitir nota de venta (RF07, RF08). Los productos y precios vienen de la API de Catálogo;
// la nota se envía a la API de Ventas, que vuelve a tomar el precio vigente y descuenta el stock.
export default function NuevaNota() {
  const navegar = useNavigate()
  const [cliente, setCliente] = useState(null)
  const [observaciones, setObservaciones] = useState('')
  const [carro, setCarro] = useState([]) // [{ producto, cantidad }]
  const [error, setError] = useState(null)
  const [enviando, setEnviando] = useState(false)

  const total = useMemo(
    () => carro.reduce((s, l) => s + (Number(l.cantidad) || 0) * l.producto.precioVenta, 0), [carro])

  function agregar(p) {
    setCarro((c) => (c.some((l) => l.producto.id === p.id) ? c : [...c, { producto: p, cantidad: 1 }]))
  }
  const cambiarCantidad = (id, cantidad) =>
    setCarro((c) => c.map((l) => (l.producto.id === id ? { ...l, cantidad } : l)))
  const quitar = (id) => setCarro((c) => c.filter((l) => l.producto.id !== id))

  async function emitir(e) {
    e.preventDefault()
    if (!cliente) { setError(new Error('Selecciona un cliente.')); return }
    setEnviando(true)
    setError(null)
    try {
      const nota = await ventas.post('/notas-venta', {
        clienteId: cliente.id,
        observaciones: observaciones.trim() || null,
        lineas: carro.map((l) => ({ productoId: l.producto.id, cantidad: Number(l.cantidad) }))
      })
      navegar('/notas', { state: { emitida: nota.folio } })
    } catch (err) {
      setError(err)
      setEnviando(false)
    }
  }

  return (
    <section>
      <Encabezado titulo="Nueva nota de venta" />
      <div className="venta">
        <div className="venta-catalogo">
          <SelectorCliente cliente={cliente} onElegir={setCliente} />
          <BuscadorProductos enCarro={carro} onAgregar={agregar} />
        </div>

        <form className="venta-carro" onSubmit={emitir}>
          <h2>Detalle</h2>
          <p className="carro-cliente">
            {cliente ? <><strong>{cliente.nombre}</strong> · RUT {cliente.rut}</> : 'Sin cliente seleccionado'}
          </p>
          {carro.length === 0 && <p className="vacio-corto">Elige productos del catálogo para agregarlos.</p>}
          <ul className="carro">
            {carro.map(({ producto: p, cantidad }) => (
              <li key={p.id}>
                <div>
                  <span className="sku">{p.sku}</span>
                  <span className="carro-nombre">{p.nombre}</span>
                </div>
                <input type="number" min="1" max={p.stock} required value={cantidad}
                  onChange={(e) => cambiarCantidad(p.id, e.target.value)} aria-label={`Cantidad de ${p.nombre}`} />
                <span className="num">{pesos((Number(cantidad) || 0) * p.precioVenta)}</span>
                <button type="button" className="btn-texto peligro" onClick={() => quitar(p.id)}>Quitar</button>
              </li>
            ))}
          </ul>
          <p className="carro-total"><span>Total estimado</span><strong>{pesos(total)}</strong></p>
          <p className="texto-suave">El total final usa el precio vigente al momento de emitir.</p>

          <Campo etiqueta="Observaciones">
            <textarea rows={2} maxLength={300} value={observaciones}
              onChange={(e) => setObservaciones(e.target.value)} />
          </Campo>
          <Aviso error={error} />
          <button className="btn btn-principal" disabled={enviando || carro.length === 0 || !cliente}>
            {enviando ? 'Emitiendo…' : 'Emitir nota de venta'}
          </button>
        </form>
      </div>
    </section>
  )
}

function SelectorCliente({ cliente, onElegir }) {
  const [buscar, setBuscar] = useState('')
  const [creando, setCreando] = useState(false)
  const texto = useDebounce(buscar)
  // Solo se busca cuando hay texto, para no listar todos los clientes.
  const { datos, error, cargando } = useDatos(ventas, texto.trim() ? `/clientes${qs({ buscar: texto, tamano: 6 })}` : null)

  return (
    <fieldset className="bloque">
      <legend>1. Cliente</legend>
      {cliente ? (
        <div className="cliente-elegido">
          <span><strong>{cliente.nombre}</strong> · RUT {cliente.rut}</span>
          <button type="button" className="btn-texto" onClick={() => onElegir(null)}>Cambiar</button>
        </div>
      ) : (
        <>
          <div className="fila-busqueda">
            <input type="search" placeholder="Buscar cliente por RUT o nombre" value={buscar}
              onChange={(e) => setBuscar(e.target.value)} aria-label="Buscar cliente" />
            <button type="button" className="btn" onClick={() => setCreando(true)}>Nuevo cliente</button>
          </div>
          <Aviso error={error} />
          {texto.trim() && cargando && <Cargando texto="Buscando clientes…" />}
          {texto.trim() && datos && (
            <ul className="resultados">
              {datos.items.map((c) => (
                <li key={c.id}>
                  <button type="button" className="resultado resultado-simple" onClick={() => onElegir(c)}>
                    <span className="resultado-nombre">{c.nombre}</span>
                    <span className="texto-suave">RUT {c.rut}</span>
                  </button>
                </li>
              ))}
              {datos.items.length === 0 && <li className="vacio-corto">No hay clientes con ese RUT o nombre.</li>}
            </ul>
          )}
        </>
      )}
      {creando && (
        <ClienteForm cliente={{}} onCerrar={() => setCreando(false)}
          onGuardado={(c) => { setCreando(false); onElegir(c) }} />
      )}
    </fieldset>
  )
}

function BuscadorProductos({ enCarro, onAgregar }) {
  const [buscar, setBuscar] = useState('')
  const texto = useDebounce(buscar)
  const { datos, error, cargando } = useDatos(catalogo, `/productos${qs({ buscar: texto, tamano: 12 })}`)
  const ids = new Set(enCarro.map((l) => l.producto.id))

  return (
    <fieldset className="bloque">
      <legend>2. Productos</legend>
      <input type="search" placeholder="Buscar por nombre o SKU" value={buscar}
        onChange={(e) => setBuscar(e.target.value)} aria-label="Buscar producto" />
      <Aviso error={error} />
      {cargando && !datos && <Cargando />}
      <ul className={`resultados ${cargando ? 'actualizando' : ''}`}>
        {datos?.items.map((p) => (
          <li key={p.id}>
            <button type="button" className="resultado" disabled={!p.activo || p.stock === 0 || ids.has(p.id)}
              onClick={() => onAgregar(p)}>
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
    </fieldset>
  )
}
