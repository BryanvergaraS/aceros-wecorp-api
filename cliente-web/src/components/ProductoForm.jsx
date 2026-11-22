import { useState } from 'react'
import { catalogo, errorDe } from '../api/cliente'
import { Aviso, Campo, Dialogo } from './Comunes'

const VACIO = {
  familiaId: '', sku: '', nombre: '', descripcion: '', unidadVenta: 'Tira',
  pesoKg: '', precioVenta: '', stock: 0, stockMinimo: 0, activo: true
}

// Crear o editar un producto (solo Administrador). Corresponde a ProductoGuardar del contrato.
export default function ProductoForm({ producto, familias, onCerrar, onGuardado }) {
  const nuevo = !producto.id
  const [f, setF] = useState(() => (nuevo ? VACIO : {
    familiaId: producto.familiaId,
    sku: producto.sku,
    nombre: producto.nombre,
    descripcion: producto.descripcion ?? '',
    unidadVenta: producto.unidadVenta,
    pesoKg: producto.pesoKg ?? '',
    precioVenta: producto.precioVenta,
    stock: producto.stock,
    stockMinimo: producto.stockMinimo,
    activo: producto.activo
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
      familiaId: Number(f.familiaId),
      sku: f.sku.trim(),
      nombre: f.nombre.trim(),
      descripcion: f.descripcion.trim() || null,
      unidadVenta: f.unidadVenta,
      pesoKg: f.pesoKg === '' ? null : Number(f.pesoKg),
      precioVenta: Number(f.precioVenta),
      stock: Number(f.stock),
      stockMinimo: Number(f.stockMinimo),
      activo: f.activo
    }
    try {
      if (nuevo) await catalogo.post('/productos', cuerpo)
      else await catalogo.put(`/productos/${producto.id}`, cuerpo)
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
          <Campo etiqueta="Precio (CLP)" error={errorDe(e, 'precioVenta')}>
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
          <input type="checkbox" checked={f.activo} onChange={set('activo')} /> Producto activo
        </label>

        {error && !e && <Aviso error={error} />}
        <div className="formulario-pie">
          <button type="button" className="btn" onClick={onCerrar}>Cancelar</button>
          <button className="btn btn-principal" disabled={guardando}>
            {guardando ? 'Guardando…' : nuevo ? 'Crear producto' : 'Guardar cambios'}
          </button>
        </div>
      </form>
    </Dialogo>
  )
}
