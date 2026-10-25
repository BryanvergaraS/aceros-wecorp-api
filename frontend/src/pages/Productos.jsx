import { memo, useCallback, useEffect, useState } from 'react'
import { api, qs } from '../api/cliente'
import { pesos } from '../api/formato'
import { Aviso, BarraStock, Encabezado, Paginador } from '../components/Comunes'
import ProductoForm from '../components/ProductoForm'
import { useSesion } from '../context/Sesion'
import { useDatos, useDebounce } from '../hooks/useDatos'

const TAMANO = 20

const Fila = memo(function Fila({ p, esAdmin, onEditar, onEliminar }) {
  return (
    <tr className={p.estado ? '' : 'inactivo'}>
      <td><span className="sku">{p.sku}</span></td>
      <td>
        {p.nombre}
        {!p.estado && <span className="etiqueta-inactivo">Inactivo</span>}
      </td>
      <td>{p.familiaNombre}</td>
      <td>{p.unidadVenta}</td>
      <td className="num">{pesos(p.precioVenta)}</td>
      <td><BarraStock stock={p.stock} minimo={p.stockMinimo} /></td>
      {esAdmin && (
        <td className="acciones">
          <button type="button" className="btn-texto" onClick={() => onEditar(p)}>Editar</button>
          <button type="button" className="btn-texto peligro" onClick={() => onEliminar(p)}>Eliminar</button>
        </td>
      )}
    </tr>
  )
})

export default function Productos() {
  const { esAdmin, familias, cargarFamilias } = useSesion()
  const [buscar, setBuscar] = useState('')
  const [familiaId, setFamiliaId] = useState('')
  const [pagina, setPagina] = useState(1)
  const [editando, setEditando] = useState(null) // null = cerrado, {} = nuevo, producto = editar
  const [error, setError] = useState(null)
  const texto = useDebounce(buscar)

  useEffect(() => { cargarFamilias().catch(setError) }, [cargarFamilias])
  useEffect(() => { setPagina(1) }, [texto, familiaId])

  const { datos, error: errorLista, cargando, recargar } =
    useDatos(`/productos${qs({ buscar: texto, familiaId, pagina, tamano: TAMANO })}`)

  const onEditar = useCallback((p) => setEditando(p), [])
  const onEliminar = useCallback(async (p) => {
    if (!window.confirm(`¿Eliminar ${p.nombre}?`)) return
    try {
      await api.del(`/productos/${p.id}`)
      recargar()
    } catch (e) {
      setError(e)
    }
  }, [recargar])

  return (
    <section>
      <Encabezado titulo="Catálogo">
        {esAdmin && <button type="button" className="btn btn-principal" onClick={() => setEditando({})}>Nuevo producto</button>}
      </Encabezado>

      <div className="filtros">
        <input type="search" placeholder="Buscar por nombre o SKU" value={buscar}
          onChange={(e) => setBuscar(e.target.value)} aria-label="Buscar producto" />
        <select value={familiaId} onChange={(e) => setFamiliaId(e.target.value)} aria-label="Familia">
          <option value="">Todas las familias</option>
          {familias?.map((f) => <option key={f.id} value={f.id}>{f.nombre}</option>)}
        </select>
      </div>

      <Aviso error={error || errorLista} />

      <div className={`tabla-marco ${cargando ? 'actualizando' : ''}`}>
        <table className="tabla">
          <thead>
            <tr>
              <th>SKU</th><th>Producto</th><th>Familia</th><th>Venta por</th>
              <th className="num">Precio neto</th><th>Stock</th>
              {esAdmin && <th><span className="oculto">Acciones</span></th>}
            </tr>
          </thead>
          <tbody>
            {datos?.items.map((p) => (
              <Fila key={p.id} p={p} esAdmin={esAdmin} onEditar={onEditar} onEliminar={onEliminar} />
            ))}
          </tbody>
        </table>
        {datos && datos.items.length === 0 && (
          <p className="vacio">No hay productos que coincidan. Pruebe con otro nombre o SKU.</p>
        )}
      </div>

      {datos && <Paginador pagina={pagina} total={datos.total} tamano={TAMANO} onCambiar={setPagina} />}

      {editando && (
        <ProductoForm producto={editando} familias={familias ?? []}
          onCerrar={() => setEditando(null)}
          onGuardado={() => { setEditando(null); recargar() }} />
      )}
    </section>
  )
}
