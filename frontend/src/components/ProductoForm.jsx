import { useEffect, useState } from 'react'
import { api } from '../api/cliente'
import { pesos } from '../api/formato'
import { Aviso, Campo, Dialogo } from './Comunes'

const VACIO = {
  familiaId: '', sku: '', nombre: '', descripcion: '', unidadVenta: 'Tira',
  pesoKg: '', precioVenta: '', stock: 0, stockMinimo: 0, estado: true
}

// Une los errores de validación del servidor (claves como "Sku" o "$.sku") con el nombre del campo.
function errorDe(errores, campo) {
  if (!errores) return null
  const clave = Object.keys(errores).find((k) => k.replace('$.', '').toLowerCase() === campo.toLowerCase())
  return clave ? errores[clave][0] : null
}

export default function ProductoForm({ producto, familias, onCerrar, onGuardado }) {
  const nuevo = !producto.id
  const [f, setF] = useState(() => (nuevo ? VACIO : {
    ...VACIO, ...producto, descripcion: producto.descripcion ?? '', pesoKg: producto.pesoKg ?? ''
  }))
  const [error, setError] = useState(null)
  const [guardando, setGuardando] = useState(false)

  const set = (campo) => (e) => {
    const v = e.target.type === 'checkbox' ? e.target.checked : e.target.value
    setF((x) => ({ ...x, [campo]: v }))
  }

  async function guardar(e) {
    e.preventDefault()
    setGuardando(true)
    setError(null)
    const cuerpo = {
      ...f,
      familiaId: Number(f.familiaId),
      pesoKg: f.pesoKg === '' ? null : Number(f.pesoKg),
      precioVenta: Number(f.precioVenta),
      stock: Number(f.stock),
      stockMinimo: Number(f.stockMinimo)
    }
    try {
      if (nuevo) await api.post('/productos', cuerpo)
      else await api.put(`/productos/${producto.id}`, cuerpo)
      onGuardado()
    } catch (err) {
      setError(err)
      setGuardando(false)
    }
  }

  const e = error?.errores
  return (
    <Dialogo abierto titulo={nuevo ? 'Nuevo producto' : `Editar ${producto.sku}`} onCerrar={onCerrar}>
      <form className="formulario" onSubmit={guardar}>
        <div className="rejilla">
          <Campo etiqueta="SKU" error={errorDe(e, 'sku')}>
            <input required maxLength={50} value={f.sku} onChange={set('sku')} className="sku-input" />
          </Campo>
          <Campo etiqueta="Familia" error={errorDe(e, 'familiaId')}>
            <select required value={f.familiaId} onChange={set('familiaId')}>
              <option value="">Seleccione</option>
              {familias.map((x) => <option key={x.id} value={x.id}>{x.nombre}</option>)}
            </select>
          </Campo>
        </div>
        <Campo etiqueta="Nombre" error={errorDe(e, 'nombre')}>
          <input required maxLength={150} value={f.nombre} onChange={set('nombre')} />
        </Campo>
        <Campo etiqueta="Descripción" error={errorDe(e, 'descripcion')}>
          <textarea maxLength={300} rows={2} value={f.descripcion} onChange={set('descripcion')} />
        </Campo>
        <div className="rejilla">
          <Campo etiqueta="Se vende por" error={errorDe(e, 'unidadVenta')}>
            <select value={f.unidadVenta} onChange={set('unidadVenta')}>
              <option value="Tira">Tira</option>
              <option value="Unidad">Unidad</option>
            </select>
          </Campo>
          <Campo etiqueta="Peso (kg)" error={errorDe(e, 'pesoKg')}>
            <input type="number" min="0" step="0.001" value={f.pesoKg} onChange={set('pesoKg')} />
          </Campo>
          <Campo etiqueta="Precio neto (CLP)" error={errorDe(e, 'precioVenta')}>
            <input type="number" min="1" step="1" required value={f.precioVenta} onChange={set('precioVenta')} />
          </Campo>
          <Campo etiqueta="Stock" error={errorDe(e, 'stock')}>
            <input type="number" min="0" step="1" required value={f.stock} onChange={set('stock')} />
          </Campo>
          <Campo etiqueta="Stock mínimo" error={errorDe(e, 'stockMinimo')}>
            <input type="number" min="0" step="1" required value={f.stockMinimo} onChange={set('stockMinimo')} />
          </Campo>
        </div>
        <label className="check">
          <input type="checkbox" checked={f.estado} onChange={set('estado')} /> Producto activo
        </label>

        {error && !e && <Aviso error={error} />}
        <div className="formulario-pie">
          <button type="button" className="btn" onClick={onCerrar}>Cancelar</button>
          <button className="btn btn-principal" disabled={guardando}>
            {guardando ? 'Guardando…' : nuevo ? 'Crear producto' : 'Guardar cambios'}
          </button>
        </div>
      </form>

      {!nuevo && <ProveedoresProducto productoId={producto.id} />}
    </Dialogo>
  )
}

// Tabla ProductoProveedores: qué proveedores venden este producto y a qué costo.
function ProveedoresProducto({ productoId }) {
  const [asignados, setAsignados] = useState([])
  const [todos, setTodos] = useState([])
  const [proveedorId, setProveedorId] = useState('')
  const [costo, setCosto] = useState('')
  const [error, setError] = useState(null)

  const cargar = () => api.get(`/productos/${productoId}/proveedores`).then(setAsignados).catch(setError)

  useEffect(() => {
    cargar()
    api.get('/proveedores').then(setTodos).catch(setError)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [productoId])

  async function asignar(e) {
    e.preventDefault()
    setError(null)
    try {
      await api.post(`/productos/${productoId}/proveedores`, {
        proveedorId: Number(proveedorId),
        costoCompra: costo === '' ? null : Number(costo)
      })
      setProveedorId('')
      setCosto('')
      cargar()
    } catch (err) {
      setError(err)
    }
  }

  async function quitar(id) {
    try {
      await api.del(`/productos/${productoId}/proveedores/${id}`)
      cargar()
    } catch (err) {
      setError(err)
    }
  }

  const disponibles = todos.filter((p) => !asignados.some((a) => a.proveedorId === p.id))
  return (
    <section className="subseccion">
      <h3>Proveedores</h3>
      {asignados.length === 0 && <p className="vacio-corto">Sin proveedores asignados.</p>}
      <ul className="lista-simple">
        {asignados.map((a) => (
          <li key={a.proveedorId}>
            <span>{a.razonSocial}</span>
            <span className="num">{a.costoCompra == null ? 'Sin costo' : pesos(a.costoCompra)}</span>
            <button type="button" className="btn-texto peligro" onClick={() => quitar(a.proveedorId)}>Quitar</button>
          </li>
        ))}
      </ul>
      {disponibles.length > 0 && (
        <form className="fila-form" onSubmit={asignar}>
          <select required value={proveedorId} onChange={(e) => setProveedorId(e.target.value)} aria-label="Proveedor">
            <option value="">Agregar proveedor</option>
            {disponibles.map((p) => <option key={p.id} value={p.id}>{p.razonSocial}</option>)}
          </select>
          <input type="number" min="0" placeholder="Costo de compra" value={costo}
            onChange={(e) => setCosto(e.target.value)} aria-label="Costo de compra" />
          <button className="btn">Asignar</button>
        </form>
      )}
      <Aviso error={error} />
    </section>
  )
}
